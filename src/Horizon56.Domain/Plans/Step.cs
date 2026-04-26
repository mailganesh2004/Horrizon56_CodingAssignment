using System;

namespace Horizon56.Domain.Plans
{
    // A Step belongs to a Plan and represents one task within that plan.
    // Steps are always created through Plan.AddStep() — never directly.
    // This ensures the PlanId is always set correctly.
    public class Step
    {
        public Guid Id { get; private set; }
        public StepName Name { get; private set; }

        // The position of this step within the plan (e.g. step 1, step 2, step 3)
        public int Order { get; private set; }

        // How long this step is expected to take (e.g. "1h 30m")
        public EstimatedTime EstimatedTime { get; private set; }

        // Which plan this step belongs to (the foreign key in the database)
        public Guid PlanId { get; private set; }

        // Private so EF Core can reconstruct Step objects from the database,
        // but application code cannot create a Step without going through Plan.AddStep()
        private Step() { }

        // Called by Plan.AddStep() — validates all fields and creates the Step
        public static Step Create(Guid planId, string name, int order, string estimatedTime)
        {
            return new Step
            {
                Id = Guid.NewGuid(),
                PlanId = planId,
                Name = StepName.Create(name),                 // validates name rules
                Order = order,
                EstimatedTime = EstimatedTime.Create(estimatedTime)  // validates time rules
            };
        }
    }
}
