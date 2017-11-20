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
    /// In the previous tutorial we improved our logging system. Instead of using a fanout exchange only capable of dummy broadcasting, we used a direct one, and gained a possibility of selectively receiving the logs.
    /// 
    /// Topic exchange: Messages sent to a topic exchange can't have an arbitrary routing_key - it must be a list of words, delimited by dots. Examples: "stock.usd.nyse", "nyse.vmw", "quick.orange.rabbit".
    ///             The logic behind the topic exchange is similar to a direct one - a message sent with a particular routing key will be delivered to all the queues that are bound with a matching binding key. 
    ///             There are two important special cases for binding keys:
    ///                 * (star) can substitute for exactly one word.
    ///                 # (hash) can substitute for zero or more words.
    ///             When a queue is bound with "#" (hash) binding key - it will receive all the messages, regardless of the routing key - like in fanout exchange.
    ///             When special characters "*" (star) and "#" (hash) aren't used in bindings, the topic exchange will behave just like a direct one.
    /// </summary>
    public class Topics : RabbitMqGeneric
    {
        public void Send(string routingKey)
        {
            mqAction += (channel) => {
                channel.ExchangeDeclare(exchange: "topic_logs",
                                    type: "topic");

                if(string.IsNullOrEmpty(routingKey)) routingKey = "anonymous.info";

                var message = "Hello World!";
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "topic_logs",
                                     routingKey: routingKey,
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent '{0}':'{1}'", routingKey, message);
            };

            this.process();
        }

        public void Receive(string[] bindingKeys)
        {
            mqAction += (channel) => {
                channel.ExchangeDeclare(exchange: "topic_logs", type: "topic");
                var queueName = channel.QueueDeclare().QueueName;

                foreach (var bindingKey in bindingKeys)
                {
                    channel.QueueBind(queue: queueName,
                                      exchange: "topic_logs",
                                      routingKey: bindingKey);
                }

                Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine(" [x] Received '{0}':'{1}'",
                                      routingKey,
                                      message);
                };
                channel.BasicConsume(queue: queueName,
                                     noAck: true,
                                     consumer: consumer);
            };

            this.process();
        }
    }
}
