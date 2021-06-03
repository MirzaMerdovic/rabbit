using Bunny;
using Bunny.Messages;
using RabbitMQ.Client;
using System;
using System.Text.Json;

namespace Requester
{
    internal static class Program
    {
        private const string MODE = "direct";

        // Direct mode
        private const string REQUESTS_QUEUE = "generation_requests";

        // Topics mode
        private const string EXCHANGE = "generations";
        private const string EXCHANGE_TYPE = "topic";
        private const string ROUTING_KEY = "generation.requests";

        internal static void Main(string[] args)
        {
            using var connection = RmqConnectionFactory.Create();
            using var channel = connection.CreateModel();

            if (MODE == "direct")
            {
                channel.QueueDeclare(queue: REQUESTS_QUEUE, durable: true, exclusive: false, autoDelete: false, arguments: null);

                var (message, properties) = CreateMessageAndProperties(channel);
                var body = JsonSerializer.SerializeToUtf8Bytes(message);

                channel.BasicPublish(
                    exchange: "",
                    routingKey: REQUESTS_QUEUE,
                    basicProperties: properties,
                    body: body);

                Console.WriteLine(" [x] Sent '{0}':'{1}' CID: {2}", REQUESTS_QUEUE, message.Id, properties.CorrelationId);
            }
            else if (MODE == "topic")
            {
                channel.ExchangeDeclare(exchange: EXCHANGE, type: EXCHANGE_TYPE, durable: true);

                var (message, properties) = CreateMessageAndProperties(channel);
                var body = JsonSerializer.SerializeToUtf8Bytes(message);

                channel.BasicPublish(
                    exchange: EXCHANGE,
                    routingKey: ROUTING_KEY,
                    basicProperties: properties,
                    body: body);

                Console.WriteLine(" [x] Sent '{0}':'{1}' CID: {2}", ROUTING_KEY, message.Id, properties.CorrelationId);
            }
            else
                throw new NotImplementedException(MODE);
        }

        private static (RequestGeneration message, IBasicProperties properties) CreateMessageAndProperties(IModel channel)
        {
            var message = new RequestGeneration
            {
                Id = new Random().Next(1000),
                Steps = new[] { Step.GenerateImagePreview, Step.GenerateMetadata }
            };

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.CorrelationId = Guid.NewGuid().ToString();

            return (message, properties);
        }
    }
}
