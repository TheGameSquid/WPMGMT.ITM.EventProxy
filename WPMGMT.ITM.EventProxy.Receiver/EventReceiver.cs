using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WPMGMT.ITM.EventProxy.Messages;
using EasyNetQ;
using NetMQ;
using NetMQ.Sockets;
using NLog;
using Topshelf;

namespace WPMGMT.ITM.EventProxy.Receiver
{
    class EventReceiver
    {
        public EventReceiver()
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
            try
            {
                using (IBus bus = RabbitHutch.CreateBus(ConfigurationManager.AppSettings["MQ"].ToString()))
                {
                    using (NetMQContext context = NetMQContext.Create())
                    {
                        using (RouterSocket routerSocket = context.CreateRouterSocket())
                        {
                            // Bind to port 5599, as configured in Tivoli EIF destination
                            routerSocket.Options.RouterRawSocket = true;
                            routerSocket.Bind("tcp://0.0.0.0:5599");

                            while (this.Running)
                            {
                                bool hasMore = false;

                                // Every 0MQ message contains a Message ID. We don't need it, but we do need to read it
                                byte[] id = routerSocket.Receive();
                                // This is the actual message
                                byte[] message = routerSocket.Receive(out hasMore);

                                while (hasMore)
                                {
                                    byte[] messageMore = routerSocket.Receive(out hasMore);
                                    byte[] messageAll = new byte[message.Length + messageMore.Length];
                                    message.CopyTo(messageAll, 0);
                                    messageMore.CopyTo(messageAll, message.Length);
                                    message = messageAll;
                                }

                                // Convert to a readable ASCII string
                                string scrambled = Encoding.ASCII.GetString(message);
                                // Discard all Non-ASCII characters
                                string unscrambled = Regex.Replace(scrambled, "[^ -~]", String.Empty);

                                // Split the messages by <START>>
                                foreach (string eventMessage in unscrambled.Split(new string[] { "<START>>" }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    // Split those messages by each ';'
                                    string[] lines = eventMessage.Split(';');
                                    Dictionary<string, string> eventMessageSections = new Dictionary<string, string>();

                                    // Loop over all lines
                                    foreach (string line in lines)
                                    {
                                        // Split them once more, so we can retrieve the keys/values
                                        string[] sections = line.Split(new char[] { '=' }, 2);
                                        if (sections.Length > 1)
                                        {
                                            eventMessageSections.Add(sections[0], sections[1].Trim(new char[] { '\'' }));
                                        }
                                        else
                                        {
                                            // We're not intereting in any stupid headers etc.
                                            Console.WriteLine("Throwing this junk away :: {0}", sections[0]);
                                        }
                                    }

                                    if (eventMessageSections.Count > 0)
                                    {
                                        // Compose the message
                                        TivoliMessage tivoliMessage = new TivoliMessage();

                                        // Sorry, this is a hard-coded mapping :(
                                        tivoliMessage.HostName = eventMessageSections["hostname"];
                                        tivoliMessage.HostIP = eventMessageSections["origin"];
                                        tivoliMessage.IntegrationType = eventMessageSections["integration_type"];
                                        tivoliMessage.SituationName = eventMessageSections["situation_name"].ToUpper();
                                        tivoliMessage.SituationStatus = eventMessageSections["situation_status"].ToUpper();
                                        tivoliMessage.SituationType = eventMessageSections["situation_type"];
                                        tivoliMessage.SituationDisplayItem = eventMessageSections["situation_displayitem"];
                                        tivoliMessage.Severity = eventMessageSections["severity"];
                                        tivoliMessage.TimeStamp = DateTime.Parse(eventMessageSections["situation_time"], new CultureInfo("en-US", false));

                                        // Publish that shit
                                        bus.Publish(tivoliMessage);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.Logger.ErrorException(e.Message, e);
            }
        }
    }
}
