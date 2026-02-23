namespace MahjongScorer.Utilities;

/// <summary>
/// Decomposes a hand (with locked melds) into all valid pair+melds decompositions.
/// Locked melds (chi/peng/kong) are fixed; only remaining hand tiles are decomposed.
/// </summary>
public static class HandDecomposer
{
    /// <summary>
    /// Enumerate all valid decompositions for a standard winning hand.
    /// </summary>
    public static List<HandDecomposition> Decompose(
        int[] counts,
        IReadOnlyList<IReadOnlyList<string>>? melds,
        int chiCount, int pengCount, int mingKongCount, int anKongCount)
    {
        var results = new List<HandDecomposition>();

        // Convert locked melds to MeldInfo + IsOpen
        var lockedMelds = new List<MeldInfo>();
        var lockedOpen = new List<bool>();

        if (melds != null)
        {
            var meldIndex = 0;
            foreach (var meld in melds)
            {
                var info = ParseMeld(meld);
                if (info == null) continue;
                lockedMelds.Add(info);
                // First chiCount are chi (open), next pengCount are peng (open),
                // next mingKongCount are ming kong (open), last anKongCount are an kong (closed)
                var isOpen = meldIndex < chiCount + pengCount + mingKongCount;
                lockedOpen.Add(isOpen);
                meldIndex++;
            }
        }

        // Subtract locked meld tiles from counts to get remaining hand tiles
        var handCounts = counts.ToArray();
        foreach (var meld in lockedMelds)
        {
            foreach (var tile in meld.Tiles)
            {
                handCounts[tile] = Math.Max(0, handCounts[tile] - 1);
            }
        }

        var remaining = handCounts.Sum();
        if (remaining < 2) return results;

        // Try each possible pair from remaining hand tiles
        for (var i = 0; i < handCounts.Length; i++)
        {
            if (handCounts[i] < 2) continue;

            handCounts[i] -= 2;
            var handMelds = new List<MeldInfo>();
            DecomposeRecursive(handCounts, handMelds, results, lockedMelds, lockedOpen, i);
            handCounts[i] += 2;
        }

        return results;
    }

    private static void DecomposeRecursive(
        int[] counts,
        List<MeldInfo> currentMelds,
        List<HandDecomposition> results,
        List<MeldInfo> lockedMelds,
        List<bool> lockedOpen,
        int pairIndex)
    {
        // Find the first tile with count > 0
        var index = Array.FindIndex(counts, c => c > 0);
        if (index == -1)
        {
            // All tiles consumed — valid decomposition
            var decomp = new HandDecomposition
            {
                PairIndex = pairIndex,
                Melds = [.. lockedMelds, .. currentMelds],
                IsOpen = [.. lockedOpen, .. currentMelds.Select(_ => false)]
            };
            results.Add(decomp);
            return;
        }

        // Try triplet
        if (counts[index] >= 3)
        {
            counts[index] -= 3;
            currentMelds.Add(new MeldInfo(MeldKind.Triplet, index));
            DecomposeRecursive(counts, currentMelds, results, lockedMelds, lockedOpen, pairIndex);
            currentMelds.RemoveAt(currentMelds.Count - 1);
            counts[index] += 3;
        }

        // Try quad (4-of-a-kind as triplet — kongs are declared explicitly)
        if (counts[index] >= 4)
        {
            counts[index] -= 4;
            currentMelds.Add(new MeldInfo(MeldKind.Kong, index));
            DecomposeRecursive(counts, currentMelds, results, lockedMelds, lockedOpen, pairIndex);
            currentMelds.RemoveAt(currentMelds.Count - 1);
            counts[index] += 4;
        }

        // Try sequence (suit tiles only, same suit)
        if (index < 27 && index % 9 <= 6)
        {
            var next1 = index + 1;
            var next2 = index + 2;
            if (counts[next1] > 0 && counts[next2] > 0)
            {
                counts[index]--;
                counts[next1]--;
                counts[next2]--;
                currentMelds.Add(new MeldInfo(MeldKind.Sequence, index));
                DecomposeRecursive(counts, currentMelds, results, lockedMelds, lockedOpen, pairIndex);
                currentMelds.RemoveAt(currentMelds.Count - 1);
                counts[index]++;
                counts[next1]++;
                counts[next2]++;
            }
        }
    }

    private static MeldInfo? ParseMeld(IReadOnlyList<string> meld)
    {
        if (meld.Count == 0) return null;

        var first = meld[0];
        if (!TileConstants.TileIndexMap.TryGetValue(first, out var firstIndex))
            return null;

        // All same tile → triplet or kong
        if (meld.All(t => t == first))
        {
            return meld.Count == 4
                ? new MeldInfo(MeldKind.Kong, firstIndex)
                : new MeldInfo(MeldKind.Triplet, firstIndex);
        }

        // Sequence: sort by index, check consecutive same-suit
        if (meld.Count != 3) return null;

        var indices = new List<int>();
        foreach (var tile in meld)
        {
            if (!TileConstants.TileIndexMap.TryGetValue(tile, out var idx))
                return null;
            indices.Add(idx);
        }

        indices.Sort();
        if (indices[0] < 27 && indices[0] / 9 == indices[2] / 9
            && indices[1] == indices[0] + 1 && indices[2] == indices[0] + 2)
        {
            return new MeldInfo(MeldKind.Sequence, indices[0]);
        }

        return null;
    }
}
