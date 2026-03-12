using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RpsLs.Infra.Data;

/// <summary>
/// Used by the EF Core CLI tools (dotnet ef migrations add/update) so they can
/// instantiate AppDbContext with SQL Server without needing the full app host.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=localhost;Database=RpsLsDb;User Id=sa;Password=RpsLs_Str0ng!Pass;TrustServerCertificate=True")
            .Options;
        return new AppDbContext(options);
    }
}
