using Bunny;
using Bunny.Messages;
using Microsoft.Toolkit.HighPerformance;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Supervisor
{
    internal static class Program
    {
        private const string REQUESTS_QUEUE = "generation_requests";

        internal static void Main(string[] args)
        {
            using var connection = RmqConnectionFactory.Create();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: REQUESTS_QUEUE, durable: true, exclusive: false, autoDelete: false, arguments: null);

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            Console.WriteLine(" [*] Waiting for messages.");

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                var message = await JsonSerializer.DeserializeAsync<RequestGeneration>(ea.Body.AsStream());
                var correlationId = ea.BasicProperties.CorrelationId;

                Console.WriteLine(" [x] Received {0}. CID: {1}", message, correlationId);

                var c = ((AsyncEventingBasicConsumer)model).Model;
                c.BasicAck(ea.DeliveryTag, false);

                await Task.Yield();
            };

            channel.BasicConsume(queue: REQUESTS_QUEUE, autoAck: false, consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private const string EXCHANGE = "generations";
        private const string EXCHANGE_TYPE = "topic";
        private const string BINDING_KEY = "generation.requests";

        internal static void Main2(string[] args)
        {
            using var connection = RmqConnectionFactory.Create();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: EXCHANGE, type: EXCHANGE_TYPE, durable: true);
            var queueName = channel.QueueDeclare().QueueName;

            channel.QueueBind(queue: queueName, exchange: EXCHANGE, routingKey: BINDING_KEY);

            Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var routingKey = ea.RoutingKey;
                var correlationId = ea.BasicProperties.CorrelationId;

                ProcessMessage(correlationId, message);
                Console.WriteLine(" [x] Received '{0}':'{1}' CID: {2}", routingKey, message, correlationId);
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            // TODO: Create a new channel that will listen on generations.step-completed

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static void ProcessMessage(string correlationId, string message)
        {
            // TODO: send a message to requires step processors
        }
    }
}
