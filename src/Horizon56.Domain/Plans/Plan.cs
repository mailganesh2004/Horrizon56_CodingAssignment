using System;
using System.Collections.Generic;

namespace Horizon56.Domain.Plans
{
    // Plan is the main "aggregate root" in DDD terms.
    // Think of it as the boss object — all changes to Steps must go through Plan.
    // You cannot create a Step directly; you must call plan.AddStep().
    // This ensures the Plan always controls what Steps belong to it.
    public class Plan
    {
        // Internal list that holds the steps — only this class can modify it
        private readonly List<Step> _steps = new();

        public Guid Id { get; private set; }
        public PlanName Name { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Exposed as read-only so outside code can read steps but cannot add/remove them directly
        public IReadOnlyCollection<Step> Steps => _steps.AsReadOnly();

        // EF Core needs a parameterless constructor to recreate objects from the database.
        // We make it private so application code cannot accidentally create an empty Plan.
        private Plan() { }

        // Use this method to create a new Plan — never use "new Plan()" directly.
        // Validates the name and assigns a new unique ID automatically.
        public static Plan Create(string name)
        {
            return new Plan
            {
                Id = Guid.NewGuid(),
                Name = PlanName.Create(name),  // validation happens inside PlanName
                CreatedAt = DateTime.UtcNow
            };
        }

        // The only way to add a Step to this Plan.
        // Creates the Step with the correct PlanId and validates name and estimated time.
        public Step AddStep(string name, int order, string estimatedTime)
        {
            var step = Step.Create(Id, name, order, estimatedTime);
            _steps.Add(step);
            return step;
        }
    }
}
