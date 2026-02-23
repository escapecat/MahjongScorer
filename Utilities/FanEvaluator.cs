namespace MahjongScorer.Utilities;

public static class FanEvaluator
{
    public static FanEvaluationResult Evaluate(FanEvaluationContext context)
    {
        var counts = context.Counts;
        var hasOpenMeld = context.HasChi || context.HasPeng || context.HasMingKong;

        // Compute hand-only counts (total counts minus locked meld tiles) once.
        // HandDecomposer does the same subtraction internally; we mirror it here
        // so that WaitAnalyzer (called from AddSituationalFans) operates only on
        // hand tiles and cannot form sequences from locked meld tiles.
        var handOnlyCounts = ComputeHandOnlyCounts(counts, context.Melds);

        // Collect all candidate results and pick the highest-scoring one
        var candidates = new List<FanEvaluationResult>();

        // ── Special hands that don't follow standard decomposition ──

        // 十三幺
        if (HandPatternDetector.IsThirteenOrphans(counts))
        {
            var result = new FanEvaluationResult();
            result.DecompositionDescription = "十三幺（特殊牌型）";
            result.DecompositionTileGroups = BuildThirteenOrphansTileGroups(counts);
            AddFan(result, "十三幺 88番", "手牌为19万19筒19条+东南西北中发白+其中一张作将");
            AddSituationalFans(result, context, hasOpenMeld, handOnlyCounts);
            ApplyExclusions(result);
            FinalizeResult(result);
            SetWinningGroupForSpecialHand(result, context.WinningTile);
            candidates.Add(result);
        }

        // 七星不靠
        if (HandPatternDetector.IsSevenStarNotConnected(counts))
        {
            var result = new FanEvaluationResult();
            result.DecompositionDescription = "七星不靠（特殊牌型）";
            result.DecompositionTileGroups = BuildSingleTileGroups(counts);
            AddFan(result, "七星不靠 24番", "手牌含东南西北中发白各一张，加上三种花色互不相连的7张序数牌");
            AddFan(result, "全不靠 12番", "由七星不靠包含");
            AddSituationalFans(result, context, hasOpenMeld, handOnlyCounts);
            ApplyExclusions(result);
            FinalizeResult(result);
            SetWinningGroupForSpecialHand(result, context.WinningTile);
            candidates.Add(result);
        }
        else if (HandPatternDetector.IsAllNotConnected(counts))
        {
            var result = new FanEvaluationResult();
            result.DecompositionDescription = "全不靠（特殊牌型）";
            result.DecompositionTileGroups = BuildSingleTileGroups(counts);
            AddFan(result, "全不靠 12番", "手中所有牌互不相连，含至少5种字牌");
            AddSituationalFans(result, context, hasOpenMeld, handOnlyCounts);
            ApplyExclusions(result);
            FinalizeResult(result);
            SetWinningGroupForSpecialHand(result, context.WinningTile);
            candidates.Add(result);
        }

        // 七对 / 连七对 (7 pairs — not standard decomposition)
        if (MahjongHandEvaluator.IsSevenPairs(counts) && !hasOpenMeld && context.Melds is null or { Count: 0 })
        {
            var result = new FanEvaluationResult();
            result.DecompositionDescription = "七对（7个对子）";
            result.DecompositionTileGroups = BuildSevenPairsTileGroups(counts);
            AddFan(result, "七对 24番", "手牌由7个对子组成");

            if (HandPatternDetector.IsSevenShiftedPairs(counts))
                AddFan(result, "连七对 88番", "7个对子为同一花色且序数相连");

            // 四归一: 4 tiles of same kind (counted as 2 pairs in 七对)
            var fourOfAKindCount = HandPatternDetector.CountFourOfAKindNotKong(counts);
            for (var i = 0; i < fourOfAKindCount; i++)
                AddFan(result, "四归一 2番", "4张相同的牌在手中（未声明为杠）");

            AddTilePropertyFans(result, counts, context);
            AddSituationalFans(result, context, hasOpenMeld, handOnlyCounts);
            ApplyExclusions(result);
            FinalizeResult(result);
            SetWinningGroupForSpecialHand(result, context.WinningTile);
            candidates.Add(result);
        }

        // 组合龙: 147m+258p+369s (or permutations) + 1 meld + 1 pair
        // This is a special pattern where 9 tiles form a composed dragon
        // and the remaining 5 tiles form 1 meld + 1 pair.
        if (HandPatternDetector.HasComposedDragon(counts))
        {
            var dragonResult = EvaluateComposedDragon(context, counts, handOnlyCounts);
            if (dragonResult != null)
                candidates.Add(dragonResult);
        }

        // ── Standard decompositions ──
        var decompositions = HandDecomposer.Decompose(
            counts, context.Melds,
            context.ChiCount, context.PengCount,
            context.MingKongCount, context.AnKongCount);

        foreach (var decomp in decompositions)
        {
            var result = EvaluateDecomposition(context, counts, decomp, handOnlyCounts);
            candidates.Add(result);
        }

        if (candidates.Count == 0)
            return new FanEvaluationResult { TotalFan = 0 };

        // Pick the highest-scoring result
        return candidates.OrderByDescending(r => r.TotalFan).First();
    }

    /// <summary>
    /// Compute counts for hand tiles only (total counts minus locked melds).
    /// For kongs, only 3 tiles are present in the evaluation counts, so we cap
    /// the subtraction at 3 per meld.
    /// </summary>
    private static int[] ComputeHandOnlyCounts(int[] counts, IReadOnlyList<IReadOnlyList<string>>? melds)
    {
        var handCounts = counts.ToArray();
        if (melds is null) return handCounts;

        foreach (var meld in melds)
        {
            var subtracted = 0;
            foreach (var tile in meld)
            {
                if (subtracted >= 3) break;
                if (TileConstants.TileIndexMap.TryGetValue(tile, out var ti) && handCounts[ti] > 0)
                {
                    handCounts[ti]--;
                    subtracted++;
                }
            }
        }

        return handCounts;
    }

    private static FanEvaluationResult EvaluateDecomposition(
        FanEvaluationContext context, int[] counts, HandDecomposition decomp, int[] handOnlyCounts)
    {
        var result = new FanEvaluationResult();
        SetDecompositionInfo(result, decomp);
        var hasOpenMeld = context.HasChi || context.HasPeng || context.HasMingKong;
        var kongCount = context.MingKongCount + context.AnKongCount;
        var decompInfo = DecompDesc(decomp);

        // ── 88番 special patterns (tile-property based) ──

        if (HandPatternDetector.IsAllGreen(counts))
            AddFan(result, "绿一色 88番", "手牌全部由23468条和发字组成");

        if (HandPatternDetector.IsNineGates(counts, hasOpenMeld))
            AddFan(result, "九莲宝灯 88番", "同一花色按1112345678999组成的门清听牌");

        // ── Tile-property fans ──
        AddTilePropertyFans(result, counts, context);

        // ── Decomposition-based fans ──
        AddDecompositionFans(result, decomp, counts, context, decompInfo);

        // ── Kong fans ──
        AddKongFans(result, kongCount, context);

        // ── Concealed triplet fans ──
        AddConcealedTripletFans(result, decomp, context);

        // ── Wind / Dragon patterns ──
        AddWindDragonPatternFans(result, decomp, counts);

        // ── Sequence/Triplet pattern fans (from meld list) ──
        AddMeldPatternFans(result, decomp);

        // ── Honor fans ──
        AddHonorFans(result, context.SeatWind, context.RoundWind, decomp);

        // ── Situational fans ──
        AddSituationalFans(result, context, hasOpenMeld, handOnlyCounts);

        ApplyExclusions(result);
        FinalizeResult(result);
        SetDecompositionInfo(result, decomp);

        // Compute which group the winning tile belongs to (skip locked melds)
        var lockedCount = context.ChiCount + context.PengCount + context.MingKongCount + context.AnKongCount;
        result.WinningTileGroupIndex = ComputeWinningTileGroupIndex(decomp, context.WinningTile, lockedCount);

        return result;
    }

    /// <summary>
    /// Fans that depend only on which tile indices are present (not decomposition).
    /// </summary>
    private static void AddTilePropertyFans(FanEvaluationResult result, int[] counts, FanEvaluationContext context)
    {
        // 字一色 / 清幺九 / 混幺九
        if (HandPatternDetector.IsAllHonors(counts))
            AddFan(result, "字一色 64番", "手牌全部由字牌（风牌+箭牌）组成");
        else if (HandPatternDetector.IsAllTerminals(counts))
            AddFan(result, "清幺九 64番", "手牌全部由序数牌的1和9组成");
        else if (HandPatternDetector.IsAllTerminalsOrHonors(counts))
            AddFan(result, "混幺九 32番", "手牌全部由幺九牌（1、9、字牌）组成");

        var pureSuit = HandPatternDetector.IsPureSuit(counts);
        var halfFlush = HandPatternDetector.IsHalfFlush(counts);
        var allBig = HandPatternDetector.IsAllBig(counts);
        var allMiddle = HandPatternDetector.IsAllMiddle(counts);
        var allSmall = HandPatternDetector.IsAllSmall(counts);

        if (pureSuit)
        {
            var suitIdx = Enumerable.Range(0, 3).First(s => Enumerable.Range(s * 9, 9).Any(i => counts[i] > 0));
            AddFan(result, "清一色 24番", $"手牌全部为{SuitName(suitIdx)}");
        }
        else if (halfFlush)
            AddFan(result, "混一色 6番", "手牌由一种花色的序数牌加字牌组成");

        if (allBig)
            AddFan(result, "全大 24番", "手牌全部由789的序数牌组成");
        else if (allMiddle)
            AddFan(result, "全中 24番", "手牌全部由456的序数牌组成");
        else if (allSmall)
            AddFan(result, "全小 24番", "手牌全部由123的序数牌组成");

        if (!allBig && HandPatternDetector.IsGreaterThanFive(counts))
            AddFan(result, "大于五 12番", "手牌全部由序数牌6-9组成");
        if (!allSmall && HandPatternDetector.IsLessThanFive(counts))
            AddFan(result, "小于五 12番", "手牌全部由序数牌1-4组成");

        if (HandPatternDetector.IsAllSimples(counts) && !allMiddle)
            AddFan(result, "断幺 2番", "手牌不含1、9和字牌");

        if (HandPatternDetector.IsNoHonors(counts) && !allBig && !allMiddle && !allSmall)
            AddFan(result, "无字 1番", "手牌不含字牌");

        if (HandPatternDetector.IsMissingOneSuit(counts) && !pureSuit && !halfFlush && HandPatternDetector.GetWindTripletCount(counts) != 3)
            AddFan(result, "缺一门 1番", "手牌缺少万/筒/条中的一种");

        if (HandPatternDetector.IsAllEvenTriplets(counts))
            AddFan(result, "全双刻 24番", "4副刻子均为序数偶数牌（2、4、6、8），将牌也为偶数");

        if (HandPatternDetector.HasFiveGates(counts))
            AddFan(result, "五门齐 6番", "手牌包含万、筒、条、风牌、箭牌五种类别");

        if (HandPatternDetector.IsAllGreen(counts) && !result.FanNames.Contains("绿一色 88番"))
            AddFan(result, "绿一色 88番", "手牌全部由23468条和发字组成");

        if (HandPatternDetector.IsPushedDownHand(counts))
            AddFan(result, "推不倒 8番", "手牌全部由上下对称的牌组成（1234589筒、2459条、白）");
    }

    /// <summary>
    /// Fans that depend on the specific decomposition (which melds were formed).
    /// </summary>
    private static void AddDecompositionFans(
        FanEvaluationResult result, HandDecomposition decomp, int[] counts,
        FanEvaluationContext context, string decompInfo = "")
    {
        var kongCount = context.MingKongCount + context.AnKongCount;

        // 碰碰和: all 4 melds are triplets/kongs
        if (decomp.Melds.All(m => m.Kind is MeldKind.Triplet or MeldKind.Kong))
            AddFan(result, "碰碰和 6番", $"4副面子均为刻子/杠: {decompInfo}");

        // 平和: all 4 melds are sequences, pair is a number tile (not wind/dragon)
        if (decomp.Melds.All(m => m.Kind == MeldKind.Sequence)
            && decomp.PairIndex < 27)
            AddFan(result, "平和 2番", $"4副面子均为顺子，将牌为序数牌: {decompInfo}");

        // 四归一: 4 tiles of same kind in hand but not declared as kong
        var fourOfAKindCount = HandPatternDetector.CountFourOfAKindNotKong(counts);
        for (var i = 0; i < fourOfAKindCount; i++)
            AddFan(result, "四归一 2番", "4张相同的牌在手中（未声明为杠）");

        // 全带五: every meld and pair contains a 5
        if (IsAllWithFive(decomp))
            AddFan(result, "全带五 16番", $"每副面子和将牌都包含5: {decompInfo}");

        // 全带幺: every meld and pair contains a terminal or honor
        if (IsAllWithTerminals(decomp))
            AddFan(result, "全带幺 4番", $"每副面子和将牌都包含幺九牌: {decompInfo}");

        // 幺九刻: terminal/honor triplets excluding seat wind, round wind, and dragons
        var yaojiuCount = CountYaojiuTriplets(decomp, context.SeatWind, context.RoundWind);
        for (var t = 0; t < yaojiuCount; t++)
            AddFan(result, "幺九刻 1番", "含幺九牌或风牌的刻子（不含圈风、门风、箭牌刻）");
    }

    private static void AddKongFans(FanEvaluationResult result, int kongCount, FanEvaluationContext context)
    {
        if (kongCount >= 4)
            AddFan(result, "四杠 88番", $"4个杠（{context.MingKongCount}明杠+{context.AnKongCount}暗杠）");
        else if (kongCount >= 3)
            AddFan(result, "三杠 32番", $"3个杠（{context.MingKongCount}明杠+{context.AnKongCount}暗杠）");

        if (context.AnKongCount >= 2)
            AddFan(result, "双暗杠 6番", $"有{context.AnKongCount}个暗杠");
        if (context.MingKongCount >= 2)
            AddFan(result, "双明杠 4番", $"有{context.MingKongCount}个明杠");
        if (context.AnKongCount >= 1)
            AddFan(result, "暗杠 2番", "有暗杠");
        if (context.MingKongCount >= 1)
            AddFan(result, "明杠 1番", "有明杠");
    }

    private static void AddConcealedTripletFans(
        FanEvaluationResult result, HandDecomposition decomp, FanEvaluationContext context)
    {
        // Count concealed triplets: triplets in the decomposition that are NOT open
        var concealedTriplets = 0;
        for (var i = 0; i < decomp.Melds.Count; i++)
        {
            if (decomp.Melds[i].Kind is MeldKind.Triplet or MeldKind.Kong)
            {
                if (!decomp.IsOpen[i])
                {
                    concealedTriplets++;
                }
            }
        }

        // If not self-draw, the winning tile might have completed a concealed triplet,
        // making it no longer concealed
        if (!context.IsSelfDraw && !string.IsNullOrWhiteSpace(context.WinningTile)
            && TileConstants.TileIndexMap.TryGetValue(context.WinningTile, out var winIndex))
        {
            for (var i = 0; i < decomp.Melds.Count; i++)
            {
                if (!decomp.IsOpen[i]
                    && decomp.Melds[i].Kind == MeldKind.Triplet
                    && decomp.Melds[i].TileIndex == winIndex)
                {
                    concealedTriplets--;
                    break;
                }
            }
        }

        concealedTriplets = Math.Max(0, concealedTriplets);

        if (concealedTriplets >= 4)
            AddFan(result, "四暗刻 64番", $"4副刻子均为暗刻（不含吃/碰/明杠）");
        else if (concealedTriplets >= 3)
            AddFan(result, "三暗刻 16番", $"有{concealedTriplets}副暗刻");
        else if (concealedTriplets >= 2)
            AddFan(result, "双暗刻 2番", $"有{concealedTriplets}副暗刻");
    }

    private static void AddWindDragonPatternFans(
        FanEvaluationResult result, HandDecomposition decomp, int[] counts)
    {
        var windTriplets = decomp.Melds.Count(m =>
            m.Kind is MeldKind.Triplet or MeldKind.Kong && m.TileIndex is >= 27 and <= 30);
        var dragonTriplets = decomp.Melds.Count(m =>
            m.Kind is MeldKind.Triplet or MeldKind.Kong && m.TileIndex is >= 31 and <= 33);

        // 大四喜 / 小四喜
        if (windTriplets == 4)
            AddFan(result, "大四喜 88番", "4副刻子均为风牌（东南西北）");
        else if (windTriplets == 3 && decomp.PairIndex is >= 27 and <= 30)
            AddFan(result, "小四喜 64番", $"3副风牌刻子+{TileName(decomp.PairIndex)}作将");

        // 大三元 / 小三元
        if (dragonTriplets == 3)
            AddFan(result, "大三元 88番", "3副箭牌刻子（中发白）");
        else if (dragonTriplets == 2 && decomp.PairIndex is >= 31 and <= 33)
            AddFan(result, "小三元 64番", $"2副箭牌刻子+{TileName(decomp.PairIndex)}作将");

        // 三风刻
        if (windTriplets == 3)
            AddFan(result, "三风刻 12番", "有3副风牌刻子");

        // 双箭刻
        if (dragonTriplets == 2)
            AddFan(result, "双箭刻 6番", "有2副箭牌刻子");

        // 同刻: same-rank triplets across suits
        var sameNumberTriplets = GetSameNumberTripletCounts(decomp);
        if (sameNumberTriplets.Any(c => c == 3))
        {
            var rank = Array.FindIndex(sameNumberTriplets, c => c == 3);
            AddFan(result, "三同刻 16番", $"3副{rank + 1}的刻子（万筒条各一）");
        }
        else if (sameNumberTriplets.Any(c => c == 2))
        {
            var rank = Array.FindIndex(sameNumberTriplets, c => c == 2);
            AddFan(result, "双同刻 2番", $"2副{rank + 1}的刻子");
        }
    }

    /// <summary>
    /// Sequence and triplet pattern fans derived from the meld list.
    /// </summary>
    private static void AddMeldPatternFans(FanEvaluationResult result, HandDecomposition decomp)
    {
        var sequences = decomp.Sequences.Select(m => m.TileIndex).ToList();
        var triplets = decomp.Triplets.Select(m => m.TileIndex).ToList();

        // ── Sequence patterns ──

        // Group sequences by (suit, startRank)
        var seqBySuit = new List<int>[3];
        for (var s = 0; s < 3; s++) seqBySuit[s] = [];
        foreach (var idx in sequences)
        {
            if (idx < 27) seqBySuit[idx / 9].Add(idx % 9);
        }

        // 一色四同顺 / 一色三同顺 / 一般高
        var maxSameSeq = 0;
        var maxSameSeqSuit = 0;
        var maxSameSeqRank = 0;
        foreach (var (suit, suitIdx) in seqBySuit.Select((s, i) => (s, i)))
        {
            foreach (var group in suit.GroupBy(r => r))
            {
                if (group.Count() > maxSameSeq)
                {
                    maxSameSeq = group.Count();
                    maxSameSeqSuit = suitIdx;
                    maxSameSeqRank = group.Key;
                }
            }
        }

        if (maxSameSeq >= 4)
            AddFan(result, "一色四同顺 48番", $"4副{SuitName(maxSameSeqSuit)}{maxSameSeqRank + 1}{maxSameSeqRank + 2}{maxSameSeqRank + 3}顺子");
        else if (maxSameSeq >= 3)
            AddFan(result, "一色三同顺 24番", $"3副{SuitName(maxSameSeqSuit)}{maxSameSeqRank + 1}{maxSameSeqRank + 2}{maxSameSeqRank + 3}顺子");
        else if (maxSameSeq >= 2)
            AddFan(result, "一般高 1番", $"2副{SuitName(maxSameSeqSuit)}{maxSameSeqRank + 1}{maxSameSeqRank + 2}{maxSameSeqRank + 3}顺子");

        // 三色三同顺 / 喜相逢
        var hasThreeSuitSameSeq = false;
        var hasTwoSuitSameSeq = false;
        var matchRank = -1;
        for (var rank = 0; rank < 7; rank++)
        {
            var suitCount = seqBySuit.Count(s => s.Contains(rank));
            if (suitCount >= 3) { hasThreeSuitSameSeq = true; matchRank = rank; }
            if (suitCount >= 2 && matchRank < 0) matchRank = rank;
            if (suitCount >= 2) hasTwoSuitSameSeq = true;
        }

        if (hasThreeSuitSameSeq)
            AddFan(result, "三色三同顺 8番", $"万筒条各有一副{matchRank + 1}{matchRank + 2}{matchRank + 3}顺子");
        else if (hasTwoSuitSameSeq && maxSameSeq < 2)
            AddFan(result, "喜相逢 1番", $"两种花色各有一副{(matchRank >= 0 ? $"{matchRank + 1}{matchRank + 2}{matchRank + 3}" : "")}顺子");

        // 一色四步高 / 一色三步高
        var maxStepSeq = GetMaxStepSequences(seqBySuit);
        if (maxStepSeq >= 4)
            AddFan(result, "一色四步高 32番", "同一花色4副顺子依次递增（步长1或2）");
        else if (maxStepSeq >= 3)
            AddFan(result, "一色三步高 16番", "同一花色3副顺子依次递增（步长1或2）");

        if (HasThreeSuitStepSequences(seqBySuit))
            AddFan(result, "三色三步高 6番", "三种花色各一副顺子，起始序数依次递增1");

        // ── Triplet step patterns ──

        var tripBySuit = new List<int>[3];
        for (var s = 0; s < 3; s++) tripBySuit[s] = [];
        foreach (var idx in triplets)
        {
            if (idx < 27) tripBySuit[idx / 9].Add(idx % 9);
        }

        // 一色四节高 / 一色三节高
        var maxTripletStep = GetMaxConsecutiveTriplets(tripBySuit);
        if (maxTripletStep >= 4)
            AddFan(result, "一色四节高 48番", "同一花色4副刻子序数依次递增1");
        else if (maxTripletStep >= 3 && !result.FanNames.Contains("一色三同顺 24番"))
            AddFan(result, "一色三节高 24番", "同一花色3副刻子序数依次递增1");

        // 三色三节高
        if (HasThreeSuitTripletSteps(tripBySuit))
            AddFan(result, "三色三节高 8番", "三种花色各一副刻子，序数依次递增1");

        // ── Straight patterns ──

        // 清龙: 123+456+789 in same suit
        for (var s = 0; s < 3; s++)
        {
            if (seqBySuit[s].Contains(0) && seqBySuit[s].Contains(3) && seqBySuit[s].Contains(6))
            {
                AddFan(result, "清龙 16番", $"{SuitName(s)}的123+456+789");
                break;
            }
        }

        // 花龙: 123/456/789 in different suits
        if (!result.FanNames.Contains("清龙 16番"))
        {
            foreach (var order in TileConstants.SuitPermutations)
            {
                if (seqBySuit[order[0]].Contains(0)
                    && seqBySuit[order[1]].Contains(3)
                    && seqBySuit[order[2]].Contains(6))
                {
                    AddFan(result, "花龙 8番", $"{SuitName(order[0])}123+{SuitName(order[1])}456+{SuitName(order[2])}789");
                    break;
                }
            }
        }

        // 老少副: 123+789 in same suit
        for (var s = 0; s < 3; s++)
        {
            if (seqBySuit[s].Contains(0) && seqBySuit[s].Contains(6))
            {
                AddFan(result, "老少副 1番", $"{SuitName(s)}的123+789");
                break;
            }
        }

        // 连六: 6 consecutive tiles in one suit (2 adjacent sequences)
        for (var s = 0; s < 3; s++)
        {
            var sorted = seqBySuit[s].OrderBy(r => r).ToList();
            for (var i = 0; i < sorted.Count - 1; i++)
            {
                if (sorted[i + 1] == sorted[i] + 3)
                {
                    AddFan(result, "连六 1番", $"{SuitName(s)}的{sorted[i] + 1}~{sorted[i] + 6}连续6张");
                    goto doneConsecutiveSix;
                }
            }
        }
        doneConsecutiveSix:

        // 一色双龙会: same suit 123+789+123+789 + pair of 5
        for (var s = 0; s < 3; s++)
        {
            var count123 = seqBySuit[s].Count(r => r == 0);
            var count789 = seqBySuit[s].Count(r => r == 6);
            if (count123 >= 2 && count789 >= 2 && decomp.PairIndex == s * 9 + 4)
            {
                AddFan(result, "一色双龙会 64番", $"{SuitName(s)}的123×2+789×2+5作将");
                break;
            }
        }

        // 三色双龙会: two suits each have 123+789, third suit has pair of 5
        if (!result.FanNames.Contains("一色双龙会 64番"))
        {
            foreach (var order in TileConstants.SuitPermutations)
            {
                if (seqBySuit[order[0]].Contains(0) && seqBySuit[order[0]].Contains(6)
                    && seqBySuit[order[1]].Contains(0) && seqBySuit[order[1]].Contains(6)
                    && decomp.PairIndex == order[2] * 9 + 4)
                {
                    AddFan(result, "三色双龙会 16番",
                        $"{SuitName(order[0])}和{SuitName(order[1])}各有123+789，{SuitName(order[2])}5作将");
                    break;
                }
            }
        }
    }

    private static void AddHonorFans(
        FanEvaluationResult result, string seatWind, string roundWind, HandDecomposition decomp)
    {
        // 门风刻
        if (TileConstants.TileIndexMap.TryGetValue(seatWind, out var seatIdx))
        {
            if (decomp.Melds.Any(m => m.Kind is MeldKind.Triplet or MeldKind.Kong && m.TileIndex == seatIdx))
                AddFan(result, "门风刻 2番", $"有门风{TileName(seatIdx)}的刻子");
        }

        // 圈风刻
        if (TileConstants.TileIndexMap.TryGetValue(roundWind, out var roundIdx))
        {
            if (decomp.Melds.Any(m => m.Kind is MeldKind.Triplet or MeldKind.Kong && m.TileIndex == roundIdx))
                AddFan(result, "圈风刻 2番", $"有圈风{TileName(roundIdx)}的刻子");
        }

        // 箭刻
        var dragonTriplets = decomp.Melds.Where(m =>
            m.Kind is MeldKind.Triplet or MeldKind.Kong && m.TileIndex is >= 31 and <= 33).ToList();
        foreach (var d in dragonTriplets)
            AddFan(result, "箭刻 2番", $"有{TileName(d.TileIndex)}的刻子");
    }

    private static void AddSituationalFans(
        FanEvaluationResult result, FanEvaluationContext context, bool hasOpenMeld, int[] handOnlyCounts)
    {
        if (context.IsSelfDraw && !hasOpenMeld)
            AddFan(result, "不求人 4番", "自摸和牌且没有吃碰明杠");
        else if (!context.IsSelfDraw && !hasOpenMeld)
            AddFan(result, "门前清 2番", "没有吃碰明杠，点炮和牌");

        if (context.IsWinningTileLast)
            AddFan(result, "和绝张 4番", "和牌时该牌为场上最后一张");

        if (context.IsKongDraw)
            AddFan(result, "杠上开花 8番", "开杠后摸牌和牌");

        if (context.IsRobbingKong)
            AddFan(result, "抢杠和 8番", "他家加杠时抢杠和牌");

        if (context.IsLastTile)
            AddFan(result, context.IsSelfDraw ? "妙手回春 8番" : "海底捞月 8番",
                context.IsSelfDraw ? "摸到牌墙最后一张牌自摸和牌" : "吃到最后一张打出的牌和牌");

        if (!string.IsNullOrWhiteSpace(context.WinningTile)
            && TileConstants.TileIndexMap.TryGetValue(context.WinningTile, out var winningIndex))
        {
            // handOnlyCounts already has locked meld tiles subtracted,
            // so WaitAnalyzer only decomposes the free hand tiles.
            var waits = WaitAnalyzer.GetWaitTypes(handOnlyCounts, winningIndex);
            if (waits.HasFlag(WaitAnalyzer.WaitType.Edge))
                AddFan(result, "边张 1番", $"和牌张{TileName(winningIndex)}为边张听牌");
            if (waits.HasFlag(WaitAnalyzer.WaitType.Closed))
                AddFan(result, "坎张 1番", $"和牌张{TileName(winningIndex)}为坎张听牌");
            if (waits.HasFlag(WaitAnalyzer.WaitType.Single))
                AddFan(result, "单钓将 1番", $"和牌张{TileName(winningIndex)}为单钓将牌");

            var totalOpenMelds = context.ChiCount + context.PengCount + context.MingKongCount;
            if (!context.IsSelfDraw && totalOpenMelds == 4 && !context.HasAnKong
                && waits.HasFlag(WaitAnalyzer.WaitType.Single))
            {
                AddFan(result, "全求人 6番", "4副面子均为吃碰明杠，单钓将牌点炮和");
            }
        }

        if (context.IsSelfDraw)
            AddFan(result, "自摸 1番", "自摸和牌");

        if (context.FlowerCount > 0)
            AddFan(result, $"花牌 {context.FlowerCount}番", $"有{context.FlowerCount}张花牌");
    }

    private static void FinalizeResult(FanEvaluationResult result)
    {
        // 无番和: per national rules, if the only remaining fans (excluding flowers)
        // are from {自摸, 边张, 坎张, 单钓将}, the hand is scored as 无番和 8番.
        // These minor situational fans are suppressed by 无番和.
        var wufanExcluded = new HashSet<string> { "自摸 1番", "边张 1番", "坎张 1番", "单钓将 1番" };
        var nonFlowerFans = result.FanNames.Where(name => !name.StartsWith("花牌")).ToList();
        var hasOnlyWufanExcludable = nonFlowerFans.Count == 0
            || nonFlowerFans.All(name => wufanExcluded.Contains(name));

        if (hasOnlyWufanExcludable)
        {
            foreach (var name in wufanExcluded)
                while (result.FanNames.Remove(name)) { }
            AddFan(result, "无番和 8番", "和牌但无其他番种，计8番");
        }

        result.FanNames.Sort((a, b) => GetFanPoints(b).CompareTo(GetFanPoints(a)));
        result.TotalFan = result.FanNames.Sum(GetFanPoints);
    }

    private static void AddFan(FanEvaluationResult result, string fanName, string reason)
    {
        result.FanNames.Add(fanName);
        if (!string.IsNullOrEmpty(reason) && !result.Reasons.ContainsKey(fanName))
            result.Reasons[fanName] = reason;
    }

    /// <summary>
    /// For special hands (十三幺, 七对, 全不靠, etc.) that don't use standard decomposition,
    /// find the last group containing the winning tile for highlighting.
    /// </summary>
    private static void SetWinningGroupForSpecialHand(FanEvaluationResult result, string? winningTile)
    {
        if (string.IsNullOrWhiteSpace(winningTile) || result.DecompositionTileGroups is null)
            return;

        // For special hands, search from the end to prefer the pair/last group
        for (var i = result.DecompositionTileGroups.Count - 1; i >= 0; i--)
        {
            if (result.DecompositionTileGroups[i].Contains(winningTile))
            {
                result.WinningTileGroupIndex = i;
                return;
            }
        }
    }

    /// <summary>
    /// Determines which tile group in the decomposition the winning tile belongs to.
    /// Locked melds (chi/peng/kong) are skipped; only hand-derived groups and the pair
    /// are candidates, so the winning tile is highlighted in the correct group.
    /// </summary>
    private static int ComputeWinningTileGroupIndex(
        HandDecomposition decomp, string? winningTile, int lockedMeldCount)
    {
        if (string.IsNullOrWhiteSpace(winningTile)
            || !TileConstants.TileIndexMap.TryGetValue(winningTile, out var winIndex))
            return -1;

        var groups = BuildDecompTileGroups(decomp);
        var winCode = TileConstants.TileCodes[winIndex];

        // The pair is the last group — check it first (单钓将 scenario)
        var pairGroupIndex = groups.Count - 1;
        if (decomp.PairIndex == winIndex)
            return pairGroupIndex;

        // Search hand-derived melds (indices lockedMeldCount .. melds.Count-1)
        // These correspond to group indices lockedMeldCount .. groups.Count-2
        for (var i = decomp.Melds.Count - 1; i >= lockedMeldCount; i--)
        {
            if (decomp.Melds[i].Tiles.Contains(winIndex))
                return i;
        }

        // Fallback: search all groups (shouldn't normally happen)
        for (var i = groups.Count - 1; i >= 0; i--)
        {
            if (groups[i].Contains(winCode))
                return i;
        }

        return -1;
    }

    private static string TileName(int index)
    {
        if (index < 0 || index >= TileConstants.TileCodes.Length) return "?";
        var code = TileConstants.TileCodes[index];
        return code switch
        {
            "E" => "东", "S" => "南", "W" => "西", "N" => "北",
            "C" => "中", "F" => "发", "P" => "白",
            _ when code.Length == 2 => $"{code[0]}{code[1] switch { 'm' => "万", 'p' => "筒", 's' => "条", _ => "" }}",
            _ => code
        };
    }

    private static string SuitName(int suitIndex) => suitIndex switch
    {
        0 => "万", 1 => "筒", 2 => "条", _ => "?"
    };

    private static string MeldDesc(MeldInfo m) => m.Kind switch
    {
        MeldKind.Sequence => $"{TileName(m.TileIndex)}{TileName(m.TileIndex + 1)}{TileName(m.TileIndex + 2)}",
        MeldKind.Triplet => $"{TileName(m.TileIndex)}×3",
        MeldKind.Kong => $"{TileName(m.TileIndex)}×4",
        _ => TileName(m.TileIndex)
    };

    private static string DecompDesc(HandDecomposition decomp)
    {
        var parts = new List<string>();
        foreach (var m in decomp.Melds)
            parts.Add(MeldDesc(m));
        parts.Add($"{TileName(decomp.PairIndex)}×2");
        return string.Join(" ", parts);
    }

    private static void SetDecompositionInfo(FanEvaluationResult result, HandDecomposition decomp)
    {
        result.DecompositionDescription = DecompDesc(decomp);
        result.DecompositionTileGroups = BuildDecompTileGroups(decomp);
        result.WinningTileGroupIndex = -1; // reset; caller sets if needed
    }

    private static List<List<string>> BuildDecompTileGroups(HandDecomposition decomp)
    {
        var groups = new List<List<string>>();
        foreach (var m in decomp.Melds)
            groups.Add(m.Tiles.Select(TileCode).ToList());
        groups.Add([TileCode(decomp.PairIndex), TileCode(decomp.PairIndex)]);
        return groups;
    }

    private static string TileCode(int index)
    {
        if (index < 0 || index >= TileConstants.TileCodes.Length) return "?";
        return TileConstants.TileCodes[index];
    }

    /// <summary>Build tile groups for thirteen orphans: each tile is its own group, pair tile gets 2.</summary>
    private static List<List<string>> BuildThirteenOrphansTileGroups(int[] counts)
    {
        var groups = new List<List<string>>();
        for (var i = 0; i < counts.Length; i++)
        {
            if (counts[i] <= 0) continue;
            var code = TileCode(i);
            groups.Add(Enumerable.Repeat(code, counts[i]).ToList());
        }
        return groups;
    }

    /// <summary>Build tile groups where each tile is a single group (for 全不靠/七星不靠).</summary>
    private static List<List<string>> BuildSingleTileGroups(int[] counts)
    {
        var groups = new List<List<string>>();
        for (var i = 0; i < counts.Length; i++)
        {
            for (var j = 0; j < counts[i]; j++)
                groups.Add([TileCode(i)]);
        }
        return groups;
    }

    /// <summary>Build tile groups for seven pairs: each pair is a group.</summary>
    private static List<List<string>> BuildSevenPairsTileGroups(int[] counts)
    {
        var groups = new List<List<string>>();
        for (var i = 0; i < counts.Length; i++)
        {
            var code = TileCode(i);
            for (var p = 0; p < counts[i] / 2; p++)
                groups.Add([code, code]);
        }
        return groups;
    }

    // ── Helper methods ──

    /// <summary>
    /// Evaluates a hand containing a composed dragon (组合龙) pattern.
    /// The dragon uses 9 individual tiles; remaining 5 tiles must form 1 meld + 1 pair.
    /// </summary>
    private static FanEvaluationResult? EvaluateComposedDragon(FanEvaluationContext context, int[] counts, int[] handOnlyCounts)
    {
        var hasOpenMeld = context.HasChi || context.HasPeng || context.HasMingKong;
        FanEvaluationResult? best = null;

        // Try each composed dragon permutation
        foreach (var order in TileConstants.SuitPermutations)
        {
            var group0Ranks = new[] { 0, 3, 6 };
            var group1Ranks = new[] { 1, 4, 7 };
            var group2Ranks = new[] { 2, 5, 8 };

            var suit0 = order[0];
            var suit1 = order[1];
            var suit2 = order[2];

            // Check this permutation has the required tiles
            if (!group0Ranks.All(r => counts[suit0 * 9 + r] > 0)) continue;
            if (!group1Ranks.All(r => counts[suit1 * 9 + r] > 0)) continue;
            if (!group2Ranks.All(r => counts[suit2 * 9 + r] > 0)) continue;

            // Subtract dragon tiles
            var remaining = counts.ToArray();
            foreach (var r in group0Ranks) remaining[suit0 * 9 + r]--;
            foreach (var r in group1Ranks) remaining[suit1 * 9 + r]--;
            foreach (var r in group2Ranks) remaining[suit2 * 9 + r]--;

            // Remaining 5 tiles must form 1 meld + 1 pair
            var decomps = new List<HandDecomposition>();
            for (var i = 0; i < remaining.Length; i++)
            {
                if (remaining[i] < 2) continue;
                remaining[i] -= 2;

                // Try triplet
                var idx = Array.FindIndex(remaining, c => c > 0);
                if (idx >= 0 && remaining[idx] >= 3)
                {
                    remaining[idx] -= 3;
                    if (remaining.All(c => c == 0))
                    {
                        decomps.Add(new HandDecomposition
                        {
                            PairIndex = i,
                            Melds = [new MeldInfo(MeldKind.Triplet, idx)],
                            IsOpen = [false]
                        });
                    }
                    remaining[idx] += 3;
                }

                // Try sequence
                if (idx >= 0 && idx < 27 && idx % 9 <= 6
                    && remaining[idx] > 0 && remaining[idx + 1] > 0 && remaining[idx + 2] > 0)
                {
                    remaining[idx]--;
                    remaining[idx + 1]--;
                    remaining[idx + 2]--;
                    if (remaining.All(c => c == 0))
                    {
                        decomps.Add(new HandDecomposition
                        {
                            PairIndex = i,
                            Melds = [new MeldInfo(MeldKind.Sequence, idx)],
                            IsOpen = [false]
                        });
                    }
                    remaining[idx]++;
                    remaining[idx + 1]++;
                    remaining[idx + 2]++;
                }

                remaining[i] += 2;
            }

            // Evaluate each decomposition with composed dragon fan
            foreach (var decomp in decomps)
            {
                var result = new FanEvaluationResult();
                SetDecompositionInfo(result, decomp);
                var decompInfo = DecompDesc(decomp);
                AddFan(result, "组合龙 12番", $"手牌含{SuitName(suit0)}147+{SuitName(suit1)}258+{SuitName(suit2)}369");

                AddTilePropertyFans(result, counts, context);
                AddDecompositionFans(result, decomp, counts, context, decompInfo);
                AddKongFans(result, context.MingKongCount + context.AnKongCount, context);
                AddConcealedTripletFans(result, decomp, context);
                AddWindDragonPatternFans(result, decomp, counts);
                AddMeldPatternFans(result, decomp);
                AddHonorFans(result, context.SeatWind, context.RoundWind, decomp);
                AddSituationalFans(result, context, hasOpenMeld, handOnlyCounts);
                ApplyExclusions(result);
                FinalizeResult(result);

                if (best is null || result.TotalFan > best.TotalFan)
                    best = result;
            }
        }

        return best;
    }

    private static bool IsAllWithFive(HandDecomposition decomp)
    {
        // Pair must contain 5
        if (decomp.PairIndex >= 27 || decomp.PairIndex % 9 != 4)
            return false;

        foreach (var meld in decomp.Melds)
        {
            var hasFive = meld.Tiles.Any(t => t < 27 && t % 9 == 4);
            if (!hasFive) return false;
        }

        return true;
    }

    private static bool IsAllWithTerminals(HandDecomposition decomp)
    {
        // Pair must be terminal or honor
        if (decomp.PairIndex < 27 && decomp.PairIndex % 9 is not (0 or 8))
            return false;

        foreach (var meld in decomp.Melds)
        {
            var hasTerminalOrHonor = meld.Tiles.Any(t =>
                t >= 27 || t % 9 is 0 or 8);
            if (!hasTerminalOrHonor) return false;
        }

        return true;
    }

    private static int CountYaojiuTriplets(HandDecomposition decomp, string seatWind, string roundWind)
    {
        var excluded = new HashSet<int>();
        if (TileConstants.TileIndexMap.TryGetValue(seatWind, out var seatIdx))
            excluded.Add(seatIdx);
        if (TileConstants.TileIndexMap.TryGetValue(roundWind, out var roundIdx))
            excluded.Add(roundIdx);
        // Exclude dragon indices (counted separately as 箭刻)
        excluded.Add(31);
        excluded.Add(32);
        excluded.Add(33);

        var count = 0;
        foreach (var meld in decomp.Melds)
        {
            if (meld.Kind is not (MeldKind.Triplet or MeldKind.Kong))
                continue;
            if (excluded.Contains(meld.TileIndex))
                continue;
            if (meld.TileIndex >= 27 || meld.TileIndex % 9 is 0 or 8)
                count++;
        }

        return count;
    }

    private static int[] GetSameNumberTripletCounts(HandDecomposition decomp)
    {
        var result = new int[9];
        foreach (var meld in decomp.Melds)
        {
            if (meld.Kind is MeldKind.Triplet or MeldKind.Kong && meld.TileIndex < 27)
            {
                result[meld.TileIndex % 9]++;
            }
        }
        return result;
    }

    private static int GetMaxStepSequences(List<int>[] seqBySuit)
    {
        var max = 0;
        foreach (var suit in seqBySuit)
        {
            if (suit.Count < 2) continue;
            var sorted = suit.OrderBy(r => r).ToList();
            foreach (var step in new[] { 1, 2 })
            {
                for (var start = 0; start < sorted.Count; start++)
                {
                    var len = 1;
                    var working = sorted.ToList();
                    var current = working[start];
                    working.RemoveAt(start);
                    while (true)
                    {
                        var next = current + step;
                        var idx = working.IndexOf(next);
                        if (idx < 0) break;
                        len++;
                        current = next;
                        working.RemoveAt(idx);
                    }
                    max = Math.Max(max, len);
                }
            }
        }
        return max;
    }

    private static int GetMaxConsecutiveTriplets(List<int>[] tripBySuit)
    {
        var max = 0;
        foreach (var suit in tripBySuit)
        {
            if (suit.Count < 2) continue;
            var sorted = suit.Distinct().OrderBy(r => r).ToList();
            var run = 1;
            for (var i = 1; i < sorted.Count; i++)
            {
                if (sorted[i] == sorted[i - 1] + 1)
                    run++;
                else
                    run = 1;
                max = Math.Max(max, run);
            }
        }
        return max;
    }

    private static bool HasThreeSuitStepSequences(List<int>[] seqBySuit)
    {
        for (var start = 0; start <= 4; start++)
        {
            foreach (var order in TileConstants.SuitPermutations)
            {
                if (seqBySuit[order[0]].Contains(start)
                    && seqBySuit[order[1]].Contains(start + 1)
                    && seqBySuit[order[2]].Contains(start + 2))
                    return true;
            }
        }
        return false;
    }

    private static bool HasThreeSuitTripletSteps(List<int>[] tripBySuit)
    {
        for (var start = 0; start <= 6; start++)
        {
            foreach (var order in TileConstants.SuitPermutations)
            {
                if (tripBySuit[order[0]].Contains(start)
                    && tripBySuit[order[1]].Contains(start + 1)
                    && tripBySuit[order[2]].Contains(start + 2))
                    return true;
            }
        }
        return false;
    }

    private static void ApplyExclusions(FanEvaluationResult result)
    {
        // Data-driven exclusion table: each fan and what it suppresses.
        // Processed in priority order (highest番 first).
        // A fan that has itself been suppressed does NOT suppress others.
        var exclusionTable = new (string Fan, string[] Suppresses)[]
        {
            // 88番
            ("大四喜 88番",     ["小四喜 64番", "三风刻 12番", "圈风刻 2番", "门风刻 2番", "幺九刻 1番", "碰碰和 6番"]),
            ("大三元 88番",     ["小三元 64番", "双箭刻 6番", "箭刻 2番"]),
            ("绿一色 88番",     ["混一色 6番"]),
            ("九莲宝灯 88番",   ["清一色 24番", "幺九刻 1番", "门前清 2番"]),
            ("四杠 88番",       ["单钓将 1番", "碰碰和 6番", "三杠 32番", "双暗杠 6番", "双明杠 4番", "暗杠 2番", "明杠 1番"]),
            ("连七对 88番",     ["清一色 24番", "七对 24番", "单钓将 1番", "门前清 2番"]),
            ("十三幺 88番",     ["五门齐 6番", "全带幺 4番", "单钓将 1番", "门前清 2番", "混幺九 32番"]),

            // 64番
            ("清幺九 64番",     ["混幺九 32番", "碰碰和 6番", "全带幺 4番", "幺九刻 1番", "无字 1番"]),
            ("小四喜 64番",     ["三风刻 12番", "幺九刻 1番"]),
            ("小三元 64番",     ["双箭刻 6番", "箭刻 2番"]),
            ("字一色 64番",     ["碰碰和 6番", "混幺九 32番", "全带幺 4番", "幺九刻 1番", "缺一门 1番"]),
            ("四暗刻 64番",     ["门前清 2番", "碰碰和 6番", "三暗刻 16番", "双暗刻 2番", "不求人 4番", "自摸 1番"]),
            ("一色双龙会 64番", ["平和 2番", "七对 24番", "清一色 24番", "一般高 1番", "老少副 1番", "缺一门 1番", "无字 1番"]),

            // 48番
            ("一色四同顺 48番", ["一色三节高 24番", "一般高 1番", "四归一 2番", "一色三同顺 24番", "缺一门 1番"]),
            ("一色四节高 48番", ["一色三同顺 24番", "一色三节高 24番", "碰碰和 6番", "缺一门 1番"]),

            // 32番
            ("一色四步高 32番", ["一色三步高 16番", "缺一门 1番"]),
            ("三杠 32番",       ["双暗杠 6番", "双明杠 4番", "暗杠 2番", "明杠 1番"]),
            ("混幺九 32番",     ["碰碰和 6番", "幺九刻 1番", "全带幺 4番"]),

            // 24番
            ("七对 24番",       ["门前清 2番", "不求人 4番", "单钓将 1番", "自摸 1番"]),
            ("七星不靠 24番",   ["五门齐 6番", "不求人 4番", "单钓将 1番", "门前清 2番", "全不靠 12番", "自摸 1番"]),
            ("全双刻 24番",     ["碰碰和 6番", "断幺 2番", "无字 1番"]),
            ("清一色 24番",     ["缺一门 1番", "无字 1番"]),
            ("一色三同顺 24番", ["一色三节高 24番", "一般高 1番"]),
            ("一色三节高 24番", ["一色三同顺 24番"]),
            ("全大 24番",       ["无字 1番", "大于五 12番"]),
            ("全中 24番",       ["断幺 2番", "无字 1番"]),
            ("全小 24番",       ["无字 1番", "小于五 12番"]),

            // 16番
            ("清龙 16番",       ["连六 1番", "老少副 1番"]),
            ("三色双龙会 16番", ["喜相逢 1番", "老少副 1番", "无字 1番", "平和 2番"]),
            ("全带五 16番",     ["断幺 2番", "无字 1番"]),
            ("三同刻 16番",     ["双同刻 2番"]),
            ("三暗刻 16番",     ["双暗刻 2番"]),

            // 12番
            ("全不靠 12番",     ["五门齐 6番", "不求人 4番", "单钓将 1番", "门前清 2番", "自摸 1番"]),
            ("大于五 12番",     ["无字 1番"]),
            ("小于五 12番",     ["无字 1番"]),
            ("三风刻 12番",     ["缺一门 1番"]),

            // 8番
            ("推不倒 8番",       ["缺一门 1番"]),
            ("三色三同顺 8番",   ["喜相逢 1番"]),
            ("花龙 8番",         ["连六 1番", "老少副 1番"]),
            ("妙手回春 8番",     ["自摸 1番"]),
            ("海底捞月 8番",     ["自摸 1番"]),
            ("杠上开花 8番",     ["自摸 1番"]),
            ("抢杠和 8番",       ["和绝张 4番"]),

            // 6番
            ("混一色 6番",       ["缺一门 1番"]),
            ("全求人 6番",       ["单钓将 1番"]),
            ("双箭刻 6番",       ["箭刻 2番"]),
            ("双暗杠 6番",       ["暗杠 2番", "双暗刻 2番"]),

            // 4番
            ("不求人 4番",       ["自摸 1番", "门前清 2番"]),
            ("双明杠 4番",       ["明杠 1番"]),

            // 2番
            ("平和 2番",         ["无字 1番"]),
            ("断幺 2番",         ["无字 1番"]),
        };

        var names = result.FanNames;
        var suppressed = new HashSet<string>();

        // Process each rule: if the fan is present AND not itself suppressed,
        // its targets get suppressed.
        foreach (var (fan, targets) in exclusionTable)
        {
            if (!names.Contains(fan) || suppressed.Contains(fan))
                continue;

            foreach (var target in targets)
                suppressed.Add(target);
        }

        foreach (var name in suppressed)
        {
            while (names.Remove(name)) { }
        }
    }

    private static int GetFanPoints(string fanName)
    {
        var parts = fanName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return 0;
        var number = parts[^1].Replace("番", string.Empty);
        return int.TryParse(number, out var points) ? points : 0;
    }
}
