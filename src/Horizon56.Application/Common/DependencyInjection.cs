using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Horizon56.Application.Common
{
    public static class DependencyInjection
    {
        // Registers all Application layer services with the DI container.
        // MediatR scans this assembly to find all Command and Query handlers automatically.
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}
