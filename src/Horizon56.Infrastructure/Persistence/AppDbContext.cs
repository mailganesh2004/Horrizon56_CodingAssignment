using Horizon56.Domain.Plans;
using Microsoft.EntityFrameworkCore;

namespace Horizon56.Infrastructure.Persistence
{
    // AppDbContext is the EF Core "gateway" to the database.
    // It knows about the Plans and Steps tables, and handles reading/writing them.
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Represents the Plans table in the database
        public DbSet<Plan> Plans => Set<Plan>();

        // Represents the Steps table in the database
        public DbSet<Step> Steps => Set<Step>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Automatically picks up PlanConfiguration and StepConfiguration
            // from the same assembly to apply column mappings and rules
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
