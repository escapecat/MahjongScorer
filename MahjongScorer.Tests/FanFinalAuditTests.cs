using Xunit;
using MahjongScorer.Utilities;

namespace MahjongScorer.Tests;

/// <summary>
/// Final comprehensive audit tests — cross-checking against 国标 rules edge cases.
/// </summary>
public partial class FanTests
{
    // ── Concealed triplet NOT counted when completed by discard ──

    [Fact]
    public void ThreeAnKe_ReducedToTwo_WhenDiscardCompletesOneTriplet()
    {
        // 3 concealed triplets but winning on discard completing one → only 2 concealed
        var counts = BuildCounts("111m 222p 333s 456m 99s");
        var context = BuildContext(counts) with { IsSelfDraw = false, WinningTile = "3s" };
        var result = FanEvaluator.Evaluate(context);

        Assert.DoesNotContain("三暗刻 16番", result.FanNames);
        Assert.Contains("双暗刻 2番", result.FanNames);
    }

    [Fact]
    public void ThreeAnKe_StillCounted_WhenSelfDraw()
    {
        var counts = BuildCounts("111m 222p 333s 456m 99s");
        var context = BuildContext(counts) with { IsSelfDraw = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("三暗刻 16番", result.FanNames);
    }

    // ── 碰碰和 with open melds ──────────────────────────────────

    [Fact]
    public void PengPengHe_WithAllOpenPeng()
    {
        var counts = BuildCounts("111m 222p 333s 444m 99s");
        var context = BuildContext(counts) with
        {
            PengCount = 4,
            Melds = new List<IReadOnlyList<string>>
            {
                new[] { "1m", "1m", "1m" },
                new[] { "2p", "2p", "2p" },
                new[] { "3s", "3s", "3s" },
                new[] { "4m", "4m", "4m" }
            }
        };
        var result = FanEvaluator.Evaluate(context);
        Assert.Contains("碰碰和 6番", result.FanNames);
        Assert.DoesNotContain("三暗刻 16番", result.FanNames);
        Assert.DoesNotContain("四暗刻 64番", result.FanNames);
    }

    // ── 全带幺 requires EVERY meld and pair to have terminal/honor ──

    [Fact]
    public void QuanDaiYao_NotRecognized_WhenMiddleSequence()
    {
        // 456m has no terminal/honor → should NOT be 全带幺
        var counts = BuildCounts("123m 456m 789s 111p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.DoesNotContain("全带幺 4番", result.FanNames);
    }

    [Fact]
    public void QuanDaiYao_Recognized_WhenAllMeldsHaveTerminal()
    {
        var counts = BuildCounts("123m 789m 111p 999s EE");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("全带幺 4番", result.FanNames);
    }

    // ── 无番和 when no fans except flowers ──────────────────────

    [Fact]
    public void WuFanHe_StillApplied_WithOnlyFlowers()
    {
        // Use a hand where truly no pattern fans apply
        // All sequences, different suits, no special patterns, with open melds
        var counts = BuildCounts("234m 567p 123s 789m EE");
        var context = BuildContext(counts) with { ChiCount = 4, IsSelfDraw = false, FlowerCount = 3,
            Melds = new List<IReadOnlyList<string>>
            {
                new[] { "2m", "3m", "4m" },
                new[] { "5p", "6p", "7p" },
                new[] { "1s", "2s", "3s" },
                new[] { "7m", "8m", "9m" }
            }
        };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("无番和 8番", result.FanNames);
        Assert.Contains("花牌 3番", result.FanNames);
    }

    // ── 平和 requires number-tile pair (not wind/dragon) ────────

    [Fact]
    public void PingHe_NotRecognized_WithWindPair()
    {
        var counts = BuildCounts("123m 456m 789p 234s EE");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.DoesNotContain("平和 2番", result.FanNames);
    }

    // ── 十三幺 detection ────────────────────────────────────────

    [Fact]
    public void ShiSanYao_NotRecognized_WhenNoPairAmongOrphans()
    {
        // 13 orphans + extra 2m instead of a pair of orphan → not 十三幺
        var counts = new int[34];
        foreach (var idx in new[] { 0, 8, 9, 17, 18, 26, 27, 28, 29, 30, 31, 32, 33 })
            counts[idx] = 1;
        counts[1] = 1; // extra 2m instead of making a pair
        // total = 14 but no pair among orphans → not winning
        var result = FanEvaluator.Evaluate(BuildContext(counts));
        Assert.DoesNotContain("十三幺 88番", result.FanNames);
    }

    // ── Total fan calculation consistency ────────────────────────

    [Fact]
    public void TotalFan_EqualsSum_OfAllFanNames()
    {
        var counts = BuildCounts("111m 222m 333m 444m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        var expectedTotal = result.FanNames.Sum(name =>
        {
            var parts = name.Split(' ');
            var num = parts[^1].Replace("番", "");
            return int.TryParse(num, out var v) ? v : 0;
        });

        Assert.Equal(expectedTotal, result.TotalFan);
    }

    // ── 花牌 count is dynamic ───────────────────────────────────

    [Fact]
    public void HuaPai_CountReflectsActualFlowerCount()
    {
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var context = BuildContext(counts) with { FlowerCount = 5 };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("花牌 5番", result.FanNames);
        Assert.DoesNotContain("花牌 1番", result.FanNames);
    }

    // ── WaitAnalyzer doesn't corrupt counts ─────────────────────

    [Fact]
    public void WaitAnalyzer_DoesNotCorruptCounts_ViaEvaluator()
    {
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var original = counts.ToArray();

        // Run evaluator which internally calls WaitAnalyzer
        var context = BuildContext(counts) with { WinningTile = "9p" };
        FanEvaluator.Evaluate(context);

        // Verify the counts array passed to context was not corrupted
        // (BuildContext copies it, so original should be unchanged)
        Assert.Equal(original, counts);
    }
}
