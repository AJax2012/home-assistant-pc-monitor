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
        private readonly IMqttService _mqttService;

        public Worker(ILogger<Worker> logger, IMqttService mqttService)
        {
            _logger = logger;
            _mqttService = mqttService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);
                var message = $"{Environment.MachineName}/idleStatus";
                IMqttClient mqttClient = await _mqttService.Connect();

                while (!stoppingToken.IsCancellationRequested)
                {
                    if (!mqttClient.IsConnected)
                    {
                        mqttClient = await _mqttService.Connect();
                    }

                    var topic = GetLastUserInput.IsIdle().ToString();

                    await _mqttService.Publish(mqttClient, topic, message);
                    _logger.LogInformation("Message: " + message);
                    await Task.Delay(10000, stoppingToken);
                }

                await mqttClient.DisconnectAsync();
                _logger.LogInformation("Worker stopped at {time}", DateTimeOffset.Now);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                if (e.InnerException != null)
                {
                    _logger.LogError(e.InnerException.Message);
                }
                throw;
            }
        }
    }
}
