using Xunit;
using MahjongScorer.Utilities;

namespace MahjongScorer.Tests;

/// <summary>
/// Tests for edge cases, interaction rules, and concealed-triplet counting.
/// </summary>
public partial class FanTests
{
    [Fact]
    public void SeatAndRoundWind_SameTile_BothCount()
    {
        var counts = BuildCounts("EEE 123m 456p 789s 99m");
        var context = BuildContext(counts) with { SeatWind = "E", RoundWind = "E" };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("门风刻 2番", result.FanNames);
        Assert.Contains("圈风刻 2番", result.FanNames);
        Assert.DoesNotContain("幺九刻 1番", result.FanNames);
    }

    [Fact]
    public void HonorsAndSelfDrawAndFlowers_AreCounted()
    {
        var counts = BuildCounts("EEE SSS CCC 123m 99p");
        var context = BuildContext(counts) with { IsSelfDraw = true, FlowerCount = 2, SeatWind = "E", RoundWind = "S" };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("不求人 4番", result.FanNames);
        Assert.Contains("门风刻 2番", result.FanNames);
        Assert.Contains("圈风刻 2番", result.FanNames);
        Assert.Contains("箭刻 2番", result.FanNames);
        Assert.DoesNotContain("自摸 1番", result.FanNames);
        Assert.Contains("花牌 2番", result.FanNames);
    }

    [Fact]
    public void TotalFan_IsCalculatedCorrectly()
    {
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var context = BuildContext(counts) with { IsSelfDraw = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.True(result.TotalFan > 0);
        Assert.Equal(result.FanNames.Sum(GetFanPoints), result.TotalFan);
    }

    [Fact]
    public void ConcealedTriplets_Require_WinningHand()
    {
        var counts = BuildCounts("222m 888m 444p 222s 35s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.DoesNotContain("四暗刻 64番", result.FanNames);
    }

    [Fact]
    public void FourTriplets_WithSequenceSplit_DoesNotCount_SiAnKe()
    {
        var counts = BuildCounts("222m 888m 444p 222s 34s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.DoesNotContain("四暗刻 64番", result.FanNames);
    }

    [Fact]
    public void TripletCompletedByDiscard_DoesNotCount_AsConcealed()
    {
        var counts = BuildCounts("222m 888m 444p 222s 55s");
        var context = BuildContext(counts) with { IsSelfDraw = false, WinningTile = "2s" };
        var result = FanEvaluator.Evaluate(context);

        Assert.DoesNotContain("四暗刻 64番", result.FanNames);
    }

    [Fact]
    public void TripletMelds_DoNotCreate_StepSequences()
    {
        var counts = BuildCounts("111m 222m 333m 444m 55m");
        var context = BuildContext(counts) with
        {
            Melds = new List<IReadOnlyList<string>>
            {
                new[] { "1m", "1m", "1m", "1m" },
                new[] { "2m", "2m", "2m" },
                new[] { "3m", "3m", "3m" },
                new[] { "4m", "4m", "4m" }
            }
        };
        var result = FanEvaluator.Evaluate(context);

        Assert.DoesNotContain("一色三步高 16番", result.FanNames);
        Assert.DoesNotContain("一色三同顺 24番", result.FanNames);
    }

    [Fact]
    public void YiSeSanJieGao_And_YiSeSanTongShun_AreMutuallyExclusive()
    {
        var counts = BuildCounts("111m 222m 333m 456p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        var has三同顺 = result.FanNames.Contains("一色三同顺 24番");
        var has三节高 = result.FanNames.Contains("一色三节高 24番");
        Assert.False(has三同顺 && has三节高);
    }

    [Fact]
    public void WuFanHe_WhenOnlyFlowerTilesRemain()
    {
        // After exclusions, if the only remaining fan is 花牌,
        // it should still count as 无番和 because per rules "花牌不计算在内"
        var counts = BuildCounts("123m 234m 456p 678s EE");
        var context = BuildContext(counts) with { ChiCount = 1, IsSelfDraw = false, FlowerCount = 2 };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("无番和 8番", result.FanNames);
        Assert.Contains("花牌 2番", result.FanNames);
    }

    [Fact]
    public void QuanQiuRen_Requires_AllFourMeldsOpen()
    {
        // Only 1 chi open — the remaining 3 melds are concealed.
        // 全求人 requires ALL 4 melds to be open, so this should NOT count.
        var counts = BuildCounts("123m 456p 789s 111m 55p");
        var context = BuildContext(counts) with { WinningTile = "5p", ChiCount = 1, IsSelfDraw = false };
        var result = FanEvaluator.Evaluate(context);

        Assert.DoesNotContain("全求人 6番", result.FanNames);
    }

    [Fact]
    public void QuanQiuRen_Recognized_WhenAllMeldsOpen()
    {
        // 4 open melds (2 chi + 2 peng) + single wait on pair from discard = 全求人
        var counts = BuildCounts("123m 456p 789s 111m 55p");
        var context = BuildContext(counts) with { WinningTile = "5p", ChiCount = 2, PengCount = 2, IsSelfDraw = false };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("全求人 6番", result.FanNames);
    }

    [Fact]
    public void PingHe_WithTerminalPair_IsRecognized()
    {
        // 平和: 4 sequences + number-tile pair. Pair of 1m should be valid.
        var counts = BuildCounts("123m 456m 789p 234s 11m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("平和 2番", result.FanNames);
    }

    [Fact]
    public void PingHe_WithNinePair_IsRecognized()
    {
        // 平和 with pair of 9s
        var counts = BuildCounts("123m 456m 789p 234s 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("平和 2番", result.FanNames);
    }

    [Fact]
    public void YiSeSanBuGao_StepTwo_IsRecognized()
    {
        // 一色三步高 with step size 2: 123m, 345m, 567m
        var counts = BuildCounts("123m 345m 567m 111p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("一色三步高 16番", result.FanNames);
    }

    [Fact]
    public void YiSeSiBuGao_StepTwo_IsRecognized()
    {
        // 一色四步高 with step size 2: 123m, 345m, 567m, 789m
        var counts = BuildCounts("123m 345m 567m 789m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("一色四步高 32番", result.FanNames);
    }

    [Fact]
    public void PengMelds_DoNotAffect_WaitType_KanZhang()
    {
        // 碰3万、4万、5万，手牌111万+2万，胡3万 → 手中只有23万顺子，3万是边张不是坎张
        var counts = BuildCounts("333m 444m 555m 111m 23m");
        var context = BuildContext(counts) with
        {
            WinningTile = "3m",
            PengCount = 3,
            IsSelfDraw = false,
            Melds = new List<IReadOnlyList<string>>
            {
                new[] { "3m", "3m", "3m" },
                new[] { "4m", "4m", "4m" },
                new[] { "5m", "5m", "5m" }
            }
        };
        var result = FanEvaluator.Evaluate(context);

        // 3万 completes 23万 as edge wait (边张), not closed wait (坎张)
        Assert.DoesNotContain("坎张 1番", result.FanNames);
        Assert.Contains("边张 1番", result.FanNames);
    }

    [Fact]
    public void KanZhang_NotReported_WhenAlternativeDecompositionExists()
    {
        // Hand: 234万×3 + 555万 + 66万, winning on 5万
        // Decomposition 1: 234×3 + 555 + 66 → 5万 in triplet, no 坎张
        // Decomposition 2: 33 pair + 222 + 345 + 456 + 456 → 5万 in middle of 456, 坎张
        // Since a decomposition exists where 5万 is NOT 坎张, it should NOT be reported.
        var counts = BuildCounts("234m 234m 234m 555m 66m");
        var context = BuildContext(counts) with { WinningTile = "5m", IsSelfDraw = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.DoesNotContain("坎张 1番", result.FanNames);
    }

    [Fact]
    public void ThreeSuits234_And_555_And_66_SelfDraw_Has_SiAnKe()
    {
        // Hand: 234万×3 + 555万 + 66万, winning on 5万, self-draw
        // Best decomposition: 222万 + 333万 + 444万 + 555万 + 66万 → 四暗刻
        // 四暗刻 不计不求人
        var counts = BuildCounts("234m 234m 234m 555m 66m");
        var context = BuildContext(counts) with { WinningTile = "5m", IsSelfDraw = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("四暗刻 64番", result.FanNames);
        Assert.DoesNotContain("不求人 4番", result.FanNames);
    }

    private static int GetFanPoints(string fanName)
    {
        var parts = fanName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return 0;
        var number = parts[^1].Replace("番", string.Empty);
        return int.TryParse(number, out var points) ? points : 0;
    }
}
