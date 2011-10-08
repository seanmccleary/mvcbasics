using System.Web.Mvc;

namespace MVCBasics.Areas.Cdn
{
	public class CdnAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "Cdn";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
				"Cdn_default",
				"Cdn/{controller}/{action}/{id}",
				new { action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}
