using Xunit;
using MahjongScorer.Utilities;

namespace MahjongScorer.Tests;

/// <summary>
/// Tests that individual fan patterns are correctly recognized.
/// </summary>
public partial class FanTests
{
    // === 88番 ===

    [Fact]
    public void AllGreen_IsRecognized()
    {
        var counts = BuildCounts("222333444666s FF");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("绿一色 88番", result.FanNames);
    }

    [Fact]
    public void AllGreen_WithoutFa_CountsQingYiSe()
    {
        var counts = BuildCounts("22233344466688s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("绿一色 88番", result.FanNames);
        Assert.Contains("清一色 24番", result.FanNames);
    }

    [Fact]
    public void NineGates_IsRecognized()
    {
        var counts = BuildCounts("1112345678999m 5m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("九莲宝灯 88番", result.FanNames);
    }

    [Fact]
    public void LianQiDui_IsRecognized()
    {
        var counts = BuildCounts("11223344556677m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("连七对 88番", result.FanNames);
    }

    [Fact]
    public void ShiSanYao_IsRecognized()
    {
        var counts = BuildCounts("1m9m 1p9p 1s9s E S W N C F P 1m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("十三幺 88番", result.FanNames);
    }

    [Fact]
    public void BigFourWinds_IsRecognized()
    {
        var counts = BuildCounts("EEE SSS WWW NNN CC");
        var context = BuildContext(counts) with { SeatWind = "E", RoundWind = "S" };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("大四喜 88番", result.FanNames);
    }

    [Fact]
    public void BigThreeDragons_IsRecognized()
    {
        var counts = BuildCounts("CCC FFF PPP 123m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("大三元 88番", result.FanNames);
    }

    [Fact]
    public void SiGang_IsRecognized()
    {
        var counts = BuildCounts("111m 222m 333p 444s 55p");
        var context = BuildContext(counts) with { MingKongCount = 4 };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("四杠 88番", result.FanNames);
    }

    // === 64番 ===

    [Fact]
    public void QingYaoJiu_IsRecognized()
    {
        var counts = BuildCounts("111m 999m 111p 999p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("清幺九 64番", result.FanNames);
    }

    [Fact]
    public void ZiYiSe_IsRecognized()
    {
        var counts = BuildCounts("EEE SSS WWW CCC FF");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("字一色 64番", result.FanNames);
    }

    [Fact]
    public void XiaoSiXi_IsRecognized()
    {
        var counts = BuildCounts("EEE SSS WWW NN 123m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("小四喜 64番", result.FanNames);
    }

    [Fact]
    public void XiaoSanYuan_IsRecognized()
    {
        var counts = BuildCounts("CCC FFF PP 123m 456m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("小三元 64番", result.FanNames);
    }

    [Fact]
    public void SiAnKe_IsRecognized()
    {
        var counts = BuildCounts("111m 222m 333p 444s 55p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("四暗刻 64番", result.FanNames);
    }

    [Fact]
    public void YiSeShuangLongHui_IsRecognized()
    {
        var counts = BuildCounts("123789m 123789m 55m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("一色双龙会 64番", result.FanNames);
    }

    // === 48番 ===

    [Fact]
    public void YiSeSiTongShun_IsRecognized()
    {
        // 123m×4 + 99p: four identical sequences in one suit
        // Note: With 3× 四归一, the triplet decomposition can score higher.
        // Use chi melds to lock the sequences so the decomposer treats them as sequences.
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
    }

    [Fact]
    public void YiSeSiJieGao_IsRecognized()
    {
        var counts = BuildCounts("111m 222m 333m 444m 55p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("一色四节高 48番", result.FanNames);
    }

    // === 32番 ===

    [Fact]
    public void YiSeSiBuGao_IsRecognized()
    {
        var counts = BuildCounts("123m 234m 345m 456m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("一色四步高 32番", result.FanNames);
    }

    [Fact]
    public void SanGang_IsRecognized()
    {
        var counts = BuildCounts("111m 222m 333p 444s 55p");
        var context = BuildContext(counts) with { MingKongCount = 2, AnKongCount = 1 };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("三杠 32番", result.FanNames);
    }

    [Fact]
    public void HunYaoJiu_IsRecognized()
    {
        var counts = BuildCounts("111m 999p EEE CCC 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("混幺九 32番", result.FanNames);
    }

    // === 24番 ===

    [Fact]
    public void SevenPairs_IsRecognized()
    {
        var counts = BuildCounts("11223344556699m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("七对 24番", result.FanNames);
        Assert.Contains("清一色 24番", result.FanNames);
    }

    [Fact]
    public void QiXingBuKao_IsRecognized()
    {
        var counts = BuildCounts("147m 258p 3s E S W N C F P");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("七星不靠 24番", result.FanNames);
    }

    [Fact]
    public void AllEvenTriplets_IsRecognized()
    {
        var counts = BuildCounts("222m 444m 666m 888m 22p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("全双刻 24番", result.FanNames);
    }

    [Fact]
    public void PureSuit_IsRecognized()
    {
        var counts = BuildCounts("123456789m 111m 22m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("清一色 24番", result.FanNames);
    }

    [Fact]
    public void YiSeSanTongShun_IsRecognized()
    {
        // 123m×3 + 456p + 99s: three identical sequences in one suit
        // Note: 123m×3 can also decompose as triplets 111m+222m+333m (一色三节高).
        // The evaluator picks the highest-scoring decomposition.
        // With triplet decomposition: 一色三节高(24) + 碰碰和(6) = higher score.
        // Use chi melds to lock the sequences so the decomposer treats them as sequences.
        var counts = BuildCounts("123m 123m 123m 456p 99s");
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
    }

    [Fact]
    public void AllBig_IsRecognized()
    {
        var counts = BuildCounts("789m 789p 789s 777m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("全大 24番", result.FanNames);
    }

    [Fact]
    public void AllMiddle_IsRecognized()
    {
        var counts = BuildCounts("456m 456p 456s 444m 66p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("全中 24番", result.FanNames);
    }

    [Fact]
    public void AllSmall_IsRecognized()
    {
        var counts = BuildCounts("123m 123p 123s 111m 22p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("全小 24番", result.FanNames);
    }

    // === 16番 ===

    [Fact]
    public void PureStraight_IsRecognized()
    {
        var counts = BuildCounts("123m 456m 789m 111p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("清龙 16番", result.FanNames);
    }

    [Fact]
    public void SanSeShuangLongHui_IsRecognized()
    {
        var counts = BuildCounts("123m 789m 123p 789p 55s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("三色双龙会 16番", result.FanNames);
    }

    [Fact]
    public void YiSeSanBuGao_IsRecognized()
    {
        var counts = BuildCounts("123m 234m 345m 456p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("一色三步高 16番", result.FanNames);
    }

    [Fact]
    public void QuanDaiWu_IsRecognized()
    {
        var counts = BuildCounts("345m 456m 567p 555s 55m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("全带五 16番", result.FanNames);
    }

    [Fact]
    public void SanTongKe_IsRecognized()
    {
        var counts = BuildCounts("111m 111p 111s 234m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("三同刻 16番", result.FanNames);
    }

    [Fact]
    public void SanAnKe_IsRecognized()
    {
        var counts = BuildCounts("111m 222m 333p 456s 55s");
        var context = BuildContext(counts) with { IsSelfDraw = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("三暗刻 16番", result.FanNames);
    }

    // === 12番 ===

    [Fact]
    public void QuanBuKao_IsRecognized()
    {
        var counts = BuildCounts("147m 258p 1s E S W N C F P");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("全不靠 12番", result.FanNames);
    }

    [Fact]
    public void ZuHeLong_IsRecognized()
    {
        var counts = BuildCounts("147m 258p 369s 111m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("组合龙 12番", result.FanNames);
    }

    [Fact]
    public void DaYuWu_IsRecognized()
    {
        var counts = BuildCounts("678m 789m 678p 789s 88m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("大于五 12番", result.FanNames);
    }

    [Fact]
    public void XiaoYuWu_IsRecognized()
    {
        var counts = BuildCounts("123m 234m 123p 234s 11m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("小于五 12番", result.FanNames);
    }

    [Fact]
    public void SanFengKe_IsRecognized()
    {
        var counts = BuildCounts("EEE SSS WWW 123m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("三风刻 12番", result.FanNames);
    }

    // === 8番 ===

    [Fact]
    public void HuaLong_IsRecognized()
    {
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("花龙 8番", result.FanNames);
    }

    [Fact]
    public void TuiBuDao_IsRecognized()
    {
        // 推不倒 tiles: pin(1,2,3,4,5,8,9), sou(2,4,5,6,8,9), P
        // Valid hand: 123p(seq) + 459p(seq? no, 4-5-9 not consecutive)
        // Let's use: 123p + 345p + 456s + 888s + PP = 3+3+3+3+2 = 14
        // Check: 1p,2p,3p,3p,4p,5p,4s,5s,6s,8s,8s,8s,P,P — all in pushed-down set ✓
        var counts = BuildCounts("123p 345p 456s 888s PP");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("推不倒 8番", result.FanNames);
    }

    [Fact]
    public void SanSeSanTongShun_IsRecognized()
    {
        var counts = BuildCounts("123m 123p 123s 456m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("三色三同顺 8番", result.FanNames);
    }

    [Fact]
    public void SanSeSanJieGao_IsRecognized()
    {
        var counts = BuildCounts("111m 222p 333s 456m 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("三色三节高 8番", result.FanNames);
    }

    [Fact]
    public void MiaoShouHuiChun_IsRecognized()
    {
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var context = BuildContext(counts) with { IsSelfDraw = true, IsLastTile = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("妙手回春 8番", result.FanNames);
    }

    [Fact]
    public void HaiDiLaoYue_IsRecognized()
    {
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var context = BuildContext(counts) with { IsSelfDraw = false, IsLastTile = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("海底捞月 8番", result.FanNames);
    }

    [Fact]
    public void GangShangKaiHua_IsRecognized()
    {
        var counts = BuildCounts("1111m 123p 456s 789m 99p");
        var context = BuildContext(counts) with { IsSelfDraw = true, IsKongDraw = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("杠上开花 8番", result.FanNames);
    }

    [Fact]
    public void QiangGangHe_IsRecognized()
    {
        var counts = BuildCounts("1111m 123p 456s 789m 99p");
        var context = BuildContext(counts) with { IsRobbingKong = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("抢杠和 8番", result.FanNames);
    }

    [Fact]
    public void WuFanHe_IsRecognized()
    {
        var counts = BuildCounts("123m 234m 456p 678s EE");
        var context = BuildContext(counts) with { ChiCount = 1, IsSelfDraw = false };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("无番和 8番", result.FanNames);
        Assert.Equal(8, result.TotalFan);
    }

    // === 6番 ===

    [Fact]
    public void PengPengHe_IsRecognized()
    {
        var counts = BuildCounts("111m 222p 333s 444m 55s");
        var context = BuildContext(counts) with
        {
            PengCount = 2,
            Melds = new List<IReadOnlyList<string>>
            {
                new[] { "1m", "1m", "1m" },
                new[] { "2p", "2p", "2p" }
            }
        };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("碰碰和 6番", result.FanNames);
    }

    [Fact]
    public void HalfFlush_IsRecognized()
    {
        var counts = BuildCounts("123456m 789m EEE 99m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("混一色 6番", result.FanNames);
    }

    [Fact]
    public void SanSeSanBuGao_IsRecognized()
    {
        var counts = BuildCounts("123m 234p 345s 456m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("三色三步高 6番", result.FanNames);
    }

    [Fact]
    public void WuMenQi_IsRecognized()
    {
        var counts = BuildCounts("123m 456p 789s EEE CC");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("五门齐 6番", result.FanNames);
    }

    [Fact]
    public void QuanQiuRen_IsRecognized()
    {
        var counts = BuildCounts("123m 456p 789s 111m 55p");
        var context = BuildContext(counts) with { WinningTile = "5p", ChiCount = 2, PengCount = 2, IsSelfDraw = false };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("全求人 6番", result.FanNames);
    }

    [Fact]
    public void ShuangAnGang_IsRecognized()
    {
        var counts = BuildCounts("111m 222m 333p 444s 55p");
        var context = BuildContext(counts) with { AnKongCount = 2 };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("双暗杠 6番", result.FanNames);
    }

    [Fact]
    public void ShuangJianKe_IsRecognized()
    {
        var counts = BuildCounts("CCC FFF 123m 456p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("双箭刻 6番", result.FanNames);
    }

    // === 4番 ===

    [Fact]
    public void QuanDaiYao_IsRecognized()
    {
        var counts = BuildCounts("123m 789m 111p 999s 11m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("全带幺 4番", result.FanNames);
    }

    [Fact]
    public void BuQiuRen_IsRecognized()
    {
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var context = BuildContext(counts) with { IsSelfDraw = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("不求人 4番", result.FanNames);
    }

    [Fact]
    public void ShuangMingGang_IsRecognized()
    {
        var counts = BuildCounts("111m 222m 333p 444s 55p");
        var context = BuildContext(counts) with { MingKongCount = 2 };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("双明杠 4番", result.FanNames);
    }

    [Fact]
    public void HeJueZhang_IsRecognized()
    {
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var context = BuildContext(counts) with { IsWinningTileLast = true };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("和绝张 4番", result.FanNames);
    }

    // === 2番 ===

    [Fact]
    public void AllSimples_IsRecognized()
    {
        var counts = BuildCounts("234m 345p 456s 678m 55p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("断幺 2番", result.FanNames);
    }

    [Fact]
    public void MenQianQing_IsRecognized()
    {
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var context = BuildContext(counts) with { IsSelfDraw = false };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("门前清 2番", result.FanNames);
    }

    [Fact]
    public void PingHe_IsRecognized()
    {
        var counts = BuildCounts("123m 456m 789p 234s 55s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("平和 2番", result.FanNames);
    }

    [Fact]
    public void SiGuiYi_IsRecognized()
    {
        var counts = BuildCounts("1111m 123p 456s 789m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("四归一 2番", result.FanNames);
    }

    [Fact]
    public void ShuangTongKe_IsRecognized()
    {
        var counts = BuildCounts("111m 111p 234m 456p 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("双同刻 2番", result.FanNames);
    }

    [Fact]
    public void AnGang_IsRecognized()
    {
        var counts = BuildCounts("111m 222m 333p 456s 55p");
        var context = BuildContext(counts) with { AnKongCount = 1 };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("暗杠 2番", result.FanNames);
    }

    [Fact]
    public void JianKe_IsRecognized()
    {
        var counts = BuildCounts("CCC 123m 456p 789s 99m");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("箭刻 2番", result.FanNames);
    }

    [Fact]
    public void MenFengKe_IsRecognized()
    {
        var counts = BuildCounts("EEE 123m 456p 789s 99m");
        var context = BuildContext(counts) with { SeatWind = "E" };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("门风刻 2番", result.FanNames);
    }

    [Fact]
    public void QuanFengKe_IsRecognized()
    {
        var counts = BuildCounts("SSS 123m 456p 789s 99m");
        var context = BuildContext(counts) with { RoundWind = "S" };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("圈风刻 2番", result.FanNames);
    }

    // === 1番 ===

    [Fact]
    public void YiBanGao_IsRecognized()
    {
        var counts = BuildCounts("123m 123m 456p 789s 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("一般高 1番", result.FanNames);
    }

    [Fact]
    public void XiXiangFeng_IsRecognized()
    {
        var counts = BuildCounts("123m 123p 789s 111m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("喜相逢 1番", result.FanNames);
    }

    [Fact]
    public void LianLiu_IsRecognized()
    {
        var counts = BuildCounts("123456m 789p 111s 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("连六 1番", result.FanNames);
    }

    [Fact]
    public void LaoShaoFu_IsRecognized()
    {
        var counts = BuildCounts("123m 789m 456p 111s 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("老少副 1番", result.FanNames);
    }

    [Fact]
    public void YaoJiuKe_IsRecognized()
    {
        var counts = BuildCounts("111m 234p 567s 789m 99p");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("幺九刻 1番", result.FanNames);
    }

    [Fact]
    public void MingGang_IsRecognized()
    {
        var counts = BuildCounts("111m 222m 333p 456s 55p");
        var context = BuildContext(counts) with { MingKongCount = 1 };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("明杠 1番", result.FanNames);
    }

    [Fact]
    public void QueYiMen_And_WuZi_AreRecognized()
    {
        var counts = BuildCounts("123m 456m 789m 111s 99s");
        var result = FanEvaluator.Evaluate(BuildContext(counts));

        Assert.Contains("无字 1番", result.FanNames);
        Assert.Contains("缺一门 1番", result.FanNames);
    }

    [Fact]
    public void BianZhang_IsRecognized()
    {
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var context = BuildContext(counts) with { WinningTile = "3m" };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("边张 1番", result.FanNames);
    }

    [Fact]
    public void KanZhang_IsRecognized()
    {
        var counts = BuildCounts("456m 123p 789s 111m 99p");
        var context = BuildContext(counts) with { WinningTile = "5m" };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("坎张 1番", result.FanNames);
    }

    [Fact]
    public void DanDiaoJiang_IsRecognized()
    {
        var counts = BuildCounts("123m 456p 789s 111m 55p");
        var context = BuildContext(counts) with { WinningTile = "5p" };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("单钓将 1番", result.FanNames);
    }

    [Fact]
    public void ZiMo_IsRecognized_WithOpenMeld()
    {
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var context = BuildContext(counts) with { IsSelfDraw = true, ChiCount = 1 };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("自摸 1番", result.FanNames);
        Assert.DoesNotContain("不求人 4番", result.FanNames);
    }

    [Fact]
    public void HuaPai_IsRecognized()
    {
        var counts = BuildCounts("123m 456p 789s 111m 99p");
        var context = BuildContext(counts) with { FlowerCount = 4 };
        var result = FanEvaluator.Evaluate(context);

        Assert.Contains("花牌 4番", result.FanNames);
    }
}
