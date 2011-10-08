using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCBasics.Areas.Cdn.Controllers
{
    public class CdnController : Controller
    {
        //
        // GET: /CDN/CDN/
		[ChildActionOnly]
        public ActionResult Include()
        {
			ViewBag.UseMicrosoftCdn =
				System.Configuration.ConfigurationManager.AppSettings["UseMicrosoftCDN"] != null
				&& System.Configuration.ConfigurationManager.AppSettings["UseMicrosoftCDN"].ToUpper().Trim() == "TRUE";

            return View();
        }

    }
}
