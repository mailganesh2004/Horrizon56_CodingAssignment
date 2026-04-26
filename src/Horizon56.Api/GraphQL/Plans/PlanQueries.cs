using System;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using Horizon56.Application.Plans.Queries.GetPlan;
using MediatR;

namespace Horizon56.Api.GraphQL.Plans
{
    // This class adds "plan" as a field on the GraphQL Query type.
    // [ExtendObjectType] means: "don't replace the Query type, just add fields to it."
    // This keeps each feature's queries in its own file instead of one giant class.
    //
    // GraphQL usage:
    //   query {
    //     plan(id: "your-plan-id") {
    //       id name createdAt
    //       steps { id name order estimatedTime estimatedMinutes }
    //     }
    //   }
    [ExtendObjectType("Query")]
    public class PlanQueries
    {
        // Flow: GraphQL client sends query → HotChocolate calls this method → MediatR → GetPlanQueryHandler → repository → SQLite
        //
        // [Service] tells HotChocolate to inject IMediator from the DI container automatically.
        // The client only needs to provide the plan "id".
        public async Task<PlanDto> GetPlan(
            Guid id,
            [Service] IMediator mediator,
            CancellationToken cancellationToken)
        {
            // Wrap the id in a query object and send it through MediatR.
            // MediatR finds GetPlanQueryHandler and calls its Handle method.
            return await mediator.Send(new GetPlanQuery(id), cancellationToken);
        }
    }
}
