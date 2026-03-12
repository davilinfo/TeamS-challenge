namespace RpsLs.Infra.Entities;

public class ScoreRecord
{
    public int Id { get; set; }
    public string Result { get; set; } = string.Empty;
    public int Player { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Computer { get; set; }
    public string ComputerName { get; set; } = string.Empty;
    public DateTime PlayedAt { get; set; }
}
