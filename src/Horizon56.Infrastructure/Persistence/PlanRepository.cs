using System;
using System.Threading;
using System.Threading.Tasks;
using Horizon56.Domain.Plans;
using Microsoft.EntityFrameworkCore;

namespace Horizon56.Infrastructure.Persistence
{
    // This is the real implementation of IPlanRepository using EF Core and SQLite.
    // The Domain only knows about the IPlanRepository interface — it never references this class.
    public class PlanRepository : IPlanRepository
    {
        private readonly AppDbContext _context;

        public PlanRepository(AppDbContext context)
        {
            _context = context;
        }

        // Load a Plan from the database, including all its Steps.
        // AsNoTracking() tells EF Core "don't watch this object for changes" —
        // this prevents EF from trying to UPDATE the Plan row when we only want to INSERT a new Step.
        public async Task<Plan> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Plans
                .Include(plan => plan.Steps)   // also load all Steps that belong to this Plan
                .AsNoTracking()
                .FirstOrDefaultAsync(plan => plan.Id == id, cancellationToken);
        }

        // Tell EF Core about a new Plan so it will be inserted on the next SaveChangesAsync
        public async Task AddAsync(Plan plan, CancellationToken cancellationToken = default)
        {
            await _context.Plans.AddAsync(plan, cancellationToken);
        }

        // Tell EF Core about a new Step so it will be inserted on the next SaveChangesAsync.
        // We add the Step directly to avoid EF mistakenly updating the parent Plan row.
        public async Task AddStepAsync(Step step, CancellationToken cancellationToken = default)
        {
            await _context.Steps.AddAsync(step, cancellationToken);
        }

        // Write all pending inserts/updates to the database in one go
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
