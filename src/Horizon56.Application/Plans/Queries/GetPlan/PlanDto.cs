using System;
using System.Collections.Generic;

namespace Horizon56.Application.Plans.Queries.GetPlan
{
    // DTO = Data Transfer Object.
    // A simple container for sending Plan data back to the client (GraphQL response).
    // Unlike the Plan domain object, this has no behaviour — it just holds data.
    public record PlanDto(
        Guid Id,
        string Name,
        DateTime CreatedAt,
        IReadOnlyList<StepDto> Steps);

    // A simple container for sending Step data back to the client.
    // EstimatedTime is provided both as a readable string ("1h 30m") and as raw minutes (90),
    // so clients can choose how to display or calculate with it.
    public record StepDto(
        Guid Id,
        string Name,
        int Order,
        string EstimatedTime,       // e.g. "1h 30m"
        int EstimatedMinutes);      // e.g. 90
}
