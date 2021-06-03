using Bunny;
using Bunny.Messages;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;

namespace Requester
{
    internal static class Program
    {
        private const string REQUESTS_QUEUE = "generation_requests";

        internal static void Main(string[] args)
        {
            using var connection = RmqConnectionFactory.Create();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: REQUESTS_QUEUE, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var message = new RequestGeneration
            {
                Id = new Random().Next(1000),
                Steps = new[] { Step.GenerateImagePreview, Step.GenerateMetadata }
            };

            var body = JsonSerializer.SerializeToUtf8Bytes(message);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.CorrelationId = Guid.NewGuid().ToString();

            channel.BasicPublish(exchange: "", routingKey: REQUESTS_QUEUE, basicProperties: properties, body: body);
            Console.WriteLine(" [x] Sent {0}", message);
        }

        private const string EXCHANGE = "generations";
        private const string EXCHANGE_TYPE = "topic";
        private const string ROUTING_KEY = "generation.requests";

        internal static void Main2(string[] args)
        {
            using var connection = RmqConnectionFactory.Create();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: EXCHANGE, type: EXCHANGE_TYPE, durable: true);

            var message = "1";
            var body = Encoding.UTF8.GetBytes(message);
            var properties = channel.CreateBasicProperties();
            properties.CorrelationId = Guid.NewGuid().ToString();

            channel.BasicPublish(
                exchange: EXCHANGE,
                routingKey: ROUTING_KEY,
                basicProperties: properties,
                body: body);

            Console.WriteLine(" [x] Sent '{0}':'{1}' CID: {2}", ROUTING_KEY, message, properties.CorrelationId);
            Console.ReadKey();
        }
    }
}
