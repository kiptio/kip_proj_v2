using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kip.Utils.RabbitMQ
{
    public abstract class RabbitMqGeneric
    {
        protected Action<IModel> mqAction = null;

        protected void process()
        {
            var factory = new ConnectionFactory() { HostName = "192.168.88.130", UserName = "test", Password = "test" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    mqAction(channel);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }

        protected ConnectionFactory getConnectionFactory()
        {
            return new ConnectionFactory() { HostName = "192.168.88.130", UserName = "test", Password = "test" };
        }
    }
}
