namespace MahjongScorer.Utilities;

/// <summary>
/// Analyzes wait types (edge, closed, single) for winning hands.
/// Per national standard Mahjong rules, 边张/坎张/单钓将 only apply when the
/// winning tile can ONLY be interpreted that way across ALL valid decompositions.
/// We therefore collect the wait-type flags from every decomposition path and
/// return their intersection (AND), not their union (OR).
/// </summary>
internal static class WaitAnalyzer
{
    [Flags]
    public enum WaitType
    {
        None = 0,
        Single = 1,
        Edge = 2,
        Closed = 4
    }

    public static WaitType GetWaitTypes(int[] counts, int winningIndex)
    {
        var result = WaitType.None;
        var hasAny = false;

        for (var i = 0; i < counts.Length; i++)
        {
            if (counts[i] < 2)
            {
                continue;
            }

            counts[i] -= 2;
            var usage = i == winningIndex ? WaitType.Single : WaitType.None;
            var branch = SearchWaitTypes(counts, winningIndex, usage);
            if (branch is not null)
            {
                Intersect(ref result, ref hasAny, branch.Value);
            }
            counts[i] += 2;
        }

        return result;
    }

    /// <summary>
    /// Recursively decomposes the remaining tiles into melds, tracking which
    /// wait-type flags the winning tile contributes to. Returns null when no
    /// valid decomposition exists (dead end), or the AND (intersection) of all
    /// reachable complete-decomposition results.
    /// </summary>
    private static WaitType? SearchWaitTypes(int[] counts, int winningIndex, WaitType usage)
    {
        var index = Array.FindIndex(counts, count => count > 0);
        if (index == -1)
        {
            return usage;
        }

        WaitType result = WaitType.None;
        var hasAny = false;

        if (counts[index] >= 3)
        {
            counts[index] -= 3;
            var branch = SearchWaitTypes(counts, winningIndex, usage);
            if (branch is not null)
            {
                Intersect(ref result, ref hasAny, branch.Value);
            }
            counts[index] += 3;
        }

        if (index < 27 && index % 9 <= 6
            && counts[index + 1] > 0 && counts[index + 2] > 0)
        {
            counts[index]--;
            counts[index + 1]--;
            counts[index + 2]--;

            var nextUsage = usage;
            if (winningIndex == index + 1)
            {
                nextUsage |= WaitType.Closed;
            }
            else if ((winningIndex == index + 2 && index % 9 == 0)
                || (winningIndex == index && index % 9 == 6))
            {
                nextUsage |= WaitType.Edge;
            }

            var branch = SearchWaitTypes(counts, winningIndex, nextUsage);
            if (branch is not null)
            {
                Intersect(ref result, ref hasAny, branch.Value);
            }

            counts[index]++;
            counts[index + 1]++;
            counts[index + 2]++;
        }

        return hasAny ? result : null;
    }

    private static void Intersect(ref WaitType accumulated, ref bool hasAny, WaitType branch)
    {
        if (!hasAny)
        {
            accumulated = branch;
            hasAny = true;
        }
        else
        {
            accumulated &= branch;
        }
    }
}
