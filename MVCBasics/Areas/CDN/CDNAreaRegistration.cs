using System.Web.Mvc;

namespace MVCBasics.Areas.CDN
{
	public class CDNAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "CDN";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
				"CDN_default",
				"CDN/{controller}/{action}/{id}",
				new { action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}
