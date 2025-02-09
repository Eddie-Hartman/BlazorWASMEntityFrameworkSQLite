using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PWAExampleData.Data;

public class PWAExampleDbContextFactory : IDesignTimeDbContextFactory<PWAExampleDbContext>
{
    public const string DbName = "PWAExample.db";
    
    public PWAExampleDbContext CreateDbContext(string[] args)
    { 
        var builder = new DbContextOptionsBuilder<PWAExampleDbContext>();
        
        const string connectionString = $"Data Source={DbName}";

        builder.UseSqlite(connectionString);

        return new PWAExampleDbContext(builder.Options);
    }
}