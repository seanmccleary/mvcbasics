using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCBasics.Areas.ExternalAuthentication.Models;

namespace MVCBasics.Areas.ExternalAuthentication.Controllers
{
	/// <summary>
	/// A base controler you can use for logging users in via an external authenticator, such as Facebook (OAuth), 
	/// Twitter (OAuth), or any number of OpenID providers (like Google or Yahoo).
	/// </summary>
    public abstract class AccountController : Controller
    {
		/// <summary>
		/// Our service layer!
		/// </summary>
		private IExternalLoginService _captService;

		/// <summary>
		/// What do we do when a user has been identified and needs to be logged in?
		/// THAT'S UP TO YOU, DEAR IMPLEMENTER!!! 
		/// </summary>
		/// <param name="userId">The ID of the user who's been authenticated</param>
		/// <param name="provider">The external authenticator</param>
		/// <param name="oauthToken">The OAuth token, if one was created and in case you'd like to save it</param>
		protected abstract void LogUserIn(string userId, ExternalLoginProvider provider, OAuthToken oauthToken);

		/// <summary>
		/// Default constructor which will choose its favorite service layer
		/// </summary>
		public AccountController()
			: this(new ExternalLoginService())
		{
		}

		/// <summary>
		/// Constructor that lets you specify your own service layer
		/// </summary>
		/// <param name="_captService">The service layer object you'd like to use</param>
		public AccountController(IExternalLoginService loginService)
		{
			_captService = loginService;
		}

		/// <summary>
		/// The login form submits to this action.
		/// </summary>
		/// <param name="returnUrl">The URL the user should be sent back to once he's logged in</param>
		/// <returns></returns>
		[ValidateInput(false)]
		public ActionResult Authenticate(string returnUrl)
		{
			try
			{
				string receiveUrl = Url.Action("ReceiveResponse", "Account", null, Request.Url.Scheme);

				// Are we doing OpenID here?
				if (Request.Form["provider_type"] == "openid")
				{
					return Redirect(_captService.GetOpenIdRedirectUrl(Request.Form["openid_identifier"], receiveUrl, returnUrl));
				}

				// How's about Facebook?
				else if (Request.Form["provider_type"] == "fb")
				{
					return Redirect(_captService.GetFacebookRedirectUrl(
						Url.Action("ReceiveFacebookResponse", "Account", null, Request.Url.Scheme)
						, returnUrl));
				}

				// Twitter, perhaps?
				else if (Request.Form["provider_type"] == "twitter")
				{
					string consumerKey = System.Configuration.ConfigurationManager.AppSettings["TwitterConsumerKey"];
					string consumerSecret = System.Configuration.ConfigurationManager.AppSettings["TwitterConsumerSecret"];

					return Redirect(
						_captService.GetTwitterRedirectUrl(receiveUrl, returnUrl, consumerKey, consumerSecret)
					);
				}

				// That's odd.  How'd we get here?
				throw new ApplicationException("Couldn't figure out what kind of login we're doing here!");

			}
			catch (Exception ex)
			{
				ModelState.AddModelError("Message", ex.Message);
				return View("LogOn");
			}
		}

		/// <summary>
		/// OK, Facebook doesn't wanna PLAY BALL like the rest of the providers,
		/// so we need to parse the "state" from them specialy.
		/// I could do this with an action filter on the ReceiveReponse method instead, but then it'd
		/// be executed each time, even when the user's not logging in via Facebook, and I just don't know
		/// which would irriate me more.
		/// </summary>
		/// <param name="state">The "state" that we sent to Facebook</param>
		/// <returns></returns>
		public ActionResult ReceiveFacebookResponse(string state, string code)
		{
			System.Collections.Specialized.NameValueCollection nvc =
				System.Web.HttpUtility.ParseQueryString(state);

			return ReceiveResponse(nvc["returnUrl"], 
				(ExternalLoginProvider) Enum.Parse(typeof(ExternalLoginProvider), nvc["provider"])
			);
		}

		/// <summary>
		/// Process the user's return from the external authenticator and redirec tthem appropriately
		/// </summary>
		/// <param name="returnUrl">The URL to which we should return the user at the end</param>
		/// <param name="provider">The ID of the external provider (Facebook, Twitter, etc.)</param>
		/// <returns></returns>
		public ActionResult ReceiveResponse(string returnUrl, ExternalLoginProvider provider)
		{
			try
			{
				MVCBasics.Areas.ExternalAuthentication.Models.OAuthToken oauthToken = null;
				string userId;


				switch (provider)
				{
					case ExternalLoginProvider.GenericOpenId:

						userId = _captService.GetOpenIdIdentifier(System.Web.HttpContext.Current.Request);
						break;

					case ExternalLoginProvider.Facebook:

						userId = _captService.GetFacebookId(
							System.Web.HttpContext.Current.Request,
							Url.Action("ReceiveFacebookResponse", "Account", null, Request.Url.Scheme),
							out oauthToken);
						break;

					case ExternalLoginProvider.Twitter:

						string consumerKey = System.Configuration.ConfigurationManager.AppSettings["TwitterConsumerKey"];
						string consumerSecret = System.Configuration.ConfigurationManager.AppSettings["TwitterConsumerSecret"];

						userId = _captService.GetTwitterId(
							System.Web.HttpContext.Current.Request,
							consumerKey, consumerSecret, out oauthToken);

						break;

					default:

						// Crud.  Couldn't figure out who's sending us back?
						throw new ApplicationException("Couldn't figure out what kind of login we're doing here!");
				}

				// Did we pick up an OAuth token in the process of logging our pal in?

				LogUserIn(userId, provider, oauthToken);


				// So now then, where was it the user wanted to go?
				if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
				{
					return Redirect(returnUrl);
				}
				else
				{
					return RedirectToAction("Index", "PictureCaptions");
				}

			}
			catch (Exception e)
			{
				ModelState.AddModelError("Message", e.Message);
				return View("LogOn");
			}
		}

		/// <summary>
		/// This unexciting action pretty much just displays the login page.
		/// </summary>
		/// <returns></returns>
		public ActionResult LogOn()
		{
			if (Request.IsAuthenticated)
			{
				// User's already logged in?
				return Redirect("/");
			}

			return View();
		}
    }
}
