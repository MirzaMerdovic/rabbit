using Bunny;
using Bunny.Messages;
using Microsoft.Toolkit.HighPerformance;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Supervisor
{
    internal static class Program
    {
        private const string MODE = "direct";

        // Direct mode
        private const string REQUESTS_QUEUE = "generation_requests";

        // Topics mode
        private const string EXCHANGE = "generations";
        private const string EXCHANGE_TYPE = "topic";
        private const string BINDING_KEY = "generation.requests";

        internal static void Main(string[] args)
        {
            var connection = RmqConnectionFactory.Create();
            var channel = connection.CreateModel();

            if (MODE == "direct")
            {
                channel.QueueDeclare(queue: REQUESTS_QUEUE, durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                Console.WriteLine(" [*] Waiting for messages.");

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (model, ea) => await ProcessMessage(model, ea);

                channel.BasicConsume(queue: REQUESTS_QUEUE, autoAck: false, consumer: consumer);
            }
            else if (MODE == "topic")
            {
                channel.ExchangeDeclare(exchange: EXCHANGE, type: EXCHANGE_TYPE, durable: true);
                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName, exchange: EXCHANGE, routingKey: BINDING_KEY);

                Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (model, ea) => await ProcessMessage(model, ea);

                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            }
            else
                throw new NotImplementedException(MODE);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();

            channel.Close();
            connection.Close();
        }

        private static async Task ProcessMessage(object model, BasicDeliverEventArgs ea)
        {
            using var body = ea.Body.AsStream();
            var message = await JsonSerializer.DeserializeAsync<RequestGeneration>(body);

            // Do something with the message

            var correlationId = ea.BasicProperties.CorrelationId;
            var sourceQueue = ea.RoutingKey;
            Console.WriteLine(" [x] Received '{0}':'{1}' CID: {2}", sourceQueue, message.Id, correlationId);

            if (MODE == "direct")
            {
                var c = ((AsyncEventingBasicConsumer)model).Model;
                c.BasicAck(ea.DeliveryTag, false);
            }
        }
    }
}
