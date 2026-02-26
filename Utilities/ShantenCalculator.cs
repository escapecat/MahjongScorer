namespace MahjongScorer.Utilities;

/// <summary>
/// Calculates the shanten number (???) for a mahjong hand.
/// -1 = complete (??), 0 = tenpai (??), 1 = iishanten (???), etc.
/// Considers standard form, seven pairs, and thirteen orphans.
/// </summary>
public static class ShantenCalculator
{
    /// <summary>
    /// Calculate the minimum shanten number across all hand forms.
    /// <paramref name="counts"/> should contain only free hand tiles
    /// (meld tiles already subtracted), length 34.
    /// <paramref name="meldCount"/> is the number of locked melds (chi/peng/kong).
    /// </summary>
    public static int Calculate(int[] counts, int meldCount)
    {
        var best = CalculateStandard(counts, meldCount);

        // Seven pairs and thirteen orphans only possible with no open melds
        if (meldCount == 0)
        {
            best = Math.Min(best, CalculateSevenPairs(counts));
            best = Math.Min(best, CalculateThirteenOrphans(counts));
        }

        return best;
    }

    /// <summary>
    /// Standard form shanten: (4 - meldCount) melds + 1 pair from free tiles.
    /// Formula: (4 - meldCount - mentsu) * 2 - taatsu - jantai
    /// where mentsu = completed melds, taatsu = partial melds (pairs/sequences),
    /// jantai = 1 if pair found.
    /// </summary>
    private static int CalculateStandard(int[] counts, int meldCount)
    {
        var work = counts.ToArray();
        var neededMelds = 4 - meldCount;
        var best = 8; // worst case: 8-shanten (4*2)

        SearchStandard(work, 0, neededMelds, 0, 0, false, ref best);

        return best;
    }

    private static void SearchStandard(
        int[] counts, int startIndex,
        int neededMelds, int mentsu, int taatsu, bool hasPair,
        ref int best)
    {
        // Current shanten estimate
        var current = (neededMelds - mentsu) * 2 - taatsu - (hasPair ? 1 : 0);
        best = Math.Min(best, current);

        // Prune: can't improve further
        if (best <= -1) return;
        // The maximum possible improvement from remaining tiles
        var maxAdd = neededMelds - mentsu;
        if (mentsu + taatsu >= neededMelds + 1)
        {
            // Already have enough partial groups; taatsu is capped
        }

        for (var i = startIndex; i < 34; i++)
        {
            if (counts[i] == 0) continue;

            // Try pair (if no pair yet)
            if (!hasPair && counts[i] >= 2)
            {
                counts[i] -= 2;
                SearchStandard(counts, i, neededMelds, mentsu, taatsu, true, ref best);
                counts[i] += 2;
            }

            // Try triplet (complete meld)
            if (counts[i] >= 3)
            {
                counts[i] -= 3;
                SearchStandard(counts, i, neededMelds, mentsu + 1, taatsu, hasPair, ref best);
                counts[i] += 3;
            }

            // Try sequence (complete meld) — suit tiles only
            if (i < 27 && i % 9 <= 6 && counts[i + 1] > 0 && counts[i + 2] > 0)
            {
                counts[i]--;
                counts[i + 1]--;
                counts[i + 2]--;
                SearchStandard(counts, i, neededMelds, mentsu + 1, taatsu, hasPair, ref best);
                counts[i]++;
                counts[i + 1]++;
                counts[i + 2]++;
            }

            // Try partial groups (taatsu) — only if mentsu + taatsu < neededMelds + 1
            // (having more partial groups than needed doesn't help)
            if (mentsu + taatsu < neededMelds + (hasPair ? 0 : 1))
            {
                // Pair as taatsu (when we already have a pair for jantai)
                if (hasPair && counts[i] >= 2)
                {
                    counts[i] -= 2;
                    SearchStandard(counts, i + 1, neededMelds, mentsu, taatsu + 1, hasPair, ref best);
                    counts[i] += 2;
                }

                // Adjacent pair (e.g., 23) — suit tiles only
                if (i < 27 && i % 9 <= 7 && counts[i + 1] > 0)
                {
                    counts[i]--;
                    counts[i + 1]--;
                    SearchStandard(counts, i, neededMelds, mentsu, taatsu + 1, hasPair, ref best);
                    counts[i]++;
                    counts[i + 1]++;
                }

                // Gap pair (e.g., 24) — suit tiles only
                if (i < 27 && i % 9 <= 6 && counts[i + 2] > 0)
                {
                    counts[i]--;
                    counts[i + 2]--;
                    SearchStandard(counts, i, neededMelds, mentsu, taatsu + 1, hasPair, ref best);
                    counts[i]++;
                    counts[i + 2]++;
                }
            }

            // Important: break after processing the first non-zero tile.
            // All combinations starting from this index have been explored.
            break;
        }
    }

    /// <summary>
    /// Seven pairs shanten: 6 - (number of pairs).
    /// Requires exactly 14 free tiles and no open melds.
    /// </summary>
    private static int CalculateSevenPairs(int[] counts)
    {
        var total = counts.Sum();
        if (total != 14) return 99; // not applicable

        var pairs = 0;
        var kinds = 0;
        for (var i = 0; i < 34; i++)
        {
            if (counts[i] >= 2) pairs++;
            if (counts[i] > 0) kinds++;
        }

        // Need 7 pairs from 7 different kinds.
        // If we have fewer than 7 kinds, we need extra kinds too.
        var shanten = 6 - pairs;
        if (kinds < 7)
            shanten += 7 - kinds;

        return shanten;
    }

    /// <summary>
    /// Thirteen orphans shanten: 13 - (number of unique terminal/honor tiles) - (has pair among them ? 1 : 0).
    /// </summary>
    private static int CalculateThirteenOrphans(int[] counts)
    {
        var total = counts.Sum();
        if (total != 14) return 99; // not applicable

        ReadOnlySpan<int> orphanIndices = [0, 8, 9, 17, 18, 26, 27, 28, 29, 30, 31, 32, 33];

        var uniqueCount = 0;
        var hasPair = false;
        foreach (var idx in orphanIndices)
        {
            if (counts[idx] > 0)
            {
                uniqueCount++;
                if (counts[idx] >= 2) hasPair = true;
            }
        }

        return 13 - uniqueCount - (hasPair ? 1 : 0);
    }
}
