using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using WPMGMT.ITM.EventProxy.Messages;
using Common.Logging;
using Common.Logging.NLog;
using NLog;
using Topshelf;

namespace WPMGMT.ITM.EventProxy.Receiver
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("Starting WPMGMT.ITM.EventProxy.Receiver");

            Common.Logging.LogManager.Adapter = new Common.Logging.NLog.NLogLoggerFactoryAdapter(new Common.Logging.Configuration.NameValueCollection());

            HostFactory.Run(x =>
            {
                x.Service<EventReceiver>(s =>
                {
                    s.ConstructUsing(name => new EventReceiver());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAs(@"MSNET\EXF284", "frederik05");

                x.StartAutomatically();

                x.SetDescription("WPMGMT.ITM.EventProxy.Receiver is a proxy application that serves as a bridge between TEM EIF events and a RabbitMQ server");
                x.SetDisplayName("WPMGMT.ITM.EventProxy.Receiver");
                x.SetServiceName("WPMGMT.ITM.EventProxy.Receiver");
            });
        }
    }
}
