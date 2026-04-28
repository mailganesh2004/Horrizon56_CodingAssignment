# Horizon56 Coding Assignment

A backend service for managing **Plans** and **Steps**, built with Clean Architecture principles on .NET 5.

## Tech Stack

| Technology | Version |
|---|---|
| .NET | 5.0 |
| ASP.NET Core + HotChocolate GraphQL | 12.0.0 |
| MediatR (CQRS) | 10.0.1 |
| Entity Framework Core + SQLite | 5.0.17 |

## Project Structure

```
Horizon56.Domain          ← Business entities, value objects, repository interfaces
Horizon56.Application     ← CQRS commands/queries via MediatR
Horizon56.Infrastructure  ← EF Core + SQLite, repository implementations
Horizon56.Api             ← HotChocolate GraphQL API (queries, mutations, subscriptions)
```

Each layer only depends on the layers inside it — Domain has zero external NuGet dependencies.

## Getting Started

```bash
cd src/Horizon56.Api
dotnet run
```

Open [http://localhost:5000/graphql](http://localhost:5000/graphql) in your browser to use the Banana Cake Pop GraphQL IDE.

The SQLite database (`horizon56.db`) is auto-created on first run via EF Core migrations.

## GraphQL Operations

**Queries**
```graphql
query {
  plan(id: "your-plan-id") {
    id
    name
    steps { id name order estimatedTime }
  }
}
```

**Mutations**
```graphql
mutation {
  createPlan(name: "My Plan")
  addStep(planId: "...", name: "Step 1", order: 1, estimatedTime: "1h 30m")
}
```

**Subscriptions**
```graphql
subscription {
  onPlanUpdated(planId: "...") {
    id
    steps { id name }
  }
}
```

## Key Design Decisions

- **Clean Architecture** — dependency rule enforced; domain logic never couples to frameworks or databases
- **DDD Aggregate Root** — `Plan` is the aggregate root; Steps are created only via `plan.AddStep(...)`, never directly
- **Value Objects** — `PlanName`, `StepName`, `EstimatedTime` validate once at creation and are always valid thereafter
- **CQRS** — commands mutate state through the domain; queries use `AsNoTracking()` for performance
- **GraphQL Subscriptions** — real-time push via HotChocolate in-memory pub/sub over WebSocket
- **Error Handling** — `DomainErrorFilter` converts domain exceptions to clean GraphQL errors (`DOMAIN_ERROR`)

## EstimatedTime Format

Steps accept duration strings in the following formats:

| Input | Stored As |
|---|---|
| `"45m"` | 45 minutes |
| `"1h"` | 60 minutes |
| `"1h 30m"` | 90 minutes |
| `"90m"` | 90 minutes |

Valid range: 1–479 minutes.
