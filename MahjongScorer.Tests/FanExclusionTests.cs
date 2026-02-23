using Xunit;
using MahjongScorer.Utilities;

namespace MahjongScorer.Tests;

/// <summary>
/// Tests that exclusion rules from rules.json are correctly applied.
/// Each high-fan pattern should suppress lower conflicting fans.
/// </summary>
public partial class FanTests
{
    // === 88番 exclusions ===

    [Fact]
    public void DaSiXi_Excludes_LowFans()
    {
        var counts = BuildCounts("EEE SSS WWW NNN CC");
        var context = BuildContext(counts) with { SeatWind = "E", RoundWind = "S" };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("大四喜 88番", result.FanNames);
        Assert.DoesNotContain("小四喜 64番", result.FanNames);
        Assert.DoesNotContain("三风刻 12番", result.FanNames);
        Assert.DoesNotContain("碰碰和 6番", result.FanNames);
        Assert.DoesNotContain("圈风刻 2番", result.FanNames);
        Assert.DoesNotContain("门风刻 2番", result.FanNames);
        Assert.DoesNotContain("幺九刻 1番", result.FanNames);
    }

    [Fact]
    public void DaSanYuan_Excludes_LowFans()
    {
        var counts = BuildCounts("CCC FFF PPP 123m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("大三元 88番", result.FanNames);
        Assert.DoesNotContain("小三元 64番", result.FanNames);
        Assert.DoesNotContain("双箭刻 6番", result.FanNames);
        Assert.DoesNotContain("箭刻 2番", result.FanNames);
    }

    [Fact]
    public void LvYiSe_WithFa_Excludes_HunYiSe()
    {
        var counts = BuildCounts("222333666s 444s FF");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("绿一色 88番", result.FanNames);
        Assert.DoesNotContain("混一色 6番", result.FanNames);
    }

    [Fact]
    public void JiuLianBaoDeng_Excludes_LowFans()
    {
        var counts = BuildCounts("1112345678999m 5m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("九莲宝灯 88番", result.FanNames);
        Assert.DoesNotContain("清一色 24番", result.FanNames);
        Assert.DoesNotContain("门前清 2番", result.FanNames);
        Assert.DoesNotContain("幺九刻 1番", result.FanNames);
    }

    [Fact]
    public void SiGang_Excludes_AllKongFans()
    {
        var counts = BuildCounts("111m 222m 333p 444s 55p");
        var context = BuildContext(counts) with { MingKongCount = 2, AnKongCount = 2 };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("四杠 88番", result.FanNames);
        Assert.DoesNotContain("三杠 32番", result.FanNames);
        Assert.DoesNotContain("碰碰和 6番", result.FanNames);
        Assert.DoesNotContain("双暗杠 6番", result.FanNames);
        Assert.DoesNotContain("双明杠 4番", result.FanNames);
        Assert.DoesNotContain("暗杠 2番", result.FanNames);
        Assert.DoesNotContain("明杠 1番", result.FanNames);
    }

    [Fact]
    public void LianQiDui_Excludes_LowFans()
    {
        var counts = BuildCounts("11223344556677m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("连七对 88番", result.FanNames);
        Assert.DoesNotContain("七对 24番", result.FanNames);
        Assert.DoesNotContain("清一色 24番", result.FanNames);
        Assert.DoesNotContain("门前清 2番", result.FanNames);
        Assert.DoesNotContain("单钓将 1番", result.FanNames);
    }

    [Fact]
    public void ShiSanYao_Excludes_LowFans()
    {
        var counts = BuildCounts("1m9m 1p9p 1s9s E S W N C F P 1m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("十三幺 88番", result.FanNames);
        Assert.DoesNotContain("混幺九 32番", result.FanNames);
        Assert.DoesNotContain("五门齐 6番", result.FanNames);
        Assert.DoesNotContain("全带幺 4番", result.FanNames);
        Assert.DoesNotContain("门前清 2番", result.FanNames);
    }

    // === 64番 exclusions ===

    [Fact]
    public void QingYaoJiu_Excludes_LowFans()
    {
        var counts = BuildCounts("111m 999m 111p 999p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("清幺九 64番", result.FanNames);
        Assert.DoesNotContain("混幺九 32番", result.FanNames);
        Assert.DoesNotContain("碰碰和 6番", result.FanNames);
        Assert.DoesNotContain("全带幺 4番", result.FanNames);
        Assert.DoesNotContain("幺九刻 1番", result.FanNames);
        Assert.DoesNotContain("无字 1番", result.FanNames);
    }

    [Fact]
    public void XiaoSiXi_Excludes_LowFans()
    {
        var counts = BuildCounts("EEE SSS WWW NN 123m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("小四喜 64番", result.FanNames);
        Assert.DoesNotContain("三风刻 12番", result.FanNames);
        Assert.DoesNotContain("幺九刻 1番", result.FanNames);
    }

    [Fact]
    public void XiaoSanYuan_Excludes_LowFans()
    {
        var counts = BuildCounts("CCC FFF PP 123m 456m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("小三元 64番", result.FanNames);
        Assert.DoesNotContain("双箭刻 6番", result.FanNames);
        Assert.DoesNotContain("箭刻 2番", result.FanNames);
    }

    [Fact]
    public void ZiYiSe_Excludes_LowFans()
    {
        var counts = BuildCounts("EEE SSS WWW CCC FF");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("字一色 64番", result.FanNames);
        Assert.DoesNotContain("混幺九 32番", result.FanNames);
        Assert.DoesNotContain("碰碰和 6番", result.FanNames);
        Assert.DoesNotContain("全带幺 4番", result.FanNames);
        Assert.DoesNotContain("缺一门 1番", result.FanNames);
        Assert.DoesNotContain("幺九刻 1番", result.FanNames);
    }

    [Fact]
    public void SiAnKe_Excludes_LowFans()
    {
        var counts = BuildCounts("111m 222m 333p 444s 55p");
        var context = BuildContext(counts) with { IsSelfDraw = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("四暗刻 64番", result.FanNames);
        Assert.DoesNotContain("碰碰和 6番", result.FanNames);
        Assert.DoesNotContain("不求人 4番", result.FanNames);
        Assert.DoesNotContain("三暗刻 16番", result.FanNames);
        Assert.DoesNotContain("双暗刻 2番", result.FanNames);
        Assert.DoesNotContain("门前清 2番", result.FanNames);
    }

    [Fact]
    public void YiSeShuangLongHui_Excludes_LowFans()
    {
        var counts = BuildCounts("123789m 123789m 55m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("一色双龙会 64番", result.FanNames);
        Assert.DoesNotContain("清一色 24番", result.FanNames);
        Assert.DoesNotContain("平和 2番", result.FanNames);
        Assert.DoesNotContain("一般高 1番", result.FanNames);
        Assert.DoesNotContain("老少副 1番", result.FanNames);
        Assert.DoesNotContain("无字 1番", result.FanNames);
        Assert.DoesNotContain("缺一门 1番", result.FanNames);
    }

    // === 48番 exclusions ===

    [Fact]
    public void YiSeSiTongShun_Excludes_LowFans()
    {
        var counts = BuildCounts("123m 123m 123m 123m 99p");
        var context = BuildContext(counts) with
        {
            ChiCount = 4,
            Melds = new List<IReadOnlyList<string>>
            {
                new[] { "1m", "2m", "3m" },
                new[] { "1m", "2m", "3m" },
                new[] { "1m", "2m", "3m" },
                new[] { "1m", "2m", "3m" }
            }
        };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("一色四同顺 48番", result.FanNames);
        Assert.DoesNotContain("一色三同顺 24番", result.FanNames);
        Assert.DoesNotContain("一般高 1番", result.FanNames);
        Assert.DoesNotContain("四归一 2番", result.FanNames);
        Assert.DoesNotContain("缺一门 1番", result.FanNames);
    }

    [Fact]
    public void YiSeSiJieGao_Excludes_LowFans()
    {
        var counts = BuildCounts("111m 222m 333m 444m 55p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("一色四节高 48番", result.FanNames);
        Assert.DoesNotContain("一色三节高 24番", result.FanNames);
        Assert.DoesNotContain("一色三同顺 24番", result.FanNames);
        Assert.DoesNotContain("碰碰和 6番", result.FanNames);
        Assert.DoesNotContain("缺一门 1番", result.FanNames);
    }

    // === 32番 exclusions ===

    [Fact]
    public void YiSeSiBuGao_Excludes_LowFans()
    {
        var counts = BuildCounts("123m 234m 345m 456m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("一色四步高 32番", result.FanNames);
        Assert.DoesNotContain("一色三步高 16番", result.FanNames);
        Assert.DoesNotContain("缺一门 1番", result.FanNames);
    }

    [Fact]
    public void SanGang_Excludes_LowFans()
    {
        var counts = BuildCounts("111m 222m 333p 444s 55p");
        var context = BuildContext(counts) with { MingKongCount = 2, AnKongCount = 1 };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("三杠 32番", result.FanNames);
        Assert.DoesNotContain("双明杠 4番", result.FanNames);
        Assert.DoesNotContain("双暗杠 6番", result.FanNames);
        Assert.DoesNotContain("暗杠 2番", result.FanNames);
        Assert.DoesNotContain("明杠 1番", result.FanNames);
    }

    [Fact]
    public void HunYaoJiu_Excludes_LowFans()
    {
        var counts = BuildCounts("111m 999p EEE CCC 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("混幺九 32番", result.FanNames);
        Assert.DoesNotContain("碰碰和 6番", result.FanNames);
        Assert.DoesNotContain("全带幺 4番", result.FanNames);
        Assert.DoesNotContain("幺九刻 1番", result.FanNames);
    }

    // === 24番 exclusions ===

    [Fact]
    public void QiDui_Excludes_LowFans()
    {
        var counts = BuildCounts("11223344556699m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("七对 24番", result.FanNames);
        Assert.DoesNotContain("门前清 2番", result.FanNames);
        Assert.DoesNotContain("不求人 4番", result.FanNames);
        Assert.DoesNotContain("单钓将 1番", result.FanNames);
    }

    [Fact]
    public void QiXingBuKao_Excludes_LowFans()
    {
        var counts = BuildCounts("147m 258p 3s E S W N C F P");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("七星不靠 24番", result.FanNames);
        Assert.DoesNotContain("全不靠 12番", result.FanNames);
        Assert.DoesNotContain("五门齐 6番", result.FanNames);
        Assert.DoesNotContain("不求人 4番", result.FanNames);
        Assert.DoesNotContain("门前清 2番", result.FanNames);
    }

    [Fact]
    public void QuanShuangKe_Excludes_LowFans()
    {
        var counts = BuildCounts("222m 444m 666p 888s 22p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("全双刻 24番", result.FanNames);
        Assert.DoesNotContain("碰碰和 6番", result.FanNames);
        Assert.DoesNotContain("断幺 2番", result.FanNames);
        Assert.DoesNotContain("无字 1番", result.FanNames);
    }

    [Fact]
    public void QingYiSe_Excludes_LowFans()
    {
        var counts = BuildCounts("123456789m 111m 22m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("清一色 24番", result.FanNames);
        Assert.DoesNotContain("缺一门 1番", result.FanNames);
        Assert.DoesNotContain("无字 1番", result.FanNames);
    }

    [Fact]
    public void YiSeSanTongShun_Excludes_LowFans()
    {
        var counts = BuildCounts("111m 222m 333m 456m 99p");
        var context = BuildContext(counts) with
        {
            ChiCount = 3,
            Melds = new List<IReadOnlyList<string>>
            {
                new[] { "1m", "2m", "3m" },
                new[] { "1m", "2m", "3m" },
                new[] { "1m", "2m", "3m" }
            }
        };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("一色三同顺 24番", result.FanNames);
        Assert.DoesNotContain("一色三节高 24番", result.FanNames);
    }

    [Fact]
    public void QuanDa_Excludes_LowFans()
    {
        var counts = BuildCounts("789m 789p 789s 777m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("全大 24番", result.FanNames);
        Assert.DoesNotContain("大于五 12番", result.FanNames);
        Assert.DoesNotContain("无字 1番", result.FanNames);
    }

    [Fact]
    public void QuanZhong_Excludes_LowFans()
    {
        var counts = BuildCounts("456m 456p 456s 444m 66p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("全中 24番", result.FanNames);
        Assert.DoesNotContain("断幺 2番", result.FanNames);
        Assert.DoesNotContain("无字 1番", result.FanNames);
    }

    [Fact]
    public void QuanXiao_Excludes_LowFans()
    {
        var counts = BuildCounts("123m 123p 123s 111m 22p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("全小 24番", result.FanNames);
        Assert.DoesNotContain("小于五 12番", result.FanNames);
        Assert.DoesNotContain("无字 1番", result.FanNames);
    }

    // === 16番 exclusions ===

    [Fact]
    public void QingLong_Excludes_LowFans()
    {
        var counts = BuildCounts("123m 456m 789m 111p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("清龙 16番", result.FanNames);
        Assert.DoesNotContain("连六 1番", result.FanNames);
        Assert.DoesNotContain("老少副 1番", result.FanNames);
    }

    [Fact]
    public void SanSeShuangLongHui_Excludes_LowFans()
    {
        var counts = BuildCounts("123m 789m 123p 789p 55s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("三色双龙会 16番", result.FanNames);
        Assert.DoesNotContain("喜相逢 1番", result.FanNames);
        Assert.DoesNotContain("老少副 1番", result.FanNames);
        Assert.DoesNotContain("平和 2番", result.FanNames);
        Assert.DoesNotContain("无字 1番", result.FanNames);
    }

    [Fact]
    public void QuanDaiWu_Excludes_LowFans()
    {
        var counts = BuildCounts("345m 456m 567p 555s 55m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("全带五 16番", result.FanNames);
        Assert.DoesNotContain("断幺 2番", result.FanNames);
        Assert.DoesNotContain("无字 1番", result.FanNames);
    }

    [Fact]
    public void SanTongKe_Excludes_ShuangTongKe()
    {
        var counts = BuildCounts("111m 111p 111s 234m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("三同刻 16番", result.FanNames);
        Assert.DoesNotContain("双同刻 2番", result.FanNames);
    }

    [Fact]
    public void SanAnKe_Excludes_ShuangAnKe()
    {
        var counts = BuildCounts("111m 222m 333p 456s 55s");
        var context = BuildContext(counts) with { IsSelfDraw = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("三暗刻 16番", result.FanNames);
        Assert.DoesNotContain("双暗刻 2番", result.FanNames);
    }

    // === 12番 exclusions ===

    [Fact]
    public void QuanBuKao_Excludes_LowFans()
    {
        var counts = BuildCounts("147m 258p 1s E S W N C F P");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("全不靠 12番", result.FanNames);
        Assert.DoesNotContain("五门齐 6番", result.FanNames);
        Assert.DoesNotContain("门前清 2番", result.FanNames);
    }

    [Fact]
    public void DaYuWu_Excludes_WuZi()
    {
        var counts = BuildCounts("678m 789m 678p 789s 88m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("大于五 12番", result.FanNames);
        Assert.DoesNotContain("无字 1番", result.FanNames);
    }

    [Fact]
    public void XiaoYuWu_Excludes_WuZi()
    {
        var counts = BuildCounts("123m 234m 123p 234s 11m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("小于五 12番", result.FanNames);
        Assert.DoesNotContain("无字 1番", result.FanNames);
    }

    [Fact]
    public void SanFengKe_Excludes_QueYiMen()
    {
        var counts = BuildCounts("EEE SSS WWW 123m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("三风刻 12番", result.FanNames);
        Assert.DoesNotContain("缺一门 1番", result.FanNames);
    }

    // === 8番 exclusions ===

    [Fact]
    public void HuaLong_Excludes_LianLiu_And_LaoShaoFu()
    {
        var counts = BuildCounts("123m 456m 456p 789s 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("花龙 8番", result.FanNames);
        Assert.DoesNotContain("连六 1番", result.FanNames);
        Assert.DoesNotContain("老少副 1番", result.FanNames);
    }

    [Fact]
    public void TuiBuDao_Excludes_QueYiMen()
    {
        var counts = BuildCounts("123p 345p 456s 888s PP");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("推不倒 8番", result.FanNames);
        Assert.DoesNotContain("缺一门 1番", result.FanNames);
    }

    [Fact]
    public void SanSeSanTongShun_Excludes_XiXiangFeng()
    {
        var counts = BuildCounts("123m 123p 123s 456m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("三色三同顺 8番", result.FanNames);
        Assert.DoesNotContain("喜相逢 1番", result.FanNames);
    }

    [Fact]
    public void MiaoShouHuiChun_Excludes_ZiMo()
    {
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var context = BuildContext(counts) with { IsSelfDraw = true, IsLastTile = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("妙手回春 8番", result.FanNames);
        Assert.DoesNotContain("自摸 1番", result.FanNames);
    }

    [Fact]
    public void GangShangKaiHua_Excludes_ZiMo()
    {
        var counts = BuildCounts("1111m 123p 456s 789m 99p");
        var context = BuildContext(counts) with { IsSelfDraw = true, IsKongDraw = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("杠上开花 8番", result.FanNames);
        Assert.DoesNotContain("自摸 1番", result.FanNames);
    }

    [Fact]
    public void QiangGangHe_Excludes_HeJueZhang()
    {
        var counts = BuildCounts("1111m 123p 456s 789m 99p");
        var context = BuildContext(counts) with { IsRobbingKong = true, IsWinningTileLast = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("抢杠和 8番", result.FanNames);
        Assert.DoesNotContain("和绝张 4番", result.FanNames);
    }

    // === 6番 exclusions ===

    [Fact]
    public void HunYiSe_Excludes_QueYiMen()
    {
        var counts = BuildCounts("123456m 789m EEE 99m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("混一色 6番", result.FanNames);
        Assert.DoesNotContain("缺一门 1番", result.FanNames);
    }

    [Fact]
    public void QuanQiuRen_Excludes_DanDiaoJiang()
    {
        var counts = BuildCounts("123m 456p 789s 111m 55p");
        var context = BuildContext(counts) with { WinningTile = "5p", ChiCount = 2, PengCount = 2, IsSelfDraw = false };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("全求人 6番", result.FanNames);
        Assert.DoesNotContain("单钓将 1番", result.FanNames);
    }

    [Fact]
    public void ShuangAnGang_Excludes_LowFans()
    {
        var counts = BuildCounts("111m 222m 333p 444s 55p");
        var context = BuildContext(counts) with { AnKongCount = 2 };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("双暗杠 6番", result.FanNames);
        Assert.DoesNotContain("暗杠 2番", result.FanNames);
        Assert.DoesNotContain("双暗刻 2番", result.FanNames);
    }

    [Fact]
    public void ShuangJianKe_Excludes_JianKe()
    {
        var counts = BuildCounts("CCC FFF 123m 456p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("双箭刻 6番", result.FanNames);
        Assert.DoesNotContain("箭刻 2番", result.FanNames);
    }

    // === 4番 exclusions ===

    [Fact]
    public void BuQiuRen_Excludes_MenQianQing_And_ZiMo()
    {
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var context = BuildContext(counts) with { IsSelfDraw = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("不求人 4番", result.FanNames);
        Assert.DoesNotContain("门前清 2番", result.FanNames);
        Assert.DoesNotContain("自摸 1番", result.FanNames);
    }

    [Fact]
    public void ShuangMingGang_Excludes_MingGang()
    {
        var counts = BuildCounts("111m 222m 333p 444s 55p");
        var context = BuildContext(counts) with { MingKongCount = 2 };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("双明杠 4番", result.FanNames);
        Assert.DoesNotContain("明杠 1番", result.FanNames);
    }

    // === 2番 exclusions ===

    [Fact]
    public void MenFengKe_Excludes_YaoJiuKe()
    {
        var counts = BuildCounts("EEE 123m 456p 789s 99m");
        var context = BuildContext(counts) with { SeatWind = "E" };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("门风刻 2番", result.FanNames);
        Assert.DoesNotContain("幺九刻 1番", result.FanNames);
    }

    [Fact]
    public void MenFengKe_DoesNotSuppress_OtherYaoJiuKe()
    {
        var counts = BuildCounts("EEE 111m 234p 567s 99p");
        var context = BuildContext(counts) with { SeatWind = "E" };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("门风刻 2番", result.FanNames);
        Assert.Contains("幺九刻 1番", result.FanNames);
    }

    [Fact]
    public void PingHe_Excludes_WuZi()
    {
        var counts = BuildCounts("123m 456m 789p 234s 55s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("平和 2番", result.FanNames);
        Assert.DoesNotContain("无字 1番", result.FanNames);
    }

    [Fact]
    public void DuanYao_Excludes_WuZi()
    {
        var counts = BuildCounts("234m 345p 456s 678m 55p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("断幺 2番", result.FanNames);
        Assert.DoesNotContain("无字 1番", result.FanNames);
    }
}
