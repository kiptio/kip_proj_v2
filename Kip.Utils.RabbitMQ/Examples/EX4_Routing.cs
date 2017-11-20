using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Kip.Utils.RabbitMQ
{
    /// <summary>
    /// We're going to make it possible to subscribe only to a subset of the messages. 
    /// 
    /// Bindings: A binding is a relationship between an exchange and a queue. This can be simply read as: the queue is interested in messages from this exchange.
    /// Direct exchange: The routing algorithm behind a direct exchange is simple - a message goes to the queues whose binding key exactly matches the routing key of the message.
    /// </summary>
    public class Routing : RabbitMqGeneric
    {
        public void Send()
        {
            mqAction += (channel) => {
                channel.ExchangeDeclare(exchange: "direct_logs",
                                    type: "direct");

                string severity = "info";
                string message = "Hello World!";
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "direct_logs",
                                     routingKey: severity,
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent '{0}':'{1}'", severity, message);
            };

            this.process();
        }

        public void Receive()
        {
            mqAction += (channel) => {
                channel.ExchangeDeclare(exchange: "direct_logs",
                                    type: "direct");
                var queueName = channel.QueueDeclare().QueueName;

                string[] severities = new string[] { "info", "error"};
                foreach (var severity in severities)
                {
                    channel.QueueBind(queue: queueName,
                                      exchange: "direct_logs",
                                      routingKey: severity);
                }

                Console.WriteLine(" [*] Waiting for messages.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine(" [x] Received '{0}':'{1}'",
                                      routingKey, message);
                };
                channel.BasicConsume(queue: queueName,
                                     noAck: true,
                                     consumer: consumer);
            };

            this.process();
        }
    }
}
