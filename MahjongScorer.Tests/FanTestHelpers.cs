using MahjongScorer.Utilities;

namespace MahjongScorer.Tests;

/// <summary>
/// Shared helpers for tile-count building and context creation.
/// </summary>
public partial class FanTests
{
    private static readonly IReadOnlyDictionary<string, int> TileIndexMap = TileConstants.TileIndexMap;

    private static FanEvaluationContext BuildContext(int[] counts)
        => new(counts, false, 0, "E", "E", 0, 0, 0, 0);

    private static int[] BuildCounts(string tiles)
    {
        var counts = new int[34];
        var tokens = tiles.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var token in tokens)
        {
            TryExpandToken(token, counts);
        }

        return counts;
    }

    private static bool TryExpandToken(string token, int[] counts)
    {
        if (token.Length >= 2)
        {
            var last = token[^1];
            if (last is 'm' or 'p' or 's')
            {
                var digits = token[..^1];
                foreach (var digit in digits)
                {
                    if (char.IsDigit(digit))
                    {
                        AddTile(counts, $"{digit}{last}");
                    }
                }

                return true;
            }
        }

        if (token.Length > 1 && token.All(ch => ch == token[0]) && TileIndexMap.ContainsKey(token[0].ToString()))
        {
            for (var i = 0; i < token.Length; i++)
            {
                AddTile(counts, token[0].ToString());
            }

            return true;
        }

        if (TileIndexMap.ContainsKey(token))
        {
            AddTile(counts, token);
            return true;
        }

        return false;
    }

    private static void AddTile(int[] counts, string code)
    {
        if (TileIndexMap.TryGetValue(code, out var index))
        {
            counts[index] += 1;
        }
    }
}
