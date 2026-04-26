using System;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using Horizon56.Application.Plans.Commands.AddStep;
using Horizon56.Application.Plans.Commands.CreatePlan;
using Horizon56.Application.Plans.Queries.GetPlan;
using MediatR;

namespace Horizon56.Api.GraphQL.Plans
{
    // This class adds "createPlan" and "addStep" fields to the GraphQL Mutation type.
    // Mutations are for write operations (creating or changing data), unlike queries which are read-only.
    // [ExtendObjectType] means we're adding to the Mutation type, not replacing it.
    [ExtendObjectType("Mutation")]
    public class PlanMutations
    {
        // Creates a new Plan and returns its new Id (a GUID).
        //
        // GraphQL usage:
        //   mutation { createPlan(name: "Website Redesign") }
        //
        // Flow: GraphQL client sends mutation → HotChocolate calls this method
        //       → MediatR → CreatePlanCommandHandler → Plan.Create() → repository → SQLite
        public async Task<Guid> CreatePlan(
            string name,
            [Service] IMediator mediator,
            CancellationToken cancellationToken)
        {
            // Send the command to MediatR. The handler will create and save the Plan, then return its new Id.
            return await mediator.Send(new CreatePlanCommand(name), cancellationToken);
        }

        // Adds a new Step to an existing Plan and returns the updated Plan.
        // Also notifies any active real-time subscribers that this Plan has changed.
        //
        // GraphQL usage:
        //   mutation {
        //     addStep(planId: "...", name: "Design Phase", order: 1, estimatedTime: "1h 30m") {
        //       id name steps { name order estimatedTime estimatedMinutes }
        //     }
        //   }
        //
        // Flow: GraphQL client sends mutation → this method
        //       → Step 1: save the new Step via MediatR command
        //       → Step 2: reload the updated Plan via MediatR query
        //       → Step 3: push the updated Plan to all WebSocket subscribers
        //       → Step 4: return the updated Plan to the mutation caller
        public async Task<PlanDto> AddStep(
            Guid planId,
            string name,
            int order,
            string estimatedTime,
            [Service] IMediator mediator,
            [Service] ITopicEventSender eventSender,  // used to push real-time updates to subscribers
            CancellationToken cancellationToken)
        {
            // Step 1: Save the new Step to the database
            await mediator.Send(new AddStepCommand(planId, name, order, estimatedTime), cancellationToken);

            // Step 2: Reload the Plan so we have all its Steps including the one just added
            var updatedPlan = await mediator.Send(new GetPlanQuery(planId), cancellationToken);

            // Step 3: Publish the updated Plan to anyone subscribed to this Plan's topic.
            // The topic name is unique per Plan so each subscriber only gets updates for their Plan.
            // PlanSubscriptions.SubscribeToPlanUpdated() listens on this exact same topic name.
            await eventSender.SendAsync($"PlanUpdated_{planId}", updatedPlan, cancellationToken);

            // Step 4: Return the updated Plan to the client who called addStep
            return updatedPlan;
        }
    }
}
