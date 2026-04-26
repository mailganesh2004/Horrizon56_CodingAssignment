using System;
using MediatR;

namespace Horizon56.Application.Plans.Commands.AddStep
{
    // A Command that says: "Add a step with these details to this plan."
    // IRequest<Guid> means: when handled, it returns the new Step's ID.
    public record AddStepCommand(
        Guid PlanId,
        string Name,
        int Order,
        string EstimatedTime) : IRequest<Guid>;
}
