using System;
using System.Net;

namespace WPMGMT.ITM.EventProxy.Messages
{
    public class TivoliMessage
    {
        public string HostName              { get; set; }
        public string HostIP                { get; set; }
        public string IntegrationType       { get; set; }
        public string SituationName         { get; set; }
        public string SituationType         { get; set; }
        public string SituationStatus       { get; set; }
        public string SituationDisplayItem  { get; set; }
        public string Severity              { get; set; }
        public DateTime TimeStamp           { get; set; }
    }
}
