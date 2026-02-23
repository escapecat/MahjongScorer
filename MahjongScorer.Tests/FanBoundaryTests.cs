using Xunit;
using MahjongScorer.Utilities;

namespace MahjongScorer.Tests;

/// <summary>
/// Boundary / corner-case tests for every fan rule, designed to catch
/// off-by-one errors, wrong rank math, and missing detection paths.
/// </summary>
public partial class FanTests
{
    // ── 推不倒 tile-set correctness ──────────────────────────────
    // Rule: 筒 1234589, 条 245689, 白板(P)
    // 筒 index 9-17  → allowed ranks 0,1,2,3,4,7,8  (tiles 1p-5p,8p,9p)
    // 条 index 18-26 → allowed ranks 1,3,4,5,7,8    (tiles 2s,4s-6s,8s,9s)

    [Fact]
    public void TuiBuDao_Rejects_6p()
    {
        // 6p (rank 5 in pin) is NOT in pushed-down set
        var counts = BuildCounts("123p 456p 789s 111p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("推不倒 8番", result.FanNames);
    }

    [Fact]
    public void TuiBuDao_Rejects_7p()
    {
        // 7p (rank 6 in pin) is NOT in pushed-down set
        var counts = BuildCounts("123p 789p 456s 111p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("推不倒 8番", result.FanNames);
    }

    [Fact]
    public void TuiBuDao_Rejects_1s()
    {
        // 1s (rank 0 in sou) is NOT in pushed-down set
        var counts = BuildCounts("123s 456p 789p 111p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("推不倒 8番", result.FanNames);
    }

    [Fact]
    public void TuiBuDao_Rejects_3s()
    {
        // 3s (rank 2 in sou) is NOT in pushed-down set
        var counts = BuildCounts("234s 456p 789p 111p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("推不倒 8番", result.FanNames);
    }

    [Fact]
    public void TuiBuDao_Rejects_7s()
    {
        // 7s (rank 6 in sou) is NOT in pushed-down set
        var counts = BuildCounts("789s 456p 123p 111p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("推不倒 8番", result.FanNames);
    }

    [Fact]
    public void TuiBuDao_Rejects_Man()
    {
        // Any 万 tile invalidates 推不倒
        var counts = BuildCounts("123m 456p 789p 111p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("推不倒 8番", result.FanNames);
    }

    // ── 绿一色 tile-set correctness ──────────────────────────────
    // Green tiles: 2s(19) 3s(20) 4s(21) 6s(23) 8s(25) F(32)

    [Fact]
    public void AllGreen_Rejects_5s()
    {
        // 5s is NOT a green tile
        var counts = BuildCounts("222345666s 888s FF");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("绿一色 88番", result.FanNames);
    }

    [Fact]
    public void AllGreen_Rejects_1s()
    {
        var counts = BuildCounts("123s 234s 666s 888s FF");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("绿一色 88番", result.FanNames);
    }

    // ── 大于五 / 小于五 boundary ────────────────────────────────
    // 大于五: only tiles 6-9 (rank 5-8)
    // 小于五: only tiles 1-4 (rank 0-3)

    [Fact]
    public void DaYuWu_Rejects_5()
    {
        // Tile 5 (rank 4) should disqualify 大于五
        var counts = BuildCounts("567m 678p 789s 99m 55m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("大于五 12番", result.FanNames);
    }

    [Fact]
    public void XiaoYuWu_Rejects_5()
    {
        // Tile 5 (rank 4) should disqualify 小于五
        var counts = BuildCounts("123m 234p 345s 11m 55m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("小于五 12番", result.FanNames);
    }

    [Fact]
    public void DaYuWu_Accepts_6789_Only()
    {
        // Tiles 6-9 only, across suits
        var counts = BuildCounts("678m 789p 678s 999m 66p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.Contains("大于五 12番", result.FanNames);
    }

    [Fact]
    public void XiaoYuWu_Accepts_1234_Only()
    {
        // Tiles 1-4 only, across suits
        var counts = BuildCounts("123m 234p 123s 111m 44p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.Contains("小于五 12番", result.FanNames);
    }

    // ── 全带五 boundary ─────────────────────────────────────────
    // Every set + pair must include a 5

    [Fact]
    public void QuanDaiWu_Rejects_SetWithout5()
    {
        // 123m does NOT contain 5
        var counts = BuildCounts("123m 456m 567p 555s 55m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("全带五 16番", result.FanNames);
    }

    // ── 全带幺 boundary ─────────────────────────────────────────
    // Every set + pair must include a terminal or honor

    [Fact]
    public void QuanDaiYao_Rejects_SetWithout19()
    {
        // 456m has no terminal/honor
        var counts = BuildCounts("123m 456m 789s EEE 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("全带幺 4番", result.FanNames);
    }

    // ── 连七对 must be exactly 7 consecutive pairs ──────────────

    [Fact]
    public void LianQiDui_Rejects_GapPairs()
    {
        // 7 pairs but NOT consecutive (has a gap)
        var counts = BuildCounts("11224455667788m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("连七对 88番", result.FanNames);
    }

    [Fact]
    public void LianQiDui_Rejects_MixedSuit()
    {
        // consecutive ranks but across suits
        var counts = BuildCounts("1122334455m 6677p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("连七对 88番", result.FanNames);
    }

    // ── 九莲宝灯 must be concealed ─────────────────────────────

    [Fact]
    public void JiuLianBaoDeng_Rejects_OpenMeld()
    {
        var counts = BuildCounts("1112345678999m 5m");
        var context = BuildContext(counts) with { ChiCount = 1 };
        var result = FanEvaluator.Evaluate(context);
        Assert.DoesNotContain("九莲宝灯 88番", result.FanNames);
    }

    // ── 边张 boundary: only 3 in 123 or 7 in 789 ───────────────

    [Fact]
    public void BianZhang_7m_In_789()
    {
        var counts = BuildCounts("789m 123p 456s 111m 99p");
        var context = BuildContext(counts) with { WinningTile = "7m" };
        var result = FanEvaluator.Evaluate(context);
        Assert.Contains("边张 1番", result.FanNames);
    }

    [Fact]
    public void BianZhang_NotFor_MiddleTile()
    {
        // Winning tile 2m in 123m is NOT edge wait (only 3 is)
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var context = BuildContext(counts) with { WinningTile = "2m" };
        var result = FanEvaluator.Evaluate(context);
        Assert.DoesNotContain("边张 1番", result.FanNames);
    }

    // ── 坎张 closed wait ────────────────────────────────────────

    [Fact]
    public void KanZhang_MiddleOf_Sequence()
    {
        // Winning on 8m in 789m → 坎张
        var counts = BuildCounts("789m 123p 456s 111m 99p");
        var context = BuildContext(counts) with { WinningTile = "8m" };
        var result = FanEvaluator.Evaluate(context);
        Assert.Contains("坎张 1番", result.FanNames);
    }

    // ── 幺九刻 for non-seat/round wind ──────────────────────────

    [Fact]
    public void YaoJiuKe_Counts_NonSeatWind()
    {
        // West wind triplet when seat=E, round=E → should be 幺九刻
        var counts = BuildCounts("WWW 123m 456p 789s 99m");
        var context = BuildContext(counts) with { SeatWind = "E", RoundWind = "E" };
        var result = FanEvaluator.Evaluate(context);
        Assert.Contains("幺九刻 1番", result.FanNames);
    }

    [Fact]
    public void YaoJiuKe_NotCounted_ForDragon()
    {
        // Dragon triplet counted as 箭刻, not 幺九刻
        var counts = BuildCounts("CCC 123m 456p 789s 99m");
        var context = BuildContext(counts) with { SeatWind = "E", RoundWind = "E" };
        var result = FanEvaluator.Evaluate(context);
        Assert.Contains("箭刻 2番", result.FanNames);
        Assert.DoesNotContain("幺九刻 1番", result.FanNames);
    }

    // ── 四归一 with kongs ───────────────────────────────────────

    [Fact]
    public void SiGuiYi_NotCounted_WhenAllAreKongs()
    {
        // Kong tiles have count capped at 3 (as in BuildEvaluationCounts).
        // With count=3, 四归一 should NOT be triggered.
        var counts = BuildCounts("111m 234m 456p 789s 99p");
        var context = BuildContext(counts) with
        {
            AnKongCount = 1,
            Melds = new List<IReadOnlyList<string>>
            {
                new[] { "1m", "1m", "1m", "1m" }
            }
        };
        var result = FanEvaluator.Evaluate(context);
        Assert.DoesNotContain("四归一 2番", result.FanNames);
    }

    // ── 清龙 also triggers 老少副 which gets suppressed ─────────

    [Fact]
    public void QingLong_Suppresses_LaoShaoFu_And_LianLiu()
    {
        var counts = BuildCounts("123m 456m 789m 111p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.Contains("清龙 16番", result.FanNames);
        Assert.DoesNotContain("老少副 1番", result.FanNames);
        Assert.DoesNotContain("连六 1番", result.FanNames);
    }

    // ── 小四喜 needs wind pair ──────────────────────────────────

    [Fact]
    public void XiaoSiXi_Needs_WindPair()
    {
        // 3 wind triplets but pair is NOT a wind → no 小四喜
        var counts = BuildCounts("EEE SSS WWW 123m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("小四喜 64番", result.FanNames);
        Assert.Contains("三风刻 12番", result.FanNames);
    }

    // ── 小三元 needs dragon pair ────────────────────────────────

    [Fact]
    public void XiaoSanYuan_Needs_DragonPair()
    {
        // 2 dragon triplets but pair is NOT a dragon → no 小三元
        var counts = BuildCounts("CCC FFF 123m 456p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("小三元 64番", result.FanNames);
        Assert.Contains("双箭刻 6番", result.FanNames);
    }

    // ── 断幺 suppressed when 全中 ───────────────────────────────

    [Fact]
    public void DuanYao_Suppressed_By_QuanZhong()
    {
        var counts = BuildCounts("456m 456p 456s 444m 66p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.Contains("全中 24番", result.FanNames);
        Assert.DoesNotContain("断幺 2番", result.FanNames);
    }

    // ── 全双刻 requires even tiles only (2/4/6/8) ──────────────

    [Fact]
    public void QuanShuangKe_Rejects_OddTile()
    {
        // 3m is odd, disqualifies
        var counts = BuildCounts("222m 444m 666p 888s 33m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("全双刻 24番", result.FanNames);
    }

    // ── 混幺九 requires both honors AND terminals ───────────────

    [Fact]
    public void HunYaoJiu_Requires_Honors()
    {
        // All terminal, no honors → should be 清幺九, not 混幺九
        var counts = BuildCounts("111m 999m 111p 999p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.Contains("清幺九 64番", result.FanNames);
        Assert.DoesNotContain("混幺九 32番", result.FanNames);
    }

    // ── 一色双龙会 exact pattern ────────────────────────────────

    [Fact]
    public void YiSeShuangLongHui_Rejects_WrongPair()
    {
        // pair is not 5 → no 一色双龙会
        var counts = BuildCounts("123789m 123789m 11m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("一色双龙会 64番", result.FanNames);
    }

    // ── 三色双龙会 exact pattern ────────────────────────────────

    [Fact]
    public void SanSeShuangLongHui_Rejects_WrongPairSuit()
    {
        // pair is 5m (same suit as one of the dragons) → should still work as
        // the pair can be from any third suit. 
        // But 5m from man suit which already has 123m,789m → third suit is sou,
        // so pair should be 5s. Let's test wrong pair value:
        var counts = BuildCounts("123m 789m 123p 789p 33s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("三色双龙会 16番", result.FanNames);
    }

    // ── 混一色 requires honors present ──────────────────────────

    [Fact]
    public void HunYiSe_Rejects_NoHonors()
    {
        // Pure suit without honors → 清一色, not 混一色
        var counts = BuildCounts("123456789m 111m 22m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.Contains("清一色 24番", result.FanNames);
        Assert.DoesNotContain("混一色 6番", result.FanNames);
    }

    // ── 七星不靠 requires exactly 7 honors, each = 1 ───────────

    [Fact]
    public void QiXingBuKao_Rejects_MissingHonor()
    {
        // Only 6 different honors
        var counts = BuildCounts("147m 258p 36s E S W N C F");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("七星不靠 24番", result.FanNames);
    }

    // ── 全不靠 allows fewer than 3 groups ───────────────────────

    [Fact]
    public void QuanBuKao_Allows_TwoGroups()
    {
        // 2 distinct suit groups (group0: 147m + 1s, group1: 258p) + 7 honors = 14 tiles
        // requireAllGroups is false for 全不靠, so 2 groups should be OK
        var counts = BuildCounts("147m 1s 258p E S W N C F P");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.Contains("全不靠 12番", result.FanNames);
    }

    // ── 花龙 with correct mixed-suit straight ───────────────────

    [Fact]
    public void HuaLong_DifferentOrder()
    {
        // 456m + 123p + 789s (different suit for each segment)
        var counts = BuildCounts("456m 123p 789s 111m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.Contains("花龙 8番", result.FanNames);
    }

    // ── 组合龙 detects any permutation of 147/258/369 across suits

    [Fact]
    public void ZuHeLong_AlternatePermutation()
    {
        // 258m + 369p + 147s
        var counts = BuildCounts("258m 369p 147s 111m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.Contains("组合龙 12番", result.FanNames);
    }

    // ── 三色三节高 boundary ──────────────────────────────────────

    [Fact]
    public void SanSeSanJieGao_MaxStart()
    {
        // 777m 888p 999s (start rank 6,7,8 → valid)
        var counts = BuildCounts("777m 888p 999s 123m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.Contains("三色三节高 8番", result.FanNames);
    }

    // ── 海底捞月 is NOT self-draw ───────────────────────────────

    [Fact]
    public void HaiDiLaoYue_IsNotSelfDraw()
    {
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var context = BuildContext(counts) with { IsSelfDraw = false, IsLastTile = true };
        var result = FanEvaluator.Evaluate(context);
        Assert.Contains("海底捞月 8番", result.FanNames);
        Assert.DoesNotContain("妙手回春 8番", result.FanNames);
    }

    // ── 四暗刻 with self-draw ───────────────────────────────────

    [Fact]
    public void SiAnKe_WithSelfDraw_IsRecognized()
    {
        var counts = BuildCounts("111m 222p 333s 444m 55p");
        var context = BuildContext(counts) with { IsSelfDraw = true };
        var result = FanEvaluator.Evaluate(context);
        Assert.Contains("四暗刻 64番", result.FanNames);
    }

    // ── IsWinningHandWithMelds correctness ──────────────────────

    [Fact]
    public void WinningHandWithMelds_PengTripletsLocked()
    {
        // User scenario: peng 111m, 222m, 333m; hand = 44m 55m
        // Full counts: [3,3,3,2,2,...] — only 4m and 5m should be winning tiles
        var fullCounts = BuildCounts("111m 222m 333m 44m 55m");
        var melds = new List<IReadOnlyList<string>>
        {
            new[] { "1m", "1m", "1m" },
            new[] { "2m", "2m", "2m" },
            new[] { "3m", "3m", "3m" }
        };

        // Adding 4m → 44m becomes 444m triplet + 55m pair → valid
        var with4m = fullCounts.ToArray();
        with4m[3]++;
        Assert.True(MahjongHandEvaluator.IsWinningHandWithMelds(with4m, melds));

        // Adding 5m → 44m pair + 555m triplet → valid
        var with5m = fullCounts.ToArray();
        with5m[4]++;
        Assert.True(MahjongHandEvaluator.IsWinningHandWithMelds(with5m, melds));

        // Adding 1m → hand becomes 44m 55m 1m, can't form set+pair → invalid
        var with1m = fullCounts.ToArray();
        with1m[0]++;
        Assert.False(MahjongHandEvaluator.IsWinningHandWithMelds(with1m, melds));

        // Adding 3m → hand becomes 44m 55m 3m, can't form set+pair → invalid
        var with3m = fullCounts.ToArray();
        with3m[2]++;
        Assert.False(MahjongHandEvaluator.IsWinningHandWithMelds(with3m, melds));

        // Adding 6m → hand becomes 44m 55m 6m, can't form set+pair → invalid
        var with6m = fullCounts.ToArray();
        with6m[5]++;
        Assert.False(MahjongHandEvaluator.IsWinningHandWithMelds(with6m, melds));
    }

    [Fact]
    public void WinningHandWithMelds_NoMelds_SameAsNormal()
    {
        // Without melds, behaves the same as IsWinningHand
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        Assert.True(MahjongHandEvaluator.IsWinningHandWithMelds(counts, null));
        Assert.True(MahjongHandEvaluator.IsWinningHandWithMelds(counts, new List<IReadOnlyList<string>>()));
    }

    [Fact]
    public void WinningHandWithMelds_ChiMeldLocked()
    {
        // Chi 123m locked, hand = 456p 789s 111m 99p → valid (4 sets + pair)
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var melds = new List<IReadOnlyList<string>>
        {
            new[] { "1m", "2m", "3m" }
        };
        Assert.True(MahjongHandEvaluator.IsWinningHandWithMelds(counts, melds));
    }

    [Fact]
    public void WinningHandWithMelds_MingKongLocked()
    {
        // Ming kong 1111m (4 tiles, but only 3 in counts due to limitToThree).
        // Hand = 456p 789s 111p 99s → valid with 3 remaining sets + pair.
        // Simulates: BuildEvaluationCounts adds 3 for kong + hand tiles.
        var fullCounts = BuildCounts("111m 456p 789s 111p 99s");
        var melds = new List<IReadOnlyList<string>>
        {
            new[] { "1m", "1m", "1m", "1m" }
        };

        // The meld has 4 tiles but counts has only 3 for 1m.
        // IsWinningHandWithMelds must handle this correctly.
        Assert.True(MahjongHandEvaluator.IsWinningHandWithMelds(fullCounts, melds));
    }

    [Fact]
    public void WinningHandWithMelds_AnKongLocked()
    {
        // An kong 1111m, hand = 456p 789s 111p 99s
        var fullCounts = BuildCounts("111m 456p 789s 111p 99s");
        var melds = new List<IReadOnlyList<string>>
        {
            new[] { "1m", "1m", "1m", "1m" }
        };
        Assert.True(MahjongHandEvaluator.IsWinningHandWithMelds(fullCounts, melds));
    }

    [Fact]
    public void WinningHandWithMelds_ChiLocked()
    {
        // Chi 123m locked, hand = 456p 789s 111m 99p
        // Without meld locking, the algorithm could rearrange 123m tiles freely.
        // With locking, only 456p 789s 111m 99p need to form 3 sets + pair.
        var fullCounts = BuildCounts("123m 456p 789s 111m 99p");
        var melds = new List<IReadOnlyList<string>>
        {
            new[] { "1m", "2m", "3m" }
        };
        Assert.True(MahjongHandEvaluator.IsWinningHandWithMelds(fullCounts, melds));
    }

    [Fact]
    public void WinningHandWithMelds_ChiLocked_RejectsInvalidRemaining()
    {
        // Chi 123m locked; hand = 14m 55m — not valid (14m can't form a set)
        var fullCounts = BuildCounts("123m 114m 55m");
        var melds = new List<IReadOnlyList<string>>
        {
            new[] { "1m", "2m", "3m" }
        };
        // Remaining after removing chi: 1m, 4m, 5m, 5m — can't form 1 set + pair
        // (without melds, IsWinningHand might rearrange to 123m + 145m... no, that's not valid either)
        // But let's be explicit: remaining = [1,0,0,1,2,...] — no valid decomposition
        Assert.False(MahjongHandEvaluator.IsWinningHandWithMelds(fullCounts, melds));
    }

    [Fact]
    public void WinningHandWithMelds_KongNegativeCount_DoesNotCorrupt()
    {
        // Ming kong 1111m (4 tiles), counts has only 3 for 1m.
        // After subtraction, handCounts[0] = -1. This should not cause false results.
        // Hand: kong 1m + hand 234m 55m → remaining after kong subtraction: [-1,1,1,1,2,...]
        // The -1 means we over-subtracted; the hand should still be valid
        // since the real hand is just 234m + 55m (1 set + pair).
        var fullCounts = BuildCounts("111m 234m 55m");
        var melds = new List<IReadOnlyList<string>>
        {
            new[] { "1m", "1m", "1m", "1m" }
        };
        // remaining sum = -1+1+1+1+2 = 4, but handCounts[0] = -1 is corrupt
        // Valid decomposition of actual hand (234m + 55m) should work
        Assert.True(MahjongHandEvaluator.IsWinningHandWithMelds(fullCounts, melds));
    }

    [Fact]
    public void WinningHandWithMelds_KongDoesNotAllowFalseWin()
    {
        // Kong 1m, hand: 23m 55m. Without kong consideration this is 123m+55m=win.
        // But the -1 at index 0 should not let sequence 1-2-3 be formed from hand tiles.
        // Actual hand tiles after subtracting kong: [-1,1,1,0,2,...] — remaining=3
        // This should still be valid: the real remaining hand is 23m+55m = not enough for set+pair
        // Actually remaining=3 → need 1 set + pair → 3+2=5 ≠ 3 → fail
        // Wait... remaining = -1+1+1+0+2 = 3, we need 1 set(3) + pair(2) = 5. So remaining < 5 → false.
        // But remaining == 3 would try: pair somewhere (need 2), then 1 tile left, can't form set.
        // Actually pair at 5m: [−1,1,1,0,0,...] remaining=1, can't form sets. Correct = false.
        var fullCounts = BuildCounts("111m 23m 55m");
        var melds = new List<IReadOnlyList<string>>
        {
            new[] { "1m", "1m", "1m", "1m" }
        };
        Assert.False(MahjongHandEvaluator.IsWinningHandWithMelds(fullCounts, melds));
    }

    [Fact]
    public void WinningHandWithMelds_ChiLocked_PreventsRearrangement()
    {
        // Chi 345m is locked. Hand tiles: 33m, 6m, 789p, 111s, EE (11 tiles).
        // fullCounts: 3m=3, 4m=1, 5m=1, 6m=1, 7p,8p,9p, 1s=3, E=2 (14 tiles)
        //
        // Without meld locking, the algorithm can rearrange all tiles freely:
        //   pair=EE, 333m(triplet) + 456m(seq) + 789p + 111s = 4 sets → valid ✅
        //
        // With meld locking, after removing chi 345m:
        //   handCounts: 3m=2, 6m=1, 789p, 111s, EE → 33m+6m can't form a set → invalid ✗
        var fullCounts = BuildCounts("33m 345m 6m 789p 111s EE");
        var melds = new List<IReadOnlyList<string>>
        {
            new[] { "3m", "4m", "5m" }
        };

        // Full counts without meld locking → valid (rearranges chi into triplet+seq)
        Assert.True(MahjongHandEvaluator.IsWinningHand(fullCounts.ToArray()));

        // With meld locking → invalid (33m + 6m can't form a set)
        Assert.False(MahjongHandEvaluator.IsWinningHandWithMelds(fullCounts, melds));
    }

    // ── 一色四节高 / 一色三节高 with peng melds ─────────────────

    [Fact]
    public void YiSeSiJieGao_WithPengMelds_IsRecognized()
    {
        // Peng 111m, 222m, 333m + hand 4444m 55m → win on 4m
        // Should recognize 一色四节高: 111m, 222m, 333m, 444m (4 consecutive triplets)
        var counts = BuildCounts("111m 222m 333m 4444m 55m");
        var context = BuildContext(counts) with
        {
            PengCount = 3,
            Melds = new List<IReadOnlyList<string>>
            {
                new[] { "1m", "1m", "1m" },
                new[] { "2m", "2m", "2m" },
                new[] { "3m", "3m", "3m" }
            }
        };
        var result = FanEvaluator.Evaluate(context);
        Assert.Contains("一色四节高 48番", result.FanNames);
    }

    [Fact]
    public void YiSeSanJieGao_WithPengMelds_IsRecognized()
    {
        // Peng 111m, 222m, 333m + hand 44m 555m → win on 5m
        // Should recognize 一色三节高: at least 3 consecutive triplets (e.g. 111m,222m,333m)
        var counts = BuildCounts("111m 222m 333m 44m 555m");
        var context = BuildContext(counts) with
        {
            PengCount = 3,
            Melds = new List<IReadOnlyList<string>>
            {
                new[] { "1m", "1m", "1m" },
                new[] { "2m", "2m", "2m" },
                new[] { "3m", "3m", "3m" }
            }
        };
        var result = FanEvaluator.Evaluate(context);
        Assert.Contains("一色三节高 24番", result.FanNames);
    }

    [Fact]
    public void SanSeSanJieGao_WithPengMelds_IsRecognized()
    {
        // Peng 111m, 222p, 333s + hand 456m 99p
        // Should recognize 三色三节高: 1m, 2p, 3s consecutive across suits
        var counts = BuildCounts("111m 222p 333s 456m 99p");
        var context = BuildContext(counts) with
        {
            PengCount = 3,
            Melds = new List<IReadOnlyList<string>>
            {
                new[] { "1m", "1m", "1m" },
                new[] { "2p", "2p", "2p" },
                new[] { "3s", "3s", "3s" }
            }
        };
        var result = FanEvaluator.Evaluate(context);
        Assert.Contains("三色三节高 8番", result.FanNames);
    }

    // ── Meld interaction audit ──────────────────────────────────
    // Verify that fan detectors handle meld-locked tiles correctly.

    [Fact]
    public void PengPengHe_NotTriggered_ByChiTilesRearrangement()
    {
        // Chi 123m + peng 111p + peng 222s + hand 345m 99m
        // Without meld awareness, 碰碰和 checks full counts for all-triplets.
        // The chi 123m tiles cannot form triplets, so IsAllTriplets should
        // correctly return false. This is safe because chi always adds +1 to
        // 3 consecutive indices — never enough for a triplet on its own.
        var counts = BuildCounts("123m 111p 222s 345m 99m");
        var context = BuildContext(counts) with
        {
            ChiCount = 1,
            PengCount = 2,
            Melds = new List<IReadOnlyList<string>>
            {
                new[] { "1m", "2m", "3m" },
                new[] { "1p", "1p", "1p" },
                new[] { "2s", "2s", "2s" }
            }
        };
        var result = FanEvaluator.Evaluate(context);
        Assert.DoesNotContain("碰碰和 6番", result.FanNames);
    }

    [Fact]
    public void PengPengHe_Recognized_WithAllPengMelds()
    {
        // Peng 111m + peng 222p + peng 333s + hand 444m 55p
        // All triplets + pair → 碰碰和
        var counts = BuildCounts("111m 222p 333s 444m 55p");
        var context = BuildContext(counts) with
        {
            PengCount = 3,
            Melds = new List<IReadOnlyList<string>>
            {
                new[] { "1m", "1m", "1m" },
                new[] { "2p", "2p", "2p" },
                new[] { "3s", "3s", "3s" }
            }
        };
        var result = FanEvaluator.Evaluate(context);
        Assert.Contains("碰碰和 6番", result.FanNames);
    }

    [Fact]
    public void PingHe_Recognized_WithChiMelds()
    {
        // Chi 123m + chi 456p + hand 789s 234m 55m
        // All sequences + pair → 平和
        var counts = BuildCounts("123m 456p 789s 234m 55m");
        var context = BuildContext(counts) with
        {
            ChiCount = 2,
            Melds = new List<IReadOnlyList<string>>
            {
                new[] { "1m", "2m", "3m" },
                new[] { "4p", "5p", "6p" }
            }
        };
        var result = FanEvaluator.Evaluate(context);
        Assert.Contains("平和 2番", result.FanNames);
    }

    [Fact]
    public void PingHe_NotRecognized_WithPengMeld()
    {
        // Peng 111m + hand 456p 789s 234m 55m → has a triplet meld, not 平和
        var counts = BuildCounts("111m 456p 789s 234m 55m");
        var context = BuildContext(counts) with
        {
            PengCount = 1,
            Melds = new List<IReadOnlyList<string>>
            {
                new[] { "1m", "1m", "1m" }
            }
        };
        var result = FanEvaluator.Evaluate(context);
        Assert.DoesNotContain("平和 2番", result.FanNames);
    }
}
