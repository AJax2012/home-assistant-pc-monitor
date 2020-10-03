using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;

namespace MqttPcHeartbeatMonitor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);
            var message = $"{Environment.MachineName}/idleStatus";
            IMqttClient mqttClient = await MqttService.Connect();

            while (!stoppingToken.IsCancellationRequested)
            {
                if (!mqttClient.IsConnected)
                {
                    mqttClient = await MqttService.Connect();
                }

                var topic = GetLastUserInput.IsIdle().ToString();

                await MqttService.Publish(mqttClient, topic, message);
                _logger.LogInformation("Message: " + message);
                await Task.Delay(10000, stoppingToken);
            }

            await mqttClient.DisconnectAsync();
            _logger.LogInformation("Worker stopped at {time}", DateTimeOffset.Now);
        }
    }
}
