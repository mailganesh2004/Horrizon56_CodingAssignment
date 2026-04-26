using Horizon56.Domain.Plans;
using Horizon56.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Horizon56.Infrastructure
{
    // This class registers all Infrastructure layer services so the app knows how to create them.
    // It is called once at startup from Startup.cs: services.AddInfrastructure(Configuration)
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register EF Core's AppDbContext and tell it to use SQLite.
            // The connection string (file path of the .db file) is read from appsettings.json.
            // Example in appsettings.json: "ConnectionStrings": { "DefaultConnection": "Data Source=horizon56.db" }
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            // Register the repository so that when code asks for IPlanRepository,
            // the DI container creates a PlanRepository instance and provides it.
            // AddScoped means: one instance per HTTP request (shared within the request, then disposed).
            services.AddScoped<IPlanRepository, PlanRepository>();

            return services;
        }
    }
}
