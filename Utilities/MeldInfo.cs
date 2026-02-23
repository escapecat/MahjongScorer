namespace MahjongScorer.Utilities;

/// <summary>
/// Represents a single meld (group of tiles) in a decomposed hand.
/// </summary>
public sealed record MeldInfo(MeldKind Kind, int TileIndex)
{
    /// <summary>Tile indices that make up this meld.</summary>
    public int[] Tiles => Kind switch
    {
        MeldKind.Pair => [TileIndex, TileIndex],
        MeldKind.Triplet => [TileIndex, TileIndex, TileIndex],
        MeldKind.Kong => [TileIndex, TileIndex, TileIndex, TileIndex],
        MeldKind.Sequence => [TileIndex, TileIndex + 1, TileIndex + 2],
        _ => []
    };
}
