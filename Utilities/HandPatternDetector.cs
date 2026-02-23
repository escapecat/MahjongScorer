namespace MahjongScorer.Utilities;

/// <summary>
/// Detects hand composition patterns (e.g. all honors, all terminals, pure suit, green tiles, etc.)
/// </summary>
internal static class HandPatternDetector
{
    private static readonly HashSet<int> AllGreenIndices = [19, 20, 21, 23, 25, 32];
    private static readonly HashSet<int> PushedDownPinRanks = [0, 1, 2, 3, 4, 7, 8];
    private static readonly HashSet<int> PushedDownSouRanks = [1, 3, 4, 5, 7, 8];
    private static readonly HashSet<int> UnrelatedGroup1 = [0, 3, 6];
    private static readonly HashSet<int> UnrelatedGroup2 = [1, 4, 7];
    private static readonly HashSet<int> UnrelatedGroup3 = [2, 5, 8];
    private static readonly HashSet<int> ThirteenOrphanIndices = [0, 8, 9, 17, 18, 26, 27, 28, 29, 30, 31, 32, 33];
    private static readonly int[] NineGatesRequired = [3, 1, 1, 1, 1, 1, 1, 1, 3];

    public static bool IsAllHonors(int[] counts)
        => counts[..27].All(count => count == 0) && counts[27..].Any(count => count > 0);

    public static bool IsAllTerminals(int[] counts)
        => counts[27..].All(count => count == 0) && IsAllTerminalRanks(counts);

    public static bool IsAllTerminalsOrHonors(int[] counts)
        => counts[27..].Any(count => count > 0) && IsAllTerminalRanks(counts);

    public static bool IsNoHonors(int[] counts)
        => counts[27..].All(count => count == 0);

    public static bool IsMissingOneSuit(int[] counts)
    {
        var suits = CountSuitsPresent(counts);
        return suits is > 0 and < 3;
    }

    public static bool IsAllBig(int[] counts)
        => IsNoHonors(counts) && IsOnlyRanks(counts, rank => rank >= 6);

    public static bool IsAllMiddle(int[] counts)
        => IsNoHonors(counts) && IsOnlyRanks(counts, rank => rank is >= 3 and <= 5);

    public static bool IsAllSmall(int[] counts)
        => IsNoHonors(counts) && IsOnlyRanks(counts, rank => rank <= 2);

    public static bool IsGreaterThanFive(int[] counts)
        => IsNoHonors(counts) && IsOnlyRanks(counts, rank => rank >= 5);

    public static bool IsLessThanFive(int[] counts)
        => IsNoHonors(counts) && IsOnlyRanks(counts, rank => rank <= 3);

    public static bool IsAllEvenTriplets(int[] counts)
    {
        if (!IsNoHonors(counts))
        {
            return false;
        }

        for (var i = 0; i < 27; i++)
        {
            if (counts[i] == 0)
            {
                continue;
            }

            var rank = i % 9;
            if (rank % 2 == 0)
            {
                return false;
            }
        }

        return counts[..27].Any(count => count > 0);
    }

    public static bool IsAllSimples(int[] counts)
    {
        for (var i = 0; i < counts.Length; i++)
        {
            if (counts[i] == 0)
            {
                continue;
            }

            if (i >= 27)
            {
                return false;
            }

            var rank = i % 9;
            if (rank is 0 or 8)
            {
                return false;
            }
        }

        return counts.Any(count => count > 0);
    }

    public static bool IsPureSuit(int[] counts)
    {
        if (!IsNoHonors(counts))
        {
            return false;
        }

        return CountSuitsPresent(counts) == 1;
    }

    public static bool IsHalfFlush(int[] counts)
    {
        if (IsNoHonors(counts))
        {
            return false;
        }

        return CountSuitsPresent(counts) == 1;
    }

    public static bool IsAllGreen(int[] counts)
    {
        var hasTile = false;
        for (var i = 0; i < counts.Length; i++)
        {
            if (counts[i] == 0)
            {
                continue;
            }

            if (!AllGreenIndices.Contains(i))
            {
                return false;
            }

            hasTile = true;
        }

        return hasTile;
    }

    public static bool IsPushedDownHand(int[] counts)
    {
        for (var i = 0; i < counts.Length; i++)
        {
            if (counts[i] == 0)
            {
                continue;
            }

            if (i < 9)
            {
                return false;
            }

            if (i < 18)
            {
                if (!PushedDownPinRanks.Contains(i - 9))
                {
                    return false;
                }

                continue;
            }

            if (i < 27)
            {
                if (!PushedDownSouRanks.Contains(i - 18))
                {
                    return false;
                }

                continue;
            }

            if (i != 33)
            {
                return false;
            }
        }

        return counts.Any(count => count > 0);
    }

    public static bool IsNineGates(int[] counts, bool hasOpenMeld)
    {
        if (hasOpenMeld || counts[27..].Any(count => count > 0))
        {
            return false;
        }

        for (var suit = 0; suit < 3; suit++)
        {
            if (IsNineGatesInSuit(counts, suit))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsSevenShiftedPairs(int[] counts)
    {
        if (!MahjongHandEvaluator.IsSevenPairs(counts) || counts[27..].Any(count => count > 0))
        {
            return false;
        }

        for (var suit = 0; suit < 3; suit++)
        {
            var pairs = new List<int>();
            var baseIndex = suit * 9;
            var suitCounts = counts.Skip(baseIndex).Take(9).ToArray();
            if (suitCounts.All(count => count == 0))
            {
                continue;
            }

            for (var i = 0; i < 9; i++)
            {
                if (suitCounts[i] == 2)
                {
                    pairs.Add(i);
                }
                else if (suitCounts[i] != 0)
                {
                    return false;
                }
            }

            if (pairs.Count == 7 && pairs.Max() - pairs.Min() == 6)
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsThirteenOrphans(int[] counts)
    {
        var total = counts.Sum();
        if (total != 14)
        {
            return false;
        }

        var pairFound = false;
        for (var i = 0; i < counts.Length; i++)
        {
            if (ThirteenOrphanIndices.Contains(i))
            {
                if (counts[i] == 0)
                {
                    return false;
                }

                if (counts[i] >= 2)
                {
                    if (pairFound)
                    {
                        return false;
                    }

                    pairFound = true;
                }

                if (counts[i] > 2)
                {
                    return false;
                }
            }
            else if (counts[i] > 0)
            {
                return false;
            }
        }

        return pairFound;
    }

    public static bool IsSevenStarNotConnected(int[] counts)
        => IsUnrelatedHand(counts, requireAllGroups: true);

    public static bool IsAllNotConnected(int[] counts)
        => IsUnrelatedHand(counts, requireAllGroups: false);

    public static bool HasComposedDragon(int[] counts)
    {
        foreach (var order in TileConstants.SuitPermutations)
        {
            if (HasRanks(counts, order[0], [0, 3, 6])
                && HasRanks(counts, order[1], [1, 4, 7])
                && HasRanks(counts, order[2], [2, 5, 8]))
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasFiveGates(int[] counts)
    {
        var hasMan = counts[..9].Any(count => count > 0);
        var hasPin = counts[9..18].Any(count => count > 0);
        var hasSou = counts[18..27].Any(count => count > 0);
        var hasWind = counts[27..31].Any(count => count > 0);
        var hasDragon = counts[31..].Any(count => count > 0);

        return hasMan && hasPin && hasSou && hasWind && hasDragon;
    }

    public static int GetWindTripletCount(int[] counts)
        => counts[27..31].Count(count => count >= 3);

    public static int CountFourOfAKindNotKong(int[] counts)
    {
        return counts.Count(count => count == 4);
    }

    private static bool IsAllTerminalRanks(int[] counts)
    {
        for (var i = 0; i < 27; i++)
        {
            var rank = i % 9;
            if (rank is > 0 and < 8 && counts[i] > 0)
            {
                return false;
            }
        }

        return counts.Any(count => count > 0);
    }

    private static bool IsOnlyRanks(int[] counts, Func<int, bool> predicate)
    {
        var hasTile = false;
        for (var i = 0; i < 27; i++)
        {
            if (counts[i] == 0)
            {
                continue;
            }

            hasTile = true;
            var rank = i % 9;
            if (!predicate(rank))
            {
                return false;
            }
        }

        return hasTile;
    }

    private static int CountSuitsPresent(int[] counts)
    {
        var suitPresence = new[]
        {
            counts[..9].Any(count => count > 0),
            counts[9..18].Any(count => count > 0),
            counts[18..27].Any(count => count > 0)
        };

        return suitPresence.Count(present => present);
    }

    private static bool IsNineGatesInSuit(int[] counts, int suit)
    {
        var baseIndex = suit * 9;
        var total = 0;
        for (var i = 0; i < 9; i++)
        {
            var count = counts[baseIndex + i];
            if (count < NineGatesRequired[i])
            {
                return false;
            }

            total += count;
        }

        if (total != 14)
        {
            return false;
        }

        for (var i = 0; i < 3; i++)
        {
            if (i == suit)
            {
                continue;
            }

            if (counts.Skip(i * 9).Take(9).Any(count => count > 0))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsUnrelatedHand(int[] counts, bool requireAllGroups)
    {
        if (counts[27..].Any(count => count != 1))
        {
            return false;
        }

        var totalNumbers = counts[..27].Sum();
        if (totalNumbers != 7)
        {
            return false;
        }

        var groupsPresent = new HashSet<int>();
        for (var suit = 0; suit < 3; suit++)
        {
            var baseIndex = suit * 9;
            var ranks = new List<int>();
            for (var i = 0; i < 9; i++)
            {
                if (counts[baseIndex + i] > 1)
                {
                    return false;
                }

                if (counts[baseIndex + i] == 1)
                {
                    ranks.Add(i);
                }
            }

            if (ranks.Count == 0)
            {
                continue;
            }

            var groupIndex = GetUnrelatedGroupIndex(ranks);
            if (groupIndex < 0)
            {
                return false;
            }

            groupsPresent.Add(groupIndex);
        }

        return !requireAllGroups || groupsPresent.Count == 3;
    }

    private static int GetUnrelatedGroupIndex(IReadOnlyCollection<int> ranks)
    {
        if (ranks.All(UnrelatedGroup1.Contains))
            return 0;

        if (ranks.All(UnrelatedGroup2.Contains))
            return 1;

        if (ranks.All(UnrelatedGroup3.Contains))
            return 2;

        return -1;
    }

    private static bool HasRanks(int[] counts, int suit, int[] ranks)
    {
        var baseIndex = suit * 9;
        return ranks.All(rank => counts[baseIndex + rank] > 0);
    }
}
