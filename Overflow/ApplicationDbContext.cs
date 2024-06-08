using Microsoft.EntityFrameworkCore;
using Overflow.SouthernWater;

namespace Overflow;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Spill> SouthernWaterSpill { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Spill>();
    }
}