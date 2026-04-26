using System;
using System.Threading;
using System.Threading.Tasks;
using Horizon56.Domain.Plans;
using MediatR;

namespace Horizon56.Application.Plans.Commands.AddStep
{
    // This handler runs when an AddStepCommand is sent through MediatR.
    // Its job: find the Plan, create a Step through it, save the Step to the database.
    //
    // Note: We load the Plan without change-tracking (AsNoTracking in the repository).
    // This means EF Core only saves the NEW Step — it won't try to update the Plan row too.
    public class AddStepCommandHandler : IRequestHandler<AddStepCommand, Guid>
    {
        private readonly IPlanRepository _repository;

        public AddStepCommandHandler(IPlanRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> Handle(AddStepCommand request, CancellationToken cancellationToken)
        {
            // Step 1: Load the Plan to make sure it exists
            var plan = await _repository.GetByIdAsync(request.PlanId, cancellationToken);

            if (plan is null)
                throw new InvalidOperationException($"Plan '{request.PlanId}' not found.");

            // Step 2: Create the Step through the Plan aggregate.
            // plan.AddStep() validates the name and estimated time, then returns the new Step.
            var step = plan.AddStep(request.Name, request.Order, request.EstimatedTime);

            // Step 3: Register the new Step with the database (not saved yet)
            await _repository.AddStepAsync(step, cancellationToken);

            // Step 4: Write the new Step to the database
            await _repository.SaveChangesAsync(cancellationToken);

            return step.Id;
        }
    }
}
