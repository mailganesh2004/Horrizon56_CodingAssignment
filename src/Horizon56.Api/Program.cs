using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Horizon56.Api
{
    // Program.cs is the entry point of the application — the first code that runs.
    // Its only job is to build and start the web host using the configuration in Startup.cs.
    public class Program
    {
        public static void Main(string[] args)
        {
            // Build the host (wires up all configuration and services), then start it.
            // The app runs until the process is stopped (e.g. Ctrl+C in the terminal).
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Tell the host to use our Startup class for all service and middleware configuration
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
