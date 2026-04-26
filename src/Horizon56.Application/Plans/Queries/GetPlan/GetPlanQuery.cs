using System;
using MediatR;

namespace Horizon56.Application.Plans.Queries.GetPlan
{
    // A Query carries the data needed to READ something — no changes are made.
    // This one says: "Give me the Plan with this ID."
    // IRequest<PlanDto> means: it returns a PlanDto (a simple data object for the response).
    public record GetPlanQuery(Guid PlanId) : IRequest<PlanDto>;
}
