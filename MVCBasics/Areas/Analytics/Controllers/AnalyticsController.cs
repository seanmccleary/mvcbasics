using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCBasics.Areas.Analytics.Controllers
{
    public class AnalyticsController : Controller
    {
        //
        // GET: /Analytics/Analytics/
		[ChildActionOnly]
        public ActionResult Include()
        {
			ViewBag.GoogleAnalyticsId = System.Configuration.ConfigurationManager.AppSettings["GoogleAnalyticsId"];
			ViewBag.GoogleSiteVerification = System.Configuration.ConfigurationManager.AppSettings["GoogleSiteVerification"];
			ViewBag.YahooSiteVerification = System.Configuration.ConfigurationManager.AppSettings["YahooSiteVerification"];
			ViewBag.BingSiteVerification = System.Configuration.ConfigurationManager.AppSettings["BingSiteVerification"];

            return View();
        }
    }
}
