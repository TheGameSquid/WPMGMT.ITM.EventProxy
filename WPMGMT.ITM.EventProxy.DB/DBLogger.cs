using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using WPMGMT.ITM.EventProxy.Messages;
using Dapper;
using EasyNetQ;
using NLog;

namespace WPMGMT.ITM.EventProxy.DB
{
    class DBLogger
    {
        public DBLogger()
        {
            this.Running = false;
            this.Logger = LogManager.GetCurrentClassLogger();
        }

        public bool Running     { get; set; }
        public Logger Logger    { get; set; }

        public bool Start()
        {
            this.Logger.Info("Running...");
            this.Running = true;

            // Run the task async so the Start event can return true
            Task task = new Task(Run);
            task.Start();

            return true;
        }

        public bool Stop()
        {
            this.Logger.Info("Stopping...");
            this.Running = false;
            return true;
        }

        private async void Run()
        {
            using (IBus bus = RabbitHutch.CreateBus(ConfigurationManager.AppSettings["MQ"]))
            {
                // For the Queue ID I'm using the assembly name
                bus.Subscribe<TivoliMessage>("WPMGMT.ITM.EventProxy.DB", HandleTextMessage);

                while (true)
                {
 
                }

                Console.WriteLine("Listening for messages. Hit <return> to quit.");
                Console.ReadLine();
            }
        }

        private static void HandleTextMessage(TivoliMessage tivoliMessage)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Receiving Message: {0}", DateTime.Now);

            Console.ForegroundColor = ConsoleColor.Red;         
            
            Console.WriteLine("HostName: {0}", tivoliMessage.HostName);
            Console.WriteLine("HostIP: {0}", tivoliMessage.HostIP);
            Console.WriteLine("IntegrationType: {0}", tivoliMessage.IntegrationType);
            Console.WriteLine("SituationName: {0}", tivoliMessage.SituationName);
            Console.WriteLine("SituationType: {0}", tivoliMessage.SituationType);
            Console.WriteLine("SituationStatus: {0}", tivoliMessage.SituationStatus);
            Console.WriteLine("SituationDisplayItem: {0}", tivoliMessage.SituationDisplayItem);
            Console.WriteLine("Severity: {0}", tivoliMessage.Severity);
            Console.WriteLine("TimeStamp: {0}", tivoliMessage.TimeStamp);

            Console.ResetColor();
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DB"].ToString()))
            {
                connection.Execute("INSERT INTO [TEM].[EVENTS] (HostName, HostIP, IntegrationType, SituationName, SituationType, SituationStatus, SituationDisplayItem, Severity, TimeStamp) VALUES (@HostName, @HostIP, @IntegrationType, @SituationName, @SituationType, @SituationStatus, @SituationDisplayItem, @Severity, @TimeStamp)", tivoliMessage);
            }
        }
    }
}
