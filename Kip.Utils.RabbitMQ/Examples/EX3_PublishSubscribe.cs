using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;

namespace Kip.Utils.RabbitMQ
{
    /// <summary>
    /// 1.Deliver a message to multiple consumers
    /// 2.Published messages are going to be broadcast to all the receivers.
    /// 
    /// Exchanges: An exchange is a very simple thing. On one side it receives messages from producers and the other side it pushes them to queues.
    ///         Exchange types: There are a few exchange types available: direct, topic, headers and fanout. //channel.ExchangeDeclare("your-exchange-name", "your-exchange-type");
    ///             default(empty string or null): Messages are routed to the queue with the name specified by routingKey, if it exists.
    ///             "fanout": it just broadcasts all the messages it receives to all the queues it knows.
    ///             
    /// Temporary queues: Giving a queue a name is important when you want to share the queue between producers and consumers. A random queue name is used to get a fresh, empty queue.
    ///         When we supply no parameters to queueDeclare() we create a non-durable, exclusive, autodelete queue with generated a random queue name. For example it may look like amq.gen-JzTY20BRgKO-HjmUJj0wLg.
    /// </summary>
    public class PublishSubscribe : RabbitMqGeneric
    {
        public void Send(string message)
        {
            mqAction += (channel) => {
                channel.ExchangeDeclare(exchange: "logs", type: "fanout");

                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "logs",
                                     routingKey: "",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            };

            this.process();
        }

        public void Receive()
        {
            mqAction += (channel) => {
                channel.ExchangeDeclare(exchange: "logs", type: "fanout");

                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName,
                                  exchange: "logs",
                                  routingKey: "");

                Console.WriteLine(" [*] Waiting for logs.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] {0}", message);
                };
                channel.BasicConsume(queue: queueName,
                                     noAck: true,
                                     consumer: consumer);
            };

            this.process();
        }
    }
}
