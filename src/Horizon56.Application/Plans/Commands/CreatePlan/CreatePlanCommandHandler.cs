using System;
using System.Threading;
using System.Threading.Tasks;
using Horizon56.Domain.Plans;
using MediatR;

namespace Horizon56.Application.Plans.Commands.CreatePlan
{
    // This handler runs when a CreatePlanCommand is sent through MediatR.
    // Its job: create a Plan in the domain, save it to the database, return the new ID.
    //
    // Flow:
    //   GraphQL mutation → MediatR → this handler → Domain (Plan.Create) → Repository → SQLite
    public class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, Guid>
    {
        private readonly IPlanRepository _repository;

        public CreatePlanCommandHandler(IPlanRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
        {
            // Step 1: Create the Plan — name is validated inside Plan.Create()
            var plan = Plan.Create(request.Name);

            // Step 2: Register it with the database (not saved yet)
            await _repository.AddAsync(plan, cancellationToken);

            // Step 3: Actually write to the database
            await _repository.SaveChangesAsync(cancellationToken);

            // Step 4: Return the new Plan's ID to the caller
            return plan.Id;
        }
    }
}
