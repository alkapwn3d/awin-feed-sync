using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AwinFeedSync.Infrastructure.Data;

public class AwinDbContextFactory : IDesignTimeDbContextFactory<AwinDbContext>
{
    public AwinDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AwinDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("Database__ConnectionString") 
            ?? "Host=localhost;Database=awin_feeds;Username=postgres;Password=postgres";
        
        optionsBuilder.UseNpgsql(connectionString);
        
        return new AwinDbContext(optionsBuilder.Options);
    }
}
