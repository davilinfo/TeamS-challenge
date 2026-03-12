using Microsoft.EntityFrameworkCore;
using RpsLs.Infra.Entities;

namespace RpsLs.Infra.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ScoreRecord> Scores => Set<ScoreRecord>();
}
