using Horizon56.Domain.Plans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Horizon56.Infrastructure.Persistence.Configurations
{
    // This class tells EF Core exactly how to store a Step in the database.
    // EF Core finds this class automatically because we called ApplyConfigurationsFromAssembly() in AppDbContext.
    public class StepConfiguration : IEntityTypeConfiguration<Step>
    {
        public void Configure(EntityTypeBuilder<Step> builder)
        {
            // Tell EF Core that the Id property is the primary key
            builder.HasKey(s => s.Id);

            // Step.Name is a StepName object, but SQLite only knows strings.
            // Converter: save as string, load back as StepName object
            var nameConverter = new ValueConverter<StepName, string>(
                stepName => stepName.Value,                // when saving: extract the string
                rawValue => StepName.Create(rawValue));    // when loading: wrap back into a StepName

            // Comparer: teaches EF Core how to detect changes to StepName values
            var nameComparer = new ValueComparer<StepName>(
                (left, right) => left != null && right != null && left.Value == right.Value,
                stepName => stepName.Value.ToLowerInvariant().GetHashCode(),
                stepName => StepName.Create(stepName.Value));

            // Step.EstimatedTime is an EstimatedTime object, but SQLite stores it as a plain integer (minutes).
            // Converter: save as total minutes (e.g. 90), load back as EstimatedTime object
            var estimatedTimeConverter = new ValueConverter<EstimatedTime, int>(
                estimatedTime => estimatedTime.TotalMinutes,            // when saving: store as integer minutes
                totalMinutes => EstimatedTime.FromMinutes(totalMinutes)); // when loading: rebuild the object

            // Comparer: teaches EF Core how to detect changes to EstimatedTime values
            var estimatedTimeComparer = new ValueComparer<EstimatedTime>(
                (left, right) => left != null && right != null && left.TotalMinutes == right.TotalMinutes,
                estimatedTime => estimatedTime.TotalMinutes.GetHashCode(),
                estimatedTime => EstimatedTime.FromMinutes(estimatedTime.TotalMinutes));

            // Apply the name converter/comparer — max 100 characters, cannot be null
            builder.Property(s => s.Name)
                .HasConversion(nameConverter, nameComparer)
                .HasMaxLength(100)
                .IsRequired();

            // Apply the estimated time converter/comparer — stored as an integer in the DB
            builder.Property(s => s.EstimatedTime)
                .HasConversion(estimatedTimeConverter, estimatedTimeComparer)
                .IsRequired();

            // These are simple value types — just mark them as required (cannot be null)
            builder.Property(s => s.Order).IsRequired();
            builder.Property(s => s.PlanId).IsRequired();
        }
    }
}
