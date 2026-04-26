using System;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using Horizon56.Application.Plans.Queries.GetPlan;

namespace Horizon56.Api.GraphQL.Subscriptions
{
    // This class adds "onPlanUpdated" to the GraphQL Subscription type.
    // Subscriptions are long-lived connections (via WebSocket) where the client waits
    // and the server pushes updates whenever something changes.
    //
    // How it works end-to-end:
    //   1. Client opens a WebSocket and sends: subscription { onPlanUpdated(planId: "...") { ... } }
    //   2. HotChocolate calls SubscribeToPlanUpdated() to set up a stream for that plan
    //   3. When someone calls the addStep mutation, PlanMutations.AddStep() publishes to the topic
    //   4. HotChocolate receives the event and calls OnPlanUpdated() to get the response
    //   5. The client receives the updated Plan data over the WebSocket in real time
    //
    // GraphQL usage (open in a separate tab):
    //   subscription {
    //     onPlanUpdated(planId: "your-plan-id") {
    //       id name steps { name order estimatedTime }
    //     }
    //   }
    [ExtendObjectType("Subscription")]
    public class PlanSubscriptions
    {
        // This method is called by HotChocolate each time an event arrives on the topic.
        // [Subscribe] links this to SubscribeToPlanUpdated() which sets up the stream.
        // [EventMessage] tells HotChocolate to inject the published PlanDto as the "plan" parameter.
        [Subscribe(With = nameof(SubscribeToPlanUpdated))]
        [Topic("{planId}")]
        public PlanDto OnPlanUpdated(
            Guid planId,
            [EventMessage] PlanDto plan)
        {
            // Just pass the received PlanDto directly to the client.
            // The plan was already built and published by PlanMutations.AddStep().
            return plan;
        }

        // This method is called once when a client first connects to subscribe.
        // It registers the client as a listener on the plan-specific topic.
        // The topic name must match exactly what PlanMutations.AddStep() publishes to.
        public ISourceStream<PlanDto> SubscribeToPlanUpdated(
            Guid planId,
            [Service] ITopicEventReceiver receiver)
        {
            // Subscribe to the in-memory event bus for this specific plan's topic.
            // When PlanMutations publishes to "PlanUpdated_{planId}", this stream receives it.
            string topicName = $"PlanUpdated_{planId}";
            return receiver.SubscribeAsync<string, PlanDto>(topicName).GetAwaiter().GetResult();
        }
    }
}
