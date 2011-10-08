using System.Web.Mvc;

namespace MVCBasics.Areas.ExternalAuthentication
{
	public class ExternalAuthenticationAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "ExternalAuthentication";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
				"ExternalAuthentication_default",
				"ExternalAuthentication/{controller}/{action}/{id}",
				new { action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}
