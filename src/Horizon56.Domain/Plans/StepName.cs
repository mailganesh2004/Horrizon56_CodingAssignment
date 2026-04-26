using System;
using System.Collections.Generic;
using Horizon56.Domain.Common;

namespace Horizon56.Domain.Plans
{
    // Same idea as PlanName but for Steps.
    // Wraps a string and guarantees it is always valid before use.
    public class StepName : ValueObject
    {
        // The actual text of the step name (already trimmed and validated)
        public string Value { get; }

        // Private constructor — only Create() below can make a StepName
        private StepName(string value)
        {
            Value = value;
        }

        // The only way to create a StepName.
        // Throws ArgumentException if the name is invalid.
        public static StepName Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Step name cannot contain only whitespace.");

            var trimmed = value.Trim();

            if (trimmed.Length < 3 || trimmed.Length > 100)
                throw new ArgumentException("Step name must be between 3–100 characters.");

            return new StepName(trimmed);
        }

        // Two step names are equal if their text matches (case-insensitive).
        // So "Design Phase" and "design phase" are treated as the same step name.
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value.ToLowerInvariant();
        }

        public override string ToString() => Value;

        // Allows using a StepName directly wherever a string is expected
        public static implicit operator string(StepName name) => name.Value;
    }
}
