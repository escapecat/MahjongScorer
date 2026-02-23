namespace MahjongScorer.Services;

/// <summary>
/// Singleton service that persists calculator state across page navigations.
/// </summary>
public sealed class CalculatorStateService
{
    public Dictionary<string, int> HandCounts { get; } = new();
    public List<List<string>> ChiMelds { get; } = [];
    public List<List<string>> PengMelds { get; } = [];
    public List<List<string>> MingKongMelds { get; } = [];
    public List<List<string>> AnKongMelds { get; } = [];

    public bool IsSelfDraw { get; set; } = true;
    public string SeatWind { get; set; } = "E";
    public string RoundWind { get; set; } = "E";
    public int FlowerCount { get; set; }
    public string? WinningTile { get; set; }
    public bool IsLastTile { get; set; }
    public bool IsKongDraw { get; set; }
    public bool IsRobbingKong { get; set; }
    public bool IsWinningTileLast { get; set; }
    public bool SettingsOpen { get; set; } = true;

    public bool HasState => HandCounts.Count > 0
        || ChiMelds.Count > 0 || PengMelds.Count > 0
        || MingKongMelds.Count > 0 || AnKongMelds.Count > 0;
}
