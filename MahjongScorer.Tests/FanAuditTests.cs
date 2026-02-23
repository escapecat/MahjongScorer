using Xunit;
using MahjongScorer.Utilities;

namespace MahjongScorer.Tests;

/// <summary>
/// Audit tests for subtle edge cases discovered during code review.
/// </summary>
public partial class FanTests
{
    // ── 四归一 multiple instances ────────────────────────────────

    [Fact]
    public void SiGuiYi_MultipleFourOfAKind_CountedSeparately()
    {
        // 1111m 2222m → two tiles with count=4 → two 四归一
        // Hand: 1m×4 + 2m×4 + 345p + 99s (4+4+3+2 = 13... need 14)
        // Actually: 1m×4 + 2m×4 + 34p + 55s = 4+4+2+2 = 12, not 14.
        // Let's use: 1111m 234m 234m 99p = 4+3+3+2 = 12... nope
        // Use: 1111m 2222m 345p 55s = 4+4+3+2+1(extra) = need exactly 14
        // 1m×4 + 2m×4 + 3p+4p+5p + 5s+5s = 14 ✓
        var counts = BuildCounts("1111m 2222m 345p 55s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        var siGuiYiCount = result.FanNames.Count(f => f == "四归一 2番");
        Assert.Equal(2, siGuiYiCount);
    }

    [Fact]
    public void SiGuiYi_SingleFourOfAKind_CountedOnce()
    {
        var counts = BuildCounts("1111m 234m 456p 789s 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        var siGuiYiCount = result.FanNames.Count(f => f == "四归一 2番");
        Assert.Equal(1, siGuiYiCount);
    }

    // ── 和绝张 situational fan ──────────────────────────────────

    [Fact]
    public void HeJueZhang_IsRecognized_WhenFlagSet()
    {
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var context = BuildContext(counts) with { IsWinningTileLast = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("和绝张 4番", result.FanNames);
    }

    [Fact]
    public void HeJueZhang_NotRecognized_WhenFlagNotSet()
    {
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.DoesNotContain("和绝张 4番", result.FanNames);
    }

    // ── WinningTile edge/closed/single detection ────────────────

    [Fact]
    public void WinningTile_EdgeWait_3m_In_123m()
    {
        var counts = BuildCounts("123m 456p 789s 111p 99s");
        var context = BuildContext(counts) with { WinningTile = "3m" };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("边张 1番", result.FanNames);
    }

    [Fact]
    public void WinningTile_ClosedWait_5m_In_456m()
    {
        var counts = BuildCounts("456m 123p 789s 111p 99s");
        var context = BuildContext(counts) with { WinningTile = "5m" };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("坎张 1番", result.FanNames);
    }

    [Fact]
    public void WinningTile_SingleWait_Pair()
    {
        var counts = BuildCounts("123m 456p 789s 111p 55s");
        var context = BuildContext(counts) with { WinningTile = "5s" };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("单钓将 1番", result.FanNames);
    }

    // ── 组合龙 with different remaining melds ───────────────────

    [Fact]
    public void ZuHeLong_WithSequenceRemainder()
    {
        // 147m + 258p + 369s + 456m + 99s
        var counts = BuildCounts("147m 258p 369s 456m 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("组合龙 12番", result.FanNames);
    }

    [Fact]
    public void ZuHeLong_WithTripletRemainder()
    {
        var counts = BuildCounts("147m 258p 369s 111s 99m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("组合龙 12番", result.FanNames);
    }

    // ── Fan results sorted by points descending ─────────────────

    [Fact]
    public void FanResults_SortedByPointsDescending()
    {
        var counts = BuildCounts("CCC FFF PPP 123m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        var points = result.FanNames.Select(GetFanPoints).ToList();
        for (var i = 1; i < points.Count; i++)
        {
            Assert.True(points[i - 1] >= points[i],
                $"Fan '{result.FanNames[i - 1]}' ({points[i - 1]}) should be >= '{result.FanNames[i]}' ({points[i]})");
        }
    }

    // ── 杠上开花 suppresses 自摸 ────────────────────────────────

    [Fact]
    public void KongDraw_SuppressesSelfDraw_WithOpenMeld()
    {
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var context = BuildContext(counts) with { IsSelfDraw = true, IsKongDraw = true, ChiCount = 1 };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("杠上开花 8番", result.FanNames);
        Assert.DoesNotContain("自摸 1番", result.FanNames);
    }

    // ── 七对 with 四归一 ────────────────────────────────────────

    [Fact]
    public void QiDui_WithFourOfAKind_Has_SiGuiYi()
    {
        // 1m×4 + 2m×2 + 3m×2 + 4m×2 + 5m×2 + 6m×2 = 14 tiles, 7 pairs with 1m×4
        var counts = BuildCounts("1111m 2233445566m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("七对 24番", result.FanNames);
        Assert.Contains("四归一 2番", result.FanNames);
    }
}
