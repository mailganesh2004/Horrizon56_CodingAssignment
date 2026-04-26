using System;
using MediatR;

namespace Horizon56.Application.Plans.Commands.CreatePlan
{
    // A Command carries the data needed to perform an action.
    // This one says: "Create a plan with this name."
    // IRequest<Guid> means: when handled, it returns the new Plan's ID.
    public record CreatePlanCommand(string Name) : IRequest<Guid>;
}
