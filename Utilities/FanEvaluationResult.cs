namespace MahjongScorer.Utilities;

public sealed class FanEvaluationResult
{
    public int TotalFan { get; set; }
    public List<string> FanNames { get; } = new();
    public Dictionary<string, string> Reasons { get; } = new();

    /// <summary>
    /// Human-readable description of the winning decomposition, e.g. "1万2万3万 4筒×3 5条×2".
    /// </summary>
    public string? DecompositionDescription { get; set; }

    /// <summary>
    /// Tile code groups for rendering the decomposition with icons.
    /// Each inner list is one meld or the pair, e.g. [["1m","2m","3m"], ["4p","4p","4p"], ["5s","5s"]].
    /// </summary>
    public List<List<string>>? DecompositionTileGroups { get; set; }

    /// <summary>
    /// Index into <see cref="DecompositionTileGroups"/> indicating which group
    /// the winning tile completed. -1 means unknown / not set.
    /// Used by the UI to highlight the winning tile in the correct group
    /// (e.g. edge wait 3m in 123m from hand, not in a chi 234m meld).
    /// </summary>
    public int WinningTileGroupIndex { get; set; } = -1;
}
