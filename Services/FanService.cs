using MahjongScorer.Models;

namespace MahjongScorer.Services;

public class FanService
{
    private readonly List<Fan> _allFans;

    public FanService()
    {
        _allFans = InitializeFans();
    }

    public IReadOnlyList<Fan> GetAllFans() => _allFans.AsReadOnly();

    public IReadOnlyList<Fan> GetFansByCategory(FanCategory category)
        => _allFans.Where(f => f.Category == category).ToList().AsReadOnly();

    public int CalculateTotal(IEnumerable<int> selectedFanIds)
    {
        return _allFans
            .Where(f => selectedFanIds.Contains(f.Id))
            .Sum(f => f.Points);
    }

    public static string GetCategoryDisplayName(FanCategory category) => category switch
    {
        FanCategory.Points88 => "88番",
        FanCategory.Points64 => "64番",
        FanCategory.Points48 => "48番",
        FanCategory.Points32 => "32番",
        FanCategory.Points24 => "24番",
        FanCategory.Points16 => "16番",
        FanCategory.Points12 => "12番",
        FanCategory.Points8 => "8番",
        FanCategory.Points6 => "6番",
        FanCategory.Points4 => "4番",
        FanCategory.Points2 => "2番",
        FanCategory.Points1 => "1番",
        _ => category.ToString()
    };

    private static List<Fan> InitializeFans()
    {
        var fans = new List<Fan>();
        int id = 1;

        void AddFan(string name, int points, FanCategory category, string description, string exampleText, string exampleTiles, string? icon = null)
        {
            var normalizedIcon = string.IsNullOrWhiteSpace(icon) || icon.Any(char.IsControl)
                ? "\U0001F004"
                : icon;

            fans.Add(new Fan
            {
                Id = id++,
                Name = name,
                Points = points,
                Category = category,
                Description = description,
                Example = exampleText,
                ExampleTiles = exampleTiles,
                Icon = normalizedIcon
            });
        }

        // 88番
        AddFan("大四喜", 88, FanCategory.Points88, "由4副风牌的刻子(杠)组成的和牌。不计圈风刻、门风刻、三风刻、幺九刻、碰碰和", "例：东东东 南南南 西西西 北北北 中中", "EEE SSS WWW NNN CC", "🀀");
        AddFan("大三元", 88, FanCategory.Points88, "和牌中，有中发白3副刻子。不计双箭刻、箭刻", "例：中中中 发发发 白白白 123万 99筒", "CCC FFF PPP 123m 99p", "🀄");
        AddFan("绿一色", 88, FanCategory.Points88, "由23468条及发字中的任何牌组成的和牌", "例：222333444666条 发发", "222333444666s FF", "🀅");
        AddFan("九莲宝灯", 88, FanCategory.Points88, "由一种花色序数牌按1112345678999组成的门清九面听。不计清一色、幺九刻、门前清；自摸记不求人", "例：1112345678999万 + 任意万", "1112345678999m 5m", "🀇");
        AddFan("四杠", 88, FanCategory.Points88, "4个杠（暗杠加计）。不计单钓将、碰碰和", "例：四个杠子成和", "1111m 2222m 3333p 4444s 55p", "🀫" );
        AddFan("连七对", 88, FanCategory.Points88, "由一种花色序数牌组成序数相连的7个对子的和牌。不计清一色、七对、单钓将、门前清；自摸记不求人", "例：11223344556677万", "11223344556677m", "🀙");
        AddFan("十三幺", 88, FanCategory.Points88, "由3种序数牌一九牌、7种字牌及其中一对作将。不计五门齐、单钓将、门前清、混幺九；自摸记不求人", "例：19万19筒19条 东南西北中发白 + 任意一对", "1m9m 1p9p 1s9s E S W N C F P 1m", "🀄");

        // 64番
        AddFan("清幺九", 64, FanCategory.Points64, "由序数牌一、九刻子组成的和牌。不计碰碰和、全带幺、幺九刻、无字（可加双同刻、三同刻）", "例：111999万 111999筒 99条", "111999m 111999p 99s", "🀙");
        AddFan("小四喜", 64, FanCategory.Points64, "和牌时有风牌的3副刻子及将牌。不计三风刻、幺九刻", "例：东东东 南南南 西西西 北北 11万", "EEE SSS WWW NN 11m", "🀀");
        AddFan("小三元", 64, FanCategory.Points64, "和牌时有箭牌的两副刻子及将牌。不计双箭刻、箭刻", "例：中中中 发发发 白白 123万", "CCC FFF PP 123m", "🀄");
        AddFan("字一色", 64, FanCategory.Points64, "由字牌的刻子(杠)、将组成的和牌。不计碰碰和、混幺九、全带幺、幺九刻、缺一门", "例：东东东 南南南 西西西 北北北 中中", "EEE SSS WWW NNN CC", "🀀");
        AddFan("四暗刻", 64, FanCategory.Points64, "包含4个暗刻的和牌。不计门前清、碰碰和、三暗刻、双暗刻、不求人", "例：暗刻四组 + 将牌", "111m 222m 333p 444s 55p", "🀫" );
        AddFan("一色双龙会", 64, FanCategory.Points64, "由一种花色的两个老少副，5作将的和牌。不计平和、七对、清一色、一般高、老少副、缺一门、无字", "例：123789万 123789万 55万", "123789m 123789m 55m", "🀇");

        // 48番
        AddFan("一色四同顺", 48, FanCategory.Points48, "一种花色4副序数相同的顺子。不计一色三节高、一般高、四归一、一色三同顺、缺一门", "例：123万123万123万123万 99筒", "123m 123m 123m 123m 99p", "🀇");
        AddFan("一色四节高", 48, FanCategory.Points48, "一种花色4副依次递增一位数字的刻子。不计一色三同顺、一色三节高、碰碰和、缺一门", "例：111万222万333万444万 55筒", "111m 222m 333m 444m 55p", "🀇");

        // 32番
        AddFan("一色四步高", 32, FanCategory.Points32, "一种花色4副依次递增一位或二位数字的顺子。不计一色三步高、缺一门", "例：123万234万345万456万 99筒", "123m 234m 345m 456m 99p", "🀇");
        AddFan("三杠", 32, FanCategory.Points32, "3个杠。不计双暗杠、双明杠、暗杠、明杠、明暗杠", "例：三个杠子成和", "1111m 2222m 3333p 55s", "🀫" );
        AddFan("混幺九", 32, FanCategory.Points32, "由字牌和序数牌一、九的刻子及将牌组成的和牌。不计碰碰和、幺九刻、全带幺", "例：111万999筒 东东东 中中", "111m 999p EEE CC", "🀙");

        // 24番
        AddFan("七对", 24, FanCategory.Points24, "由7个对子组成的和牌。不计门前清、不求人、单钓将", "例：11223344556677万", "11223344556677m", "🀙");
        AddFan("七星不靠", 24, FanCategory.Points24, "由7张不同字牌和三组147/258/369中任意7张序数牌组成。不计五门齐、不求人、单钓将、门前清、全不靠", "例：147万258筒369条 东南西北中发白", "147m 258p 369s E S W N C F P", "🀖");
        AddFan("全双刻", 24, FanCategory.Points24, "由2、4、6、8序数牌的刻子、将牌组成的和牌。不计碰碰和、断幺、无字", "例：222444666888万 22筒", "222m 444m 666m 888m 22p", "🀇");
        AddFan("清一色", 24, FanCategory.Points24, "由一种花色的序数牌组成的和牌。不计缺一门、无字", "例：123456789万 111万 99万", "123456789m 111m 99m", "🀇");
        AddFan("一色三同顺", 24, FanCategory.Points24, "一种花色3副序数相同的顺子。不计一色三节高、一般高", "例：123万123万123万 456万 99万", "123m 123m 123m 456m 99m", "🀇");
        AddFan("一色三节高", 24, FanCategory.Points24, "一种花色3副依次递增一位数字的刻子。不计一色三同顺", "例：111万222万333万 456万 99万", "111m 222m 333m 456m 99m", "🀇");
        AddFan("全大", 24, FanCategory.Points24, "由序数牌789组成的和牌。不计无字、大于五", "例：789万789筒789条 777万 99筒", "789m 789p 789s 777m 99p", "🀘");
        AddFan("全中", 24, FanCategory.Points24, "由序数牌456组成的和牌。不计断幺、无字", "例：456万456筒456条 444万 66筒", "456m 456p 456s 444m 66p", "🀔");
        AddFan("全小", 24, FanCategory.Points24, "由序数牌123组成的和牌。不计无字、小于五", "例：123万123筒123条 111万 22筒", "123m 123p 123s 111m 22p", "🀇");

        // 16番
        AddFan("清龙", 16, FanCategory.Points16, "一种花色的123、456、789三组顺子", "例：123万456万789万 111筒 99条", "123m 456m 789m 111p 99s", "🀇");
        AddFan("三色双龙会", 16, FanCategory.Points16, "2种花色2个老少副、另一种花色5作将的和牌。不计喜相逢、老少副、无字、平和", "例：123789万 123789筒 55条", "123789m 123789p 55s", "🀇");
        AddFan("一色三步高", 16, FanCategory.Points16, "一种花色3副依次递增一位或二位数字的顺子", "例：123万234万345万 678万 99筒", "123m 234m 345m 678m 99p", "🀇");
        AddFan("全带五", 16, FanCategory.Points16, "每副牌及将牌必须有5的序数牌。不计断幺、无字", "例：345万 456万 567筒 555条 55万", "345m 456m 567p 555s 55m", "🀔");
        AddFan("三同刻", 16, FanCategory.Points16, "3个序数相同的刻子(杠)。不计双同刻", "例：111万111筒111条 123万 99筒", "111m 111p 111s 123m 99p", "🀇");
        AddFan("三暗刻", 16, FanCategory.Points16, "3个暗刻。不计双暗刻", "例：暗刻三组 + 将牌", "111m 222p 333s 456m 99p", "🀫" );

        // 12番
        AddFan("全不靠", 12, FanCategory.Points12, "由7张不同字牌与三组147/258/369任意14张组成。不计五门齐、不求人、单钓将、门前清", "例：147万258筒369条 東南西北中發白", "147m 258p 369s E S W N C F P", "🀖");
        AddFan("组合龙", 12, FanCategory.Points12, "包含147、258、369三组组成的和牌", "例：147万258筒369条 另加一组面子+将", "147m 258p 369s 111m 99p", "🀖");
        AddFan("大于五", 12, FanCategory.Points12, "由序数牌6789组成的和牌。不计无字", "例：6789万678筒789条 88万", "6789m 678p 789s 88m", "🀘");
        AddFan("小于五", 12, FanCategory.Points12, "由序数牌1234组成的和牌。不计无字", "例：1234万123筒234条 11万", "1234m 123p 234s 11m", "🀇");
        AddFan("三风刻", 12, FanCategory.Points12, "3个风刻。不计缺一门", "例：东东东 南南南 西西西 123万 99筒", "EEE SSS WWW 123m 99p", "🀀");

        // 8番
        AddFan("花龙", 8, FanCategory.Points8, "包含三色123/456/789组成1-9序数牌的和牌", "例：123万456筒789条 111万 99筒", "123m 456p 789s 111m 99p", "🀇");
        AddFan("推不倒", 8, FanCategory.Points8, "由1234589筒、245689条、白板组成的和牌。不计缺一门", "例：1234589筒 245689条 白白", "1234589p 245689s PP", "🀝");
        AddFan("三色三同顺", 8, FanCategory.Points8, "3种花色3副序数相同的顺子。不计喜相逢", "例：123万123万123条 456万 99筒", "123m 123p 123s 456m 99p", "🀇");
        AddFan("三色三节高", 8, FanCategory.Points8, "3种花色3副依次递增一位数字的刻子", "例：111万222筒333条 456万 99筒", "111m 222p 333s 456m 99p", "🀇");
        AddFan("无番和", 8, FanCategory.Points8, "和牌后数不出任何番种分（不计其它所有番型）", "例：无任何其他番的和牌", "123m 456p 789s 111m 99p", "🀇");
        AddFan("妙手回春", 8, FanCategory.Points8, "自摸牌墙上最后一张牌和牌。不计自摸", "例：自摸牌墙最后一张成和", "123m 456p 789s 111m 99p", "🀇");
        AddFan("海底捞月", 8, FanCategory.Points8, "和打出的最后一张牌", "例：和出最后一张打出的牌", "123m 456p 789s 111m 99p", "🀇");
        AddFan("杠上开花", 8, FanCategory.Points8, "开杠补张成和，不计自摸", "例：开杠后补张成和", "1111m 123p 456s 789m 99p", "🀫" );
        AddFan("抢杠和", 8, FanCategory.Points8, "和别人开明杠的牌。不计和绝张", "例：抢别人加杠的牌成和", "1111m 123p 456s 789m 99p", "🀫" );

        // 6番
        AddFan("碰碰和", 6, FanCategory.Points6, "由4副刻子(或杠)、将牌组成的和牌", "例：111万222万333筒444条 99万", "111m 222m 333p 444s 99m", "🀇");
        AddFan("混一色", 6, FanCategory.Points6, "由一种花色序数牌及字牌组成的和牌。不计缺一门", "例：123万456万789万 东东 99万", "123m 456m 789m EE 99m", "🀇");
        AddFan("三色三步高", 6, FanCategory.Points6, "3种花色3副依次递增一位数字的顺子", "例：123万234筒345条 456万 99筒", "123m 234p 345s 456m 99p", "🀇");
        AddFan("五门齐", 6, FanCategory.Points6, "和牌时3种序数牌、风、箭牌齐全", "例：万筒条风箭齐全成和", "123m 456p 789s E C 99m", "🀇");
        AddFan("全求人", 6, FanCategory.Points6, "全靠吃牌、碰牌、单钓别人打出的牌和牌。不计单钓将", "例：全副吃碰后单钓成和", "111m 222p 333s 456m 99p", "🀇");
        AddFan("双箭刻", 6, FanCategory.Points6, "2副箭刻。不计箭刻", "例：中中中 发发发 123万 99筒", "CCC FFF 123m 99p", "🀄");
        AddFan("双暗杠", 6, FanCategory.Points6, "2个暗杠。不计双暗刻、暗杠", "例：两个暗杠成和", "1111m 2222p 123s 456m 99p", "🀫" );

        // 4番
        AddFan("全带幺", 4, FanCategory.Points4, "和牌时每副牌、将牌均含幺牌（1/9/字）", "例：123万 789万 111筒 999条 11万", "123m 789m 111p 999s 11m", "🀇");
        AddFan("不求人", 4, FanCategory.Points4, "没有吃牌、碰牌（包括明杠），自摸和牌。不计门前清、自摸", "例：门前清自摸", "123m 456p 789s 111m 99p", "🀇");
        AddFan("双明杠", 4, FanCategory.Points4, "2个明杠。不计明杠", "例：两个明杠成和", "1111m 2222p 123s 456m 99p", "🀫" );
        AddFan("和绝张", 4, FanCategory.Points4, "和牌池、桌面已亮明的第4张牌", "例：和已见三张的第四张", "111m 222p 333s 444m 99p", "🀇");

        // 2番
        AddFan("箭刻", 2, FanCategory.Points2, "中、发、白的刻子(或杠)", "例：中中中 123万 456筒 789条 99万", "CCC 123m 456p 789s 99m", "🀄");
        AddFan("圈风刻", 2, FanCategory.Points2, "与圈风相同的风刻。计圈风刻的那副刻子不再计幺九刻", "例：圈风为东，东东东成刻", "EEE 123m 456p 789s 99m", "🀀");
        AddFan("门风刻", 2, FanCategory.Points2, "与本门风相同的风刻。计门风刻的那副刻子不再计幺九刻", "例：门风为南，南南南成刻", "SSS 123m 456p 789s 99m", "🀁");
        AddFan("门前清", 2, FanCategory.Points2, "没有吃、碰、明杠，和别人打出的牌", "例：门前清荣和", "123m 456p 789s 111m 99p", "🀇");
        AddFan("平和", 2, FanCategory.Points2, "由4副顺子及序数牌作将组成的和牌。不计无字", "例：四副顺子+将", "123m 456m 789m 234p 55s", "🀇");
        AddFan("四归一", 2, FanCategory.Points2, "和牌中，有4张相同的牌归于一家的顺、刻子、对、将牌中（杠不计）", "例：同一牌4张分散成组", "1111m 123p 456s 789m 99p", "🀇");
        AddFan("双同刻", 2, FanCategory.Points2, "2副序数相同的刻子", "例：111万111筒 123万 456筒 99条", "111m 111p 123m 456p 99s", "🀇");
        AddFan("双暗刻", 2, FanCategory.Points2, "2个暗刻", "例：暗刻两组成和", "111m 222p 456s 789m 99p", "🀫" );
        AddFan("暗杠", 2, FanCategory.Points2, "自抓4张相同的牌开杠", "例：自抓四张成暗杠", "1111m 123p 456s 789m 99p", "🀫" );
        AddFan("断幺", 2, FanCategory.Points2, "和牌中没有一、九及字牌。不计无字", "例：手牌无1/9及字牌", "234m 345p 456s 678m 55p", "🀇");

        // 1番
        AddFan("一般高", 1, FanCategory.Points1, "一种花色2副序数相同的顺子", "例：123万123万 456筒 789条 99万", "123m 123m 456p 789s 99m", "🀇");
        AddFan("喜相逢", 1, FanCategory.Points1, "2种花色2副序数相同的顺子", "例：123万123筒 456万 789条 99万", "123m 123p 456m 789s 99m", "🀇");
        AddFan("连六", 1, FanCategory.Points1, "一种花色6张相连接的序数牌", "例：123456万 789筒 111条 99万", "123456m 789p 111s 99m", "🀇");
        AddFan("老少副", 1, FanCategory.Points1, "一种花色牌的123、789两组顺子", "例：123万789万 456筒 111条 99万", "123m 789m 456p 111s 99m", "🀇");
        AddFan("幺九刻", 1, FanCategory.Points1, "序数牌一、九的刻子（或杠），字牌的刻子（或杠）", "例：111万 或 999筒 或 东东东", "111m 999p EEE", "🀙");
        AddFan("明杠", 1, FanCategory.Points1, "有暗刻，碰别人打出的那张牌开杠或抓进一张与已碰明刻相同的牌开杠", "例：碰后加杠或抓杠", "1111m 123p 456s 789m 99p", "🀫" );
        AddFan("缺一门", 1, FanCategory.Points1, "和牌中缺少一种花色序数牌", "例：缺筒子成和", "123m 456m 789m 111s 99s", "🀇");
        AddFan("无字", 1, FanCategory.Points1, "和牌中没有字牌", "例：全是数牌成和", "123m 456p 789s 111m 99p", "🀇");
        AddFan("边张", 1, FanCategory.Points1, "单和123的3及789的7或1233和3、7789和7等", "例：和123的3或789的7", "123m 456p 789s 111m 99p", "🀇");
        AddFan("坎张", 1, FanCategory.Points1, "和2张牌之间的那张牌（4556和5亦为坎张）", "例：和456中的5", "456m 123p 789s 111m 99p", "🀋");
        AddFan("单钓将", 1, FanCategory.Points1, "单钓一张将牌", "例：单钓将牌成和", "123m 456p 789s 111m 55p", "🀇");
        AddFan("自摸", 1, FanCategory.Points1, "自己抓进牌成和牌", "例：自摸成和", "123m 456p 789s 111m 99p", "🀇");
        AddFan("花牌", 1, FanCategory.Points1, "春夏秋冬，梅兰竹菊，每花计一分。杠上开花不计自摸，起和分后计", "例：有春夏秋冬梅兰竹菊", "F1 F2 F3 F4 F5 F6 F7 F8", "🀈" );

        return fans;
    }
}
