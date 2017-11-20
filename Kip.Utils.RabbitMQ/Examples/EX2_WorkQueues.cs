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
    /// The assumption behind a work queue is that each task is delivered to exactly one worker
    /// 
    /// Message acknowledgment(turned on by default): An ack(nowledgement) is sent back from the consumer to tell RabbitMQ that a particular message has been received, processed and that RabbitMQ is free to delete it.
    /// 
    /// Message durability: Two things are required to make sure that messages aren't lost: we need to mark both the queue and messages as durable.
    /// 
    /// Fair dispatch: RabbitMQ just dispatches a message when the message enters the queue. It doesn't look at the number of unacknowledged messages for a consumer. It just blindly dispatches every n-th message to the n-th consumer.
    ///             In order to defeat that we can use the basicQos method with the prefetchCount = 1 setting. This tells RabbitMQ not to give more than one message to a worker at a time. Or, in other words, don't dispatch a new message to a worker until it has processed and acknowledged the previous one.
    ///             channel.BasicQos(0, 1, false);
    /// </summary>
    public class WorkQueues : RabbitMqGeneric
    {
        public void Send(string message)
        {
            // Message persistence：通过以下方式持久化消息并不能保证消息一定不会丢失，可能存在某个瞬间RabbitMQ接收到消息但是并未持久化到磁盘中（可能只保存在cache中但未真正写入磁盘）
            mqAction += (channel) => {
                channel.QueueDeclare(queue: "kip_task_queue",
                                 durable: true, // Queue持久化（到磁盘）
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true; // 消息持久化（到磁盘）

                channel.BasicPublish(exchange: "",
                                     routingKey: "kip_task_queue",
                                     basicProperties: properties,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            };

            this.process();
        }

        public void Receive()
        {
            mqAction += (channel) => {
                channel.QueueDeclare(queue: "kip_task_queue",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                // prefetchCount = 1：RabbitMQ 每次只会推送一个消息；换句话说，在消息没有处理完以及回应（acknowledged）的时候，不会推送新消息
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                Console.WriteLine(" [*] Waiting for messages.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);

                    int dots = message.Split('.').Length - 1;
                    Thread.Sleep(dots * 1000);

                    Console.WriteLine(" [x] Done");

                    //channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };
                channel.BasicConsume(queue: "kip_task_queue",
                                     noAck: true,  // 通知RabbitMQ消息已经接收到并已处理完成，RabbitMQ可以自由删除该消息。如果消费者停止工作（channel被关闭、链接中断、Tcp连接丢失等）导致没有发送Ack，RabbitMQ将会requeue该消息，随后发送给空闲的消费者处理
                                     consumer: consumer);
            };

            this.process();
        }
    }
}
