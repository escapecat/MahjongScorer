namespace MahjongScorer.Utilities;

public static class TileConstants
{
    public static readonly IReadOnlyDictionary<string, int> TileIndexMap = new Dictionary<string, int>
    {
        ["1m"] = 0, ["2m"] = 1, ["3m"] = 2, ["4m"] = 3, ["5m"] = 4, ["6m"] = 5, ["7m"] = 6, ["8m"] = 7, ["9m"] = 8,
        ["1p"] = 9, ["2p"] = 10, ["3p"] = 11, ["4p"] = 12, ["5p"] = 13, ["6p"] = 14, ["7p"] = 15, ["8p"] = 16, ["9p"] = 17,
        ["1s"] = 18, ["2s"] = 19, ["3s"] = 20, ["4s"] = 21, ["5s"] = 22, ["6s"] = 23, ["7s"] = 24, ["8s"] = 25, ["9s"] = 26,
        ["E"] = 27, ["S"] = 28, ["W"] = 29, ["N"] = 30,
        ["C"] = 31, ["F"] = 32, ["P"] = 33
    };

    public static readonly string[] TileCodes = TileIndexMap.OrderBy(kv => kv.Value).Select(kv => kv.Key).ToArray();

    public static readonly int[][] SuitPermutations =
    [
        [0, 1, 2],
        [0, 2, 1],
        [1, 0, 2],
        [1, 2, 0],
        [2, 0, 1],
        [2, 1, 0]
    ];
}
