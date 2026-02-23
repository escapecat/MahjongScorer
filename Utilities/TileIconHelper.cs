using System.Linq;

namespace MahjongScorer.Utilities;

public static class TileIconHelper
{
    private static readonly Dictionary<string, string> TileMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["E"] = "tiles/F1.png",
        ["S"] = "tiles/F2.png",
        ["W"] = "tiles/F3.png",
        ["N"] = "tiles/F4.png",
        ["C"] = "tiles/J1.png",
        ["F"] = "tiles/J2.png",
        ["P"] = "tiles/J3.png",

        ["1m"] = "tiles/W1.png",
        ["2m"] = "tiles/W2.png",
        ["3m"] = "tiles/W3.png",
        ["4m"] = "tiles/W4.png",
        ["5m"] = "tiles/W5.png",
        ["6m"] = "tiles/W6.png",
        ["7m"] = "tiles/W7.png",
        ["8m"] = "tiles/W8.png",
        ["9m"] = "tiles/W9.png",

        ["1s"] = "tiles/T1.png",
        ["2s"] = "tiles/T2.png",
        ["3s"] = "tiles/T3.png",
        ["4s"] = "tiles/T4.png",
        ["5s"] = "tiles/T5.png",
        ["6s"] = "tiles/T6.png",
        ["7s"] = "tiles/T7.png",
        ["8s"] = "tiles/T8.png",
        ["9s"] = "tiles/T9.png",

        ["1p"] = "tiles/B1.png",
        ["2p"] = "tiles/B2.png",
        ["3p"] = "tiles/B3.png",
        ["4p"] = "tiles/B4.png",
        ["5p"] = "tiles/B5.png",
        ["6p"] = "tiles/B6.png",
        ["7p"] = "tiles/B7.png",
        ["8p"] = "tiles/B8.png",
        ["9p"] = "tiles/B9.png",
    };

    public static string? GetImagePath(string code)
    {
        return TileMap.TryGetValue(code, out var path) ? path : null;
    }

    public static IReadOnlyList<string> GetIcons(string? tiles)
    {
        if (string.IsNullOrWhiteSpace(tiles))
        {
            return Array.Empty<string>();
        }

        var result = new List<string>();
        var tokens = tiles.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var token in tokens)
        {
            if (TryExpandToken(token, result))
            {
                continue;
            }
        }

        return result;
    }

    private static bool TryExpandToken(string token, List<string> result)
    {
        if (token.Length >= 2)
        {
            var last = token[^1];
            if (last is 'm' or 'p' or 's')
            {
                var digits = token[..^1];
                foreach (var digit in digits)
                {
                    if (!char.IsDigit(digit))
                    {
                        continue;
                    }

                    var key = $"{digit}{last}";
                    if (TileMap.TryGetValue(key, out var path))
                    {
                        result.Add(path);
                    }
                }

                return true;
            }
        }

        if (token.Length > 1 && token.All(ch => ch == token[0]) && TileMap.ContainsKey(token[0].ToString()))
        {
            if (TileMap.TryGetValue(token[0].ToString(), out var honorPath))
            {
                for (var i = 0; i < token.Length; i++)
                {
                    result.Add(honorPath);
                }
            }

            return true;
        }

        if (TileMap.TryGetValue(token, out var single))
        {
            result.Add(single);
            return true;
        }

        return false;
    }
}
