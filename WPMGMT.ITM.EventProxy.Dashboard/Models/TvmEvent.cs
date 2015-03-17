using System;

namespace WPMGMT.ITM.EventProxy.Dashboard.Models
{
    public class TvmEvent
    {
	    public string HostName              { get; set; }
	    public string SituationName         { get; set; }
	    public string SituationStatus       { get; set; }
	    public string SituationDisplayItem  { get; set; }
	    public string Severity              { get; set; }
	    public string TvmName               { get; set; }
	    public string Location              { get; set; }
	    public double Latitude              { get; set; }
	    public double Longitude             { get; set; }
        public DateTime TimeStamp           { get; set; }
    }
}