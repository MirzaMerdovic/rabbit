using RabbitMQ.Client;
using System;

namespace Bunny
{
    public static class RmqConnectionFactory
    {
        private static readonly Uri Address = new Uri("amqps://tqhcacyr:DdEi_FHc-tjoVJndNO-2lfySLf3wgax6@hawk.rmq.cloudamqp.com/tqhcacyr");

        public static IConnection Create(Uri address = null, bool asyncConsumer = true)
        {
            var factory = new ConnectionFactory { Uri = address ?? Address };
            factory.DispatchConsumersAsync = asyncConsumer;

            return factory.CreateConnection();
        }
    }
}
