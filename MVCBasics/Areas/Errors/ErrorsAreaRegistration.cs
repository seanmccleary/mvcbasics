using System.Web.Mvc;

namespace MVCBasics.Areas.Errors
{
	public class ErrorsAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "Errors";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
				"Errors_default",
				"Errors/{controller}/{action}/{id}",
				new { action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}
