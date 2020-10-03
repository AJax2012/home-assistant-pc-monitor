using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using MQTTnet.Server;
using System.Threading.Tasks;
using System;

namespace MqttPcHeartbeatMonitor
{
    
    public static class MqttService
    {
        public static async Task Publish(IMqttClient client, string payload, string topic, bool retain = false)
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
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async Task<IMqttClient> Connect()
        {
            var options = ReadConfiguration();
            var client = new MqttFactory().CreateMqttClient();
            try
            {
                await client.ConnectAsync(options, CancellationToken.None);
            }
            catch (Exception e)
            {
                throw e;
            }

            return client;
        }

        private static IMqttClientOptions ReadConfiguration()
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, "config.json");

            if (File.Exists(filePath))
            {
                using var r = new StreamReader(filePath);
                var json = r.ReadToEnd();
                var config = JsonConvert.DeserializeObject<Config>(json);

                return new MqttClientOptionsBuilder()
                    .WithClientId(config.BridgeUser.ClientId)
                    .WithTcpServer(config.BridgeUrl, config.BridgePort)
                    .WithCredentials(config.BridgeUser.UserName, config.BridgeUser.Password)
                    .WithCleanSession()
                    .Build();
            }
            else
            {
                return null;
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
