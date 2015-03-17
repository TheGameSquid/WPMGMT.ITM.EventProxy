using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using WPMGMT.ITM.EventProxy.Dashboard.Models;
using Dapper;

namespace WPMGMT.ITM.EventProxy.Dashboard.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            List<TvmEvent> events = new List<TvmEvent>();

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DB"].ToString()))
            {
                events.AddRange(connection.Query<TvmEvent>("SELECT E.HostName, E.SituationName, E.SituationStatus, E.SituationDisplayItem, E.Severity, E.TimeStamp, TM.TvmName, TM.Location, TM.Latitude, TM.Longitude FROM [TEM].[EVENTS] E JOIN [TEM].[TVM_META] TM ON TM.HostName = E.HostName"));
            }
            
            return View(events);
        }
    }
}