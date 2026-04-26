using System;
using System.Collections.Generic;
using Horizon56.Domain.Common;

namespace Horizon56.Domain.Plans
{
    // Represents the name of a Plan as a validated value.
    // Instead of passing a raw string everywhere (which could be empty or too long),
    // we wrap it in this class so the rules are enforced exactly once at creation time.
    // Once created, a PlanName is always valid — no need to re-check it anywhere else.
    public class PlanName : ValueObject
    {
        // The actual text of the plan name (already trimmed and validated)
        public string Value { get; }

        // Private constructor — only Create() below can make a PlanName
        private PlanName(string value)
        {
            Value = value;
        }

        // The only way to create a PlanName.
        // Validates the input before constructing the object.
        // Throws ArgumentException if the name is invalid.
        public static PlanName Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Plan name cannot contain only whitespace.");

            // Remove leading/trailing spaces before checking length
            var trimmed = value.Trim();

            if (trimmed.Length < 3 || trimmed.Length > 100)
                throw new ArgumentException("Plan name must be between 3–100 characters.");

            return new PlanName(trimmed);
        }

        // Two plan names are equal if their text matches (case-insensitive).
        // So "My Plan" and "my plan" are treated as the same plan name.
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value.ToLowerInvariant();
        }

        public override string ToString() => Value;

        // Allows using a PlanName directly wherever a string is expected
        public static implicit operator string(PlanName name) => name.Value;
    }
}
