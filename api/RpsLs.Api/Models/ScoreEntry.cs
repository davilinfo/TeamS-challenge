namespace RpsLs.Api.Models;

public record ScoreEntry(
    int Id,
    string Result,
    int Player,
    string PlayerName,
    int Computer,
    string ComputerName,
    DateTime PlayedAt
);
