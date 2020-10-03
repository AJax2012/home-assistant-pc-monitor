using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MqttPcHeartbeatMonitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((_, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddEventLog();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<IMqttService, MqttService>();
                    services.AddHostedService<Worker>();
                })
                .UseWindowsService();
    }
}
