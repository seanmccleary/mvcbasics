using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCBasics.Areas.CDN.Controllers
{
    public class CDNController : Controller
    {
        //
        // GET: /CDN/CDN/
		[ChildActionOnly]
        public ActionResult Include()
        {
			ViewBag.UseMicrosoftCDN =
				System.Configuration.ConfigurationManager.AppSettings["UseMicrosoftCDN"] != null
				&& System.Configuration.ConfigurationManager.AppSettings["UseMicrosoftCDN"].ToUpper().Trim() == "TRUE";

            return View();
        }

    }
}
