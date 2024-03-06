using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace PassengerGenerator
{
    internal class Rabbit
    {
        private static Rabbit? _instance = null;
        ConnectionFactory _factory;
        IConnection _connection;

        // WQ - Write Queue - очередь для записи
        // RQ - Read Queue - очередь для считывания
        public string TicketsWQ = "";
        public string TicketsRQ = "";
        public string RegistrationWQ = "";
        public string RegistrationRQ = "";

        private Rabbit()
        {
            _factory = new ConnectionFactory
            {
                VirtualHost = "uyloeqsa",
                HostName = "kangaroo-01.rmq.cloudamqp.com",
                Password = "hDhr65HvuCvl1ayNAWa1euilU6bmH6FB",
                UserName = "uyloeqsa"
            };
            while (true)
            {
                try
                {
                    _connection = _factory.CreateConnection();
                    Log.Information("Connected to RabbitMQ host.");
                    break;
                }
                catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException)
                {
                    Log.Error("Can't connect to RabbitMQ host. Trying again in 1 sec.");
                    Thread.Sleep(1000);
                }
            }
        }

        public static Rabbit GetInstance() 
        {
            if (null == _instance)
                _instance = new Rabbit();
            return _instance;
        }

        public void PutMessage(string queue, string message)
        {
            Log.Information("Send to queue {Queue} message {Message}.", queue, message);
        }

        public string GetMessage(string queue)
        {
            string message = "test message";
            Log.Information("Got from queue {Queue} message {Message}.", queue, message);
            return "";
        }
    }
}
