using Horizon56.Api.GraphQL;
using Horizon56.Api.GraphQL.Plans;
using Horizon56.Api.GraphQL.Subscriptions;
using Horizon56.Application.Common;
using Horizon56.Infrastructure;
using Horizon56.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Horizon56.Api
{
    // Startup is the main configuration file for the web application.
    // ASP.NET Core calls these two methods automatically when the app starts:
    //   - ConfigureServices: register everything the app needs (database, GraphQL, etc.)
    //   - Configure: set up the request pipeline (which middleware runs in which order)
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // ConfigureServices is called first. All services registered here become available
        // via dependency injection throughout the application.
        public void ConfigureServices(IServiceCollection services)
        {
            // Register Application layer services (MediatR + all command/query handlers)
            services.AddApplication();

            // Register Infrastructure layer services (EF Core + SQLite + repository)
            services.AddInfrastructure(Configuration);

            // Set up the HotChocolate GraphQL server
            services
                .AddGraphQLServer()

                // Attach our error filter so domain validation errors surface cleanly
                .AddErrorFilter<DomainErrorFilter>()

                // Create an empty "Query" root type, then add plan queries from PlanQueries class
                .AddQueryType(d => d.Name("Query"))
                .AddTypeExtension<PlanQueries>()

                // Create an empty "Mutation" root type, then add plan mutations from PlanMutations class
                .AddMutationType(d => d.Name("Mutation"))
                .AddTypeExtension<PlanMutations>()

                // Create an empty "Subscription" root type, then add plan subscriptions
                .AddSubscriptionType(d => d.Name("Subscription"))
                .AddTypeExtension<PlanSubscriptions>()

                // Register the GraphQL types so HotChocolate knows the shape of Plan and Step
                .AddType<PlanType>()
                .AddType<StepType>()

                // Use an in-memory pub/sub bus for real-time subscriptions (no Redis needed)
                .AddInMemorySubscriptions();
        }

        // Configure is called after ConfigureServices.
        // It sets up the middleware pipeline — each call to app.Use... adds a step
        // that every incoming HTTP request passes through in order.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDbContext dbContext)
        {
            // Create the SQLite database file and apply any pending migrations on startup.
            // This means you don't need to run "dotnet ef database update" manually.
            dbContext.Database.Migrate();

            // Enable WebSocket support — required for GraphQL subscriptions to work
            app.UseWebSockets();

            // Enable routing so the app can match requests to endpoints
            app.UseRouting();

            // Map the GraphQL endpoint to /graphql (the default HotChocolate path)
            // This is where Banana Cake Pop (the built-in GraphQL IDE) is also served
            app.UseEndpoints(endpoints => endpoints.MapGraphQL());
        }
    }
}
