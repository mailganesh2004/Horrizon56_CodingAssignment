using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Horizon56.Domain.Plans;
using MediatR;

namespace Horizon56.Application.Plans.Queries.GetPlan
{
    // This handler runs when a GetPlanQuery is sent through MediatR.
    // Its job: load the Plan from the database, then convert it to a PlanDto for the response.
    // No changes are made to the database — this is a read-only operation.
    public class GetPlanQueryHandler : IRequestHandler<GetPlanQuery, PlanDto>
    {
        private readonly IPlanRepository _repository;

        public GetPlanQueryHandler(IPlanRepository repository)
        {
            _repository = repository;
        }

        public async Task<PlanDto> Handle(GetPlanQuery request, CancellationToken cancellationToken)
        {
            // Load the Plan and all its Steps from the database
            var plan = await _repository.GetByIdAsync(request.PlanId, cancellationToken);

            if (plan is null)
                throw new InvalidOperationException($"Plan '{request.PlanId}' not found.");

            // Convert each Step into a StepDto, sorted by their order number
            var stepDtos = plan.Steps
                .OrderBy(step => step.Order)
                .Select(step => new StepDto(
                    step.Id,
                    step.Name.Value,
                    step.Order,
                    step.EstimatedTime.ToDisplayString(),   // "1h 30m"
                    step.EstimatedTime.TotalMinutes))       // 90
                .ToList();

            // Wrap everything into a PlanDto and return
            return new PlanDto(
                plan.Id,
                plan.Name.Value,
                plan.CreatedAt,
                stepDtos);
        }
    }
}
