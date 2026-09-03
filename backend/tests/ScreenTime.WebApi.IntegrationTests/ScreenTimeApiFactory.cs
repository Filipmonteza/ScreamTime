using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ScreenTime.Infrastructure.Data;

namespace ScreenTime.WebApi.IntegrationTests;

/// <summary>
/// Boots the real API against a throwaway SQLite in-memory database.
/// SQLite is a relational provider, so migrations, foreign keys and unique
/// indexes behave exactly as they do against the real database file. The
/// EF InMemory provider ignores unique indexes, which would let a broken
/// double-booking constraint pass its own test unnoticed.
/// </summary>
public class ScreenTimeApiFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Drop the file-backed DbContext registered by AddInfrastructure.
            services.RemoveAll<DbContextOptions<ScreenTimeDbContext>>();
            services.RemoveAll<DbContextOptions>();

            // Held open for the lifetime of the factory: an in-memory SQLite
            // database is discarded as soon as its last connection closes.
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<ScreenTimeDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection?.Dispose();
        }
    }
}
