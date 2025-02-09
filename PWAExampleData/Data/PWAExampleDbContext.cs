using Microsoft.EntityFrameworkCore;

namespace PWAExampleData.Data;

public class PWAExampleDbContext : DbContext
{
    public PWAExampleDbContext(DbContextOptions<PWAExampleDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Counter> Counters { get; set; }
    
    public DbSet<ToDo> ToDos { get; set; }
}