namespace MahjongScorer.Utilities;

public sealed record FanEvaluationContext(
    int[] Counts,
    bool IsSelfDraw,
    int FlowerCount,
    string SeatWind,
    string RoundWind,
    int ChiCount,
    int PengCount,
    int MingKongCount,
    int AnKongCount,
    string? WinningTile = null,
    bool IsLastTile = false,
    bool IsKongDraw = false,
    bool IsRobbingKong = false,
    bool IsWinningTileLast = false,
    IReadOnlyList<IReadOnlyList<string>>? Melds = null)
{
    public bool HasChi => ChiCount > 0;
    public bool HasPeng => PengCount > 0;
    public bool HasMingKong => MingKongCount > 0;
    public bool HasAnKong => AnKongCount > 0;
}
