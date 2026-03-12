using Microsoft.EntityFrameworkCore;
using RpsLs.ApplicationService.Models;
using RpsLs.ApplicationService.Services;
using RpsLs.Infra.Data;
using RpsLs.Infra.Entities;

namespace RpsLs.Infra.Repositories;

public class ScoreRepository(AppDbContext context) : IScoreRepository
{
    public async Task AddAsync(ScoreEntry entry)
    {
        var record = new ScoreRecord
        {
            Result = entry.Result,
            Player = entry.Player,
            PlayerName = entry.PlayerName,
            Computer = entry.Computer,
            ComputerName = entry.ComputerName,
            PlayedAt = entry.PlayedAt,
        };
        context.Scores.Add(record);
        await context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<ScoreEntry>> GetLastAsync(int count)
    {
        var records = await context.Scores.AsNoTracking()
            .OrderByDescending(s => s.PlayedAt)
            .Take(count)
            .ToListAsync();

        return records
            .Select(r => new ScoreEntry(r.Id, r.Result, r.Player, r.PlayerName, r.Computer, r.ComputerName, r.PlayedAt))
            .ToList()
            .AsReadOnly();
    }

    public async Task ClearAsync()
    {
        context.Scores.RemoveRange(context.Scores);
        await context.SaveChangesAsync();
    }
}
