using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using MQTTnet.Server;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text;
using MQTTnet.Exceptions;

namespace MqttPcHeartbeatMonitor
{
    public interface IMqttService
    {
        Task<IMqttClient> Connect();
        Task Publish(IMqttClient client, string payload, string topic, bool retain = false);
    }

    public class MqttService : IMqttService
    {
        private readonly ILogger<MqttService> _logger;

        public MqttService(ILogger<MqttService> logger)
        {
            _logger = logger;
        }

        public async Task Publish(IMqttClient client, string payload, string topic, bool retain = false)
        {
            var messageBuilder = new MqttApplicationMessageBuilder()
                .WithTopic(topic.ToLower())
                .WithPayload(payload.ToLower())
                .WithRetainFlag(retain)
                .Build();

            try
            {
                await client.PublishAsync(messageBuilder);
            }
            catch
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append($"Topic: {messageBuilder.Topic}");
                stringBuilder.Append($"Payload: {messageBuilder.Payload}");
                stringBuilder.Append($"Retain Message: {messageBuilder.Retain}");

                _logger.LogError(stringBuilder.ToString());

                throw;
            }
        }

        public async Task<IMqttClient> Connect()
        {
            var options = ReadConfiguration();
            var client = new MqttFactory().CreateMqttClient();

            try
            {
                await client.ConnectAsync(options, CancellationToken.None);
            }
            catch
            {
                var json = Helpers.MqttConfigJson;
                var config = JsonConvert.DeserializeObject<Config>(json);

                var stringBuilder = new StringBuilder();
                stringBuilder.Append("Clinet could not connect to bridge\n");
                stringBuilder.Append("MQTT Configuration Options:\n");
                stringBuilder.Append($"Bridge: {config.BridgeUrl}:{config.BridgePort}\n");
                stringBuilder.Append($"Username: {config.BridgeUser.UserName}\n");
                stringBuilder.Append($"ClientId: {config.BridgeUser.ClientId}");

                throw new MqttConfigurationException(stringBuilder.ToString());
            }

            return client;
        }

        private IMqttClientOptions ReadConfiguration()
        {
            var json = Helpers.MqttConfigJson;

            if (!string.IsNullOrEmpty(json))
            {
                var config = JsonConvert.DeserializeObject<Config>(json);

                if (!string.IsNullOrEmpty(config.BridgeUser.UserName) && !string.IsNullOrEmpty(config.BridgeUser.Password))
                {
                    return new MqttClientOptionsBuilder()
                        .WithClientId(config.BridgeUser.ClientId)
                        .WithTcpServer(config.BridgeUrl, config.BridgePort)
                        .WithCleanSession()
                        .WithCredentials(config.BridgeUser.UserName, config.BridgeUser.Password)
                        .Build();
                }
                else
                {
                    return new MqttClientOptionsBuilder()
                        .WithClientId(config.BridgeUser.ClientId)
                        .WithTcpServer(config.BridgeUrl, config.BridgePort)
                        .WithCleanSession()
                        .Build();
                }
            }
            else
            {
                throw new FileNotFoundException($"config.json not found. Assembly location {Helpers.AssemblyDirectory}");
            }
        }
    }

    public class Config
    {
        public int BridgePort { get; set; }
        public string BridgeUrl { get; set; }
        public User BridgeUser { get; set; }
    }

    public class User
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }
    }
}
