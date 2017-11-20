using Kip.Utils.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kip.Test.Common
{
    class Program
    {
        static void Main(string[] args)
        {
            new WorkQueues().Send("Hello ...");
            //new WorkQueues().Receive();
        }
    }
}
