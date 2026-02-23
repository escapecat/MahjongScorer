namespace MahjongScorer.Models;

public class Fan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Points { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Example { get; set; }
    public string? ExampleTiles { get; set; }
    public string Icon { get; set; } = "🀄";
    public FanCategory Category { get; set; }
}

public enum FanCategory
{
    Points88,
    Points64,
    Points48,
    Points32,
    Points24,
    Points16,
    Points12,
    Points8,
    Points6,
    Points4,
    Points2,
    Points1
}
