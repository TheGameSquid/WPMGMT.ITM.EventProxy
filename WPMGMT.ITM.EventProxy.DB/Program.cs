using System;
using Common.Logging;
using NLog;
using Topshelf;

namespace WPMGMT.ITM.EventProxy.DB
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("Starting WPMGMT.ITM.EventProxy.DB");

            Common.Logging.LogManager.Adapter = new Common.Logging.NLog.NLogLoggerFactoryAdapter(new Common.Logging.Configuration.NameValueCollection());

            HostFactory.Run(x =>
            {
                x.Service<DBLogger>(s =>
                {
                    s.ConstructUsing(name => new DBLogger());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAs(@"MSNET\EXF284", "frederik05");

                x.StartAutomatically();

                x.SetDescription("WPMGMT.ITM.EventProxy.DB is a proxy application that serves as a bridge between a RabbitMQ server and a historical DB");
                x.SetDisplayName("WPMGMT.ITM.EventProxy.DB");
                x.SetServiceName("WPMGMT.ITM.EventProxy.DB");
            });
        }
    }
}
