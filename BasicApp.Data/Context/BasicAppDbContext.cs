using BasicApp.Data.Configuration;
using BasicApp.Data.Domain;
using Microsoft.EntityFrameworkCore;

namespace BasicApp.Data.Context;

public class BasicAppDbContext : DbContext
{
    public BasicAppDbContext(DbContextOptions<BasicAppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {

            optionsBuilder.UseSqlite("Data Source=BasicAppDb.sqlite");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
