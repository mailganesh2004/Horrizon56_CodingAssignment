using Horizon56.Domain.Plans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Horizon56.Infrastructure.Persistence.Configurations
{
    // This class tells EF Core exactly how to store a Plan in the database.
    // EF Core finds this class automatically because we called ApplyConfigurationsFromAssembly() in AppDbContext.
    public class PlanConfiguration : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> builder)
        {
            // Tell EF Core that the Id property is the primary key (the unique row identifier)
            builder.HasKey(p => p.Id);

            // PROBLEM: Plan.Name is a PlanName object, but SQLite only understands plain strings.
            // SOLUTION: A ValueConverter teaches EF Core how to translate between them.
            //   - Writing to DB:  PlanName object → its .Value string (e.g. "My Plan")
            //   - Reading from DB: string → PlanName.Create(string) → back to a PlanName object
            var nameConverter = new ValueConverter<PlanName, string>(
                planName => planName.Value,                // when saving: extract the string
                rawValue => PlanName.Create(rawValue));    // when loading: wrap back into a PlanName

            // PROBLEM: EF Core also needs to compare PlanName objects to detect changes.
            // If it can't compare them, it won't know when to run an UPDATE.
            // SOLUTION: A ValueComparer tells EF Core three things:
            //   1. How to check if two PlanName values are equal
            //   2. How to compute a hash code (used for quick lookups)
            //   3. How to create a fresh copy (EF keeps a snapshot of the original value)
            var nameComparer = new ValueComparer<PlanName>(
                (left, right) => left != null && right != null && left.Value == right.Value,
                planName => planName.Value.ToLowerInvariant().GetHashCode(),
                planName => PlanName.Create(planName.Value));

            // Apply the converter and comparer to the Name column, with a max length of 100
            builder.Property(p => p.Name)
                .HasConversion(nameConverter, nameComparer)
                .HasMaxLength(100)
                .IsRequired();

            // Set up the one-to-many relationship: one Plan has many Steps
            // - WithOne() means each Step belongs to exactly one Plan
            // - HasForeignKey means the Steps table stores the Plan's Id in a "PlanId" column
            // - OnDelete(Cascade) means: when a Plan is deleted, all its Steps are also deleted automatically
            builder.HasMany(p => p.Steps)
                .WithOne()
                .HasForeignKey(step => step.PlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // PROBLEM: Plan stores its steps in a private field called _steps (not a public property).
            // EF Core won't find private fields by default when populating data from the database.
            // SOLUTION: Tell EF Core to use the backing field directly instead of going through a property.
            builder.Metadata
                .FindNavigation(nameof(Plan.Steps))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
