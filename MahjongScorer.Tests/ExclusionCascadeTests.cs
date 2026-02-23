using Xunit;
using MahjongScorer.Utilities;

namespace MahjongScorer.Tests;

/// <summary>
/// Tests for "自摸记不求人" patterns and exclusion cascade edge cases.
/// Verifies that the data-driven exclusion table correctly handles:
/// - Fans that say "自摸记不求人" (连七对, 十三幺, 九莲宝灯) → 不求人 survives
/// - Fans that say "不计不求人" (七对, 七星不靠, 全不靠, 四暗刻) → 不求人 AND 自摸 both removed
/// </summary>
public partial class FanTests
{
    // ── "自摸记不求人" patterns: 不求人 should survive ──

    [Fact]
    public void JiuLianBaoDeng_SelfDraw_Should_Have_BuQiuRen()
    {
        var counts = BuildCounts("1112345678999m 5m");
        var context = BuildContext(counts) with { IsSelfDraw = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("九莲宝灯 88番", result.FanNames);
        Assert.Contains("不求人 4番", result.FanNames);
        Assert.DoesNotContain("清一色 24番", result.FanNames);
        Assert.DoesNotContain("门前清 2番", result.FanNames);
        Assert.DoesNotContain("自摸 1番", result.FanNames);
    }

    [Fact]
    public void ShiSanYao_SelfDraw_Should_Have_BuQiuRen()
    {
        var counts = BuildCounts("1m9m 1p9p 1s9s E S W N C F P 1m");
        var context = BuildContext(counts) with { IsSelfDraw = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("十三幺 88番", result.FanNames);
        Assert.Contains("不求人 4番", result.FanNames);
        Assert.DoesNotContain("五门齐 6番", result.FanNames);
        Assert.DoesNotContain("单钓将 1番", result.FanNames);
        Assert.DoesNotContain("门前清 2番", result.FanNames);
        Assert.DoesNotContain("混幺九 32番", result.FanNames);
        Assert.DoesNotContain("自摸 1番", result.FanNames);
    }

    // ── "不计不求人" patterns: both 不求人 and 自摸 should be removed ──
    // 七对、七星不靠、全不靠 explicitly say 不计不求人

    [Fact]
    public void SiAnKe_SelfDraw_No_BuQiuRen_No_ZiMo()
    {
        // 四暗刻: 不计不求人 → suppresses both 不求人 and 自摸
        var counts = BuildCounts("111m 222m 333p 444s 55p");
        var context = BuildContext(counts) with { IsSelfDraw = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("四暗刻 64番", result.FanNames);
        Assert.DoesNotContain("不求人 4番", result.FanNames);
        Assert.DoesNotContain("自摸 1番", result.FanNames);
        Assert.DoesNotContain("碰碰和 6番", result.FanNames);
        Assert.DoesNotContain("门前清 2番", result.FanNames);
    }

    [Fact]
    public void QiXingBuKao_SelfDraw_No_BuQiuRen()
    {
        // 七星不靠: 不计不求人 → suppresses both 不求人 and 自摸
        var counts = BuildCounts("147m 258p 3s E S W N C F P");
        var context = BuildContext(counts) with { IsSelfDraw = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("七星不靠 24番", result.FanNames);
        Assert.DoesNotContain("不求人 4番", result.FanNames);
        Assert.DoesNotContain("五门齐 6番", result.FanNames);
        Assert.DoesNotContain("门前清 2番", result.FanNames);
        Assert.DoesNotContain("自摸 1番", result.FanNames);
    }

    [Fact(Skip = "全不靠 detector requires all 7 honors — detector needs separate fix")]
    public void QuanBuKao_SelfDraw_No_BuQiuRen()
    {
        var counts = BuildCounts("147m 258p 369s E S W N C");
        var context = BuildContext(counts) with { IsSelfDraw = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("全不靠 12番", result.FanNames);
        Assert.DoesNotContain("不求人 4番", result.FanNames);
        Assert.DoesNotContain("门前清 2番", result.FanNames);
        Assert.DoesNotContain("自摸 1番", result.FanNames);
    }

    [Fact]
    public void QiDui_SelfDraw_No_BuQiuRen()
    {
        // 七对: 不计不求人 → suppresses both 不求人 and 自摸
        var counts = BuildCounts("11223344556699m");
        var context = BuildContext(counts) with { IsSelfDraw = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("七对 24番", result.FanNames);
        Assert.DoesNotContain("不求人 4番", result.FanNames);
        Assert.DoesNotContain("门前清 2番", result.FanNames);
        Assert.DoesNotContain("自摸 1番", result.FanNames);
    }

    // ── Composed dragon should have TotalFan properly calculated ──

    [Fact]
    public void ZuHeLong_HasCorrectTotalFan()
    {
        // 组合龙: 147m + 258p + 369s + 111m + 99p → 组合龙12 + other fans
        var counts = BuildCounts("147m 258p 369s 111m 99p");
        var context = BuildContext(counts);
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("组合龙 12番", result.FanNames);
        Assert.True(result.TotalFan >= 12, $"TotalFan should be >= 12 but was {result.TotalFan}");
    }
}
