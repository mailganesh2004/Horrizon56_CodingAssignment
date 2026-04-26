using HotChocolate.Types;
using Horizon56.Application.Plans.Queries.GetPlan;

namespace Horizon56.Api.GraphQL.Plans
{
    // PlanType tells HotChocolate what a "Plan" looks like in the GraphQL schema.
    // Without this, HotChocolate would try to expose every property on PlanDto automatically.
    // By defining it explicitly, we control the field names, types, and which fields are non-null.
    public class PlanType : ObjectType<PlanDto>
    {
        protected override void Configure(IObjectTypeDescriptor<PlanDto> descriptor)
        {
            // NonNullType means the field will never return null in the GraphQL response
            // UuidType maps the Guid to a UUID string in GraphQL (e.g. "3fa85f64-...")
            descriptor.Field(p => p.Id).Type<NonNullType<UuidType>>();
            descriptor.Field(p => p.Name).Type<NonNullType<StringType>>();
            descriptor.Field(p => p.CreatedAt).Type<NonNullType<DateTimeType>>();

            // Steps is a list, and neither the list itself nor any item inside it can be null
            descriptor.Field(p => p.Steps).Type<NonNullType<ListType<NonNullType<StepType>>>>();
        }
    }

    // StepType tells HotChocolate what a "Step" looks like in the GraphQL schema.
    public class StepType : ObjectType<StepDto>
    {
        protected override void Configure(IObjectTypeDescriptor<StepDto> descriptor)
        {
            descriptor.Field(s => s.Id).Type<NonNullType<UuidType>>();
            descriptor.Field(s => s.Name).Type<NonNullType<StringType>>();
            descriptor.Field(s => s.Order).Type<NonNullType<IntType>>();

            // EstimatedTime is the human-readable version, e.g. "1h 30m"
            descriptor.Field(s => s.EstimatedTime)
                .Type<NonNullType<StringType>>()
                .Description("Human-readable estimated time, e.g. '1h 30m'");

            // EstimatedMinutes is the same duration as a raw number, e.g. 90
            // Clients can use this for calculations without having to parse the string themselves
            descriptor.Field(s => s.EstimatedMinutes)
                .Type<NonNullType<IntType>>()
                .Description("Total minutes for this step");
        }
    }
}
