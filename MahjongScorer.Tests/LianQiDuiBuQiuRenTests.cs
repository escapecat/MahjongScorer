using Xunit;
using MahjongScorer.Utilities;

namespace MahjongScorer.Tests;

/// <summary>
/// Verify 连七对 + self-draw interaction with 不求人.
/// </summary>
public partial class FanTests
{
    [Fact]
    public void LianQiDui_SelfDraw_Should_Have_BuQiuRen()
    {
        // 连七对 rules: "不计清一色、七对、单钓将、门前清；自摸记不求人"
        var counts = BuildCounts("11223344556677m");
        var context = BuildContext(counts) with { IsSelfDraw = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("连七对 88番", result.FanNames);
        Assert.Contains("不求人 4番", result.FanNames);
        Assert.DoesNotContain("七对 24番", result.FanNames);
        Assert.DoesNotContain("清一色 24番", result.FanNames);
        Assert.DoesNotContain("门前清 2番", result.FanNames);
        Assert.DoesNotContain("自摸 1番", result.FanNames);
    }

    [Fact]
    public void LianQiDui_NotSelfDraw_Should_Not_Have_BuQiuRen()
    {
        var counts = BuildCounts("11223344556677m");
        var context = BuildContext(counts) with { IsSelfDraw = false };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("连七对 88番", result.FanNames);
        Assert.DoesNotContain("不求人 4番", result.FanNames);
        Assert.DoesNotContain("门前清 2番", result.FanNames);
    }
}
