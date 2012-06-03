/*
 * Copyright 2011 Sean McCleary
 * 
 * This file is part of MVCBasics.
 *
 * MVCBasics is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * MVCBasics is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with MVCBasics.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCBasics.Areas.ExternalAuthentication.Services;
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
		private IExternalLoginService _externalLoginService;

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
		/// <param name="_externalLoginService">The external login service you'd like to use</param>
		public AccountController(IExternalLoginService loginService)
		{
			_externalLoginService = loginService;
		}

		/// <summary>
		/// The login form submits to this action.
		/// </summary>
		/// <param name="returnUrl">The URL the user should be sent back to once he's logged in</param>
		/// <returns></returns>
		[ValidateInput(false)]
		public virtual ActionResult Authenticate(string returnUrl)
		{
			try
			{
				string receiveUrl = Url.Action("ReceiveResponse", "Account", null, Request.Url.Scheme);

				// Are we doing OpenID here?
				if (Request.Form["provider_type"] == "openid")
				{
					string realm = System.Configuration.ConfigurationManager.AppSettings["OpenIdRealm"];

					return Redirect(_externalLoginService.GetOpenIdRedirectUrl(Request.Form["openid_identifier"], receiveUrl, returnUrl, 
						realm));
				}

				// How's about Facebook?
				else if (Request.Form["provider_type"] == "fb")
				{
					string appId = System.Configuration.ConfigurationManager.AppSettings["FacebookAppId"];
					string appSecret = System.Configuration.ConfigurationManager.AppSettings["FacebookAppSecret"];

					return Redirect(
						_externalLoginService.GetFacebookRedirectUrl(
							Url.Action("ReceiveFacebookResponse", "Account", null, Request.Url.Scheme),
							returnUrl,
							appId,
							appSecret));
				}

				// Twitter, perhaps?
				else if (Request.Form["provider_type"] == "twitter")
				{
					string consumerKey = System.Configuration.ConfigurationManager.AppSettings["TwitterConsumerKey"];
					string consumerSecret = System.Configuration.ConfigurationManager.AppSettings["TwitterConsumerSecret"];

					return Redirect(
						_externalLoginService.GetTwitterRedirectUrl(receiveUrl, returnUrl, consumerKey, consumerSecret)
					);
				}

				// That's odd.  How'd we get here?
				throw new ExternalLoginException("Couldn't figure out what kind of login we're doing here!");

			}
			catch (ExternalLoginException ex)
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
		public virtual ActionResult ReceiveFacebookResponse(string state, string code)
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
		public virtual ActionResult ReceiveResponse(string returnUrl, ExternalLoginProvider provider)
		{
			try
			{
				MVCBasics.Areas.ExternalAuthentication.Models.OAuthToken oauthToken = null;
				string userId;


				switch (provider)
				{
					case ExternalLoginProvider.GenericOpenId:

						userId = _externalLoginService.GetOpenIdIdentifier(System.Web.HttpContext.Current.Request);
						break;

					case ExternalLoginProvider.Facebook:

						string appId = System.Configuration.ConfigurationManager.AppSettings["FacebookAppId"];
						string appSecret = System.Configuration.ConfigurationManager.AppSettings["FacebookAppSecret"];

						userId = _externalLoginService.GetFacebookId(
							System.Web.HttpContext.Current.Request,
							Url.Action("ReceiveFacebookResponse", "Account", null, Request.Url.Scheme),
							appId, appSecret,
							out oauthToken);
						break;

					case ExternalLoginProvider.Twitter:

						string consumerKey = System.Configuration.ConfigurationManager.AppSettings["TwitterConsumerKey"];
						string consumerSecret = System.Configuration.ConfigurationManager.AppSettings["TwitterConsumerSecret"];

						userId = _externalLoginService.GetTwitterId(
							System.Web.HttpContext.Current.Request,
							consumerKey, consumerSecret, out oauthToken);

						break;

					default:

						// Crud.  Couldn't figure out who's sending us back?
						throw new ExternalLoginException("Couldn't figure out what kind of login we're doing here!");
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
					return Redirect("/");
				}

			}
			catch (ExternalLoginException e)
			{
				ModelState.AddModelError("Message", e.Message);
				return View("LogOn");
			}
		}
    }
}
