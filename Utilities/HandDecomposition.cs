namespace MahjongScorer.Utilities;

/// <summary>
/// A single valid decomposition of a winning hand into a pair + melds.
/// </summary>
public sealed class HandDecomposition
{
    public int PairIndex { get; init; } = -1;
    public List<MeldInfo> Melds { get; init; } = [];
    public List<bool> IsOpen { get; init; } = [];

    public int SequenceCount => Melds.Count(m => m.Kind == MeldKind.Sequence);
    public int TripletCount => Melds.Count(m => m.Kind is MeldKind.Triplet or MeldKind.Kong);

    public IEnumerable<MeldInfo> Sequences => Melds.Where(m => m.Kind == MeldKind.Sequence);
    public IEnumerable<MeldInfo> Triplets => Melds.Where(m => m.Kind is MeldKind.Triplet or MeldKind.Kong);
}
