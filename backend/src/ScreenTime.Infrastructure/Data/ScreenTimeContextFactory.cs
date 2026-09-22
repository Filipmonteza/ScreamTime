using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ScreenTime.Infrastructure.Data;

/// <summary>
/// Lets the EF Core tools build a DbContext without going through the WebApi
/// project, so `dotnet ef` works from this folder and from the test project
/// without anyone having to remember --startup-project.
///
/// Design time only - the running application gets its connection string from
/// appsettings.json via AddInfrastructure.
/// </summary>
public class ScreenTimeContextFactory : IDesignTimeDbContextFactory<ScreenTimeDbContext>
{
    public ScreenTimeDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ScreenTimeDbContext>()
            .UseSqlite("Data Source=screenTime.db")
            .Options;

        return new ScreenTimeDbContext(options);
    }
}
