namespace MahjongScorer.Utilities;

public static class MahjongHandEvaluator
{
    public static bool IsWinningHand(int[] counts)
    {
        if (counts.Sum() == 0)
        {
            return false;
        }

        if (IsSevenPairs(counts))
        {
            return true;
        }

        // Special hand forms (no standard decomposition)
        if (HandPatternDetector.IsThirteenOrphans(counts))
        {
            return true;
        }

        if (HandPatternDetector.IsSevenStarNotConnected(counts))
        {
            return true;
        }

        if (HandPatternDetector.IsAllNotConnected(counts))
        {
            return true;
        }

        for (var i = 0; i < counts.Length; i++)
        {
            if (counts[i] < 2)
            {
                continue;
            }

            counts[i] -= 2;
            if (CanFormSets(counts))
            {
                counts[i] += 2;
                return true;
            }
            counts[i] += 2;
        }

        return false;
    }

    /// <summary>
    /// Checks if the hand is a winning hand when some tiles are already locked in melds.
    /// Only the remaining (non-meld) tiles need to form valid sets + a pair.
    /// </summary>
    public static bool IsWinningHandWithMelds(int[] fullCounts, IReadOnlyList<IReadOnlyList<string>>? melds)
    {
        if (melds is null || melds.Count == 0)
        {
            return IsWinningHand(fullCounts);
        }

        var handCounts = fullCounts.ToArray();
        foreach (var meld in melds)
        {
            foreach (var tile in meld)
            {
                if (TileConstants.TileIndexMap.TryGetValue(tile, out var index))
                {
                    handCounts[index] = Math.Max(0, handCounts[index] - 1);
                }
            }
        }

        // Remaining hand tiles must form (4 - meldCount) sets + 1 pair
        var remaining = handCounts.Sum();
        if (remaining < 2)
        {
            return false;
        }

        // Special case: all melds accounted for, only pair remains
        if (remaining == 2)
        {
            return Array.Exists(handCounts, c => c == 2);
        }

        // Try each possible pair, then check remaining can form sets
        for (var i = 0; i < handCounts.Length; i++)
        {
            if (handCounts[i] < 2)
            {
                continue;
            }

            handCounts[i] -= 2;
            if (CanFormSets(handCounts))
            {
                handCounts[i] += 2;
                return true;
            }
            handCounts[i] += 2;
        }

        return false;
    }

    public static bool IsSevenPairs(int[] counts)
    {
        var total = counts.Sum();
        if (total % 2 != 0)
        {
            return false;
        }

        var pairs = 0;
        foreach (var count in counts)
        {
            pairs += count / 2;
        }

        return pairs * 2 == total;
    }

    private static bool CanFormSets(int[] counts)
    {
        var index = Array.FindIndex(counts, c => c > 0);
        if (index == -1)
        {
            return true;
        }

        if (counts[index] >= 3)
        {
            counts[index] -= 3;
            if (CanFormSets(counts))
            {
                counts[index] += 3;
                return true;
            }
            counts[index] += 3;
        }

        if (counts[index] >= 4)
        {
            counts[index] -= 4;
            if (CanFormSets(counts))
            {
                counts[index] += 4;
                return true;
            }
            counts[index] += 4;
        }

        if (IsSuitTile(index))
        {
            var next1 = index + 1;
            var next2 = index + 2;
            if (next2 < counts.Length && IsSameSuit(index, next2) && counts[next1] > 0 && counts[next2] > 0)
            {
                counts[index]--;
                counts[next1]--;
                counts[next2]--;
                if (CanFormSets(counts))
                {
                    counts[index]++;
                    counts[next1]++;
                    counts[next2]++;
                    return true;
                }
                counts[index]++;
                counts[next1]++;
                counts[next2]++;
            }
        }

        return false;
    }

    private static bool IsSuitTile(int index) => index < 27;

    private static bool IsSameSuit(int a, int b)
    {
        if (!IsSuitTile(a) || !IsSuitTile(b))
        {
            return false;
        }

        return a / 9 == b / 9;
    }
}
