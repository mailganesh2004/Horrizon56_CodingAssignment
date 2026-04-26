using System;
using System.Threading;
using System.Threading.Tasks;

namespace Horizon56.Domain.Plans
{
    // This interface defines HOW we save and load Plans, without saying WHERE they are stored.
    // The Domain layer defines this contract; the Infrastructure layer (EF Core + SQLite) implements it.
    // This means we could switch from SQLite to PostgreSQL without touching any domain or application code.
    public interface IPlanRepository
    {
        // Load a Plan (and all its Steps) from the database by its ID.
        // Returns null if no Plan with that ID exists.
        Task<Plan> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        // Register a brand-new Plan so it gets saved on the next SaveChangesAsync call
        Task AddAsync(Plan plan, CancellationToken cancellationToken = default);

        // Register a brand-new Step so it gets saved on the next SaveChangesAsync call.
        // We add the Step directly (instead of through the Plan) to avoid EF Core
        // mistakenly trying to update the parent Plan row at the same time.
        Task AddStepAsync(Step step, CancellationToken cancellationToken = default);

        // Actually write all pending changes to the database
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
