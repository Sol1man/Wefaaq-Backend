using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Wefaaq.Dal;

/// <summary>
/// Factory for creating WefaaqContext instances at design time (for EF migrations)
/// </summary>
public class WefaaqContextFactory : IDesignTimeDbContextFactory<WefaaqContext>
{
    public WefaaqContext CreateDbContext(string[] args)
    {
        // Build configuration to read from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Wefaaq.Api"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        // Create DbContextOptionsBuilder with SQL Server
        var optionsBuilder = new DbContextOptionsBuilder<WefaaqContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString);

        return new WefaaqContext(optionsBuilder.Options);
    }
}
