using System.Collections.Generic;
using System.Linq;

namespace Horizon56.Domain.Common
{
    // A Value Object is compared by its DATA, not by its memory address.
    // Example: two "PlanName" objects with the same text are considered equal,
    // just like how two strings "hello" == "hello" are equal regardless of which object they are.
    //
    // Any class that inherits this must implement GetEqualityComponents()
    // to tell us WHICH fields to use for comparison.
    public abstract class ValueObject
    {
        // Subclasses return the fields that define equality.
        // Example: PlanName yields its text in lowercase so "My Plan" == "my plan"
        protected abstract IEnumerable<object> GetEqualityComponents();

        // Compare two value objects field by field
        public override bool Equals(object obj)
        {
            // Must be same type — a PlanName can never equal a StepName
            if (obj is null || obj.GetType() != GetType())
                return false;

            var other = (ValueObject)obj;

            // Compare each field returned by GetEqualityComponents one by one
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        // Build a hash code from all the equality fields.
        // This is required so value objects work correctly in dictionaries and hash sets.
        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Aggregate(1, (current, component) => current * 23 + (component?.GetHashCode() ?? 0));
        }

        // Allow using == and != operators between value objects
        public static bool operator ==(ValueObject left, ValueObject right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(ValueObject left, ValueObject right) => !(left == right);
    }
}
