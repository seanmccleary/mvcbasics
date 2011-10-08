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
using System.Text.RegularExpressions;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.RelyingParty;
using Facebook;
using System.Collections.Generic;
using Twitterizer;
using MVCBasics.Areas.ExternalAuthentication.Models;

namespace MVCBasics.Areas.ExternalAuthentication.Services
{

	/// <see cref="MVCBasics.Areas.ExternalAuthentication.Services.IExternalLoginService"/>
	public class ExternalLoginService : IExternalLoginService
	{

		/// <see cref="MVCBasics.Areas.ExternalAuthentication.Services.IExternalLoginService.GetOpenIdRedirectUrl" />
		public string GetOpenIdRedirectUrl(string identifier, string receiveUrl, string returnUrl, string realmUrl)
		{

			OpenIdRelyingParty openid = new OpenIdRelyingParty();
			Identifier id;
			Identifier.TryParse(identifier, out id);

			// This is the URL that the Open ID server should send the user back to
			// NOT the one that WE will eventually redirect the user back to.
			Uri sendBackUri = new Uri(
				receiveUrl
				+ "?returnUrl=" + returnUrl
				+ "&provider=" + ExternalLoginProvider.GenericOpenId);

			if (String.IsNullOrWhiteSpace(realmUrl))
			{
				realmUrl =
					sendBackUri.Scheme + "://"
					+ sendBackUri.Host
					+ (sendBackUri.IsDefaultPort ? "" : ":" + sendBackUri.Port);
			}

			Realm realm = new Realm(realmUrl);
			var openidRequest = openid.CreateRequest(identifier, realm, sendBackUri);

			return openidRequest.RedirectingResponse.Headers["Location"];
		}

		/// <see cref="MVCBasics.Areas.ExternalAuthentication.Services.IExternalLoginService.GetOpenIdIdentifier"/>
		public string GetOpenIdIdentifier(System.Web.HttpRequest request)
		{
			OpenIdRelyingParty openid = new OpenIdRelyingParty();

			IAuthenticationResponse response;

			if ((response = openid.GetResponse(new HttpRequestInfo(request))) == null)
			{
				throw new ExternalLoginException("No OpenID response");
			}

			switch (response.Status)
			{
				case AuthenticationStatus.Authenticated:
					return response.ClaimedIdentifier;

				case AuthenticationStatus.Canceled:
					throw new ExternalLoginException("Canceled at provider");

				case AuthenticationStatus.Failed:
					throw new ExternalLoginException("There was a problem logging in.", response.Exception);

				default:
					throw new ExternalLoginException("There was a problem logging in.");
			}

		}

		/// <see cref="MVCBasics.Areas.ExternalAuthentication.Services.IExternalLoginService.GetFacebookRedirectUrl"/>
		public string GetFacebookRedirectUrl(string receiveUrl, string returnUrl,
			string appId, string appSecret)
		{
			FacebookApplication app = new FacebookApplication();
			FacebookOAuthClient FBClient = new FacebookOAuthClient(FacebookApplication.Current);

			FBClient.AppId = appId;
			FBClient.AppSecret = appSecret;

			string state =
				"returnUrl=" + returnUrl
				+ "&provider=" + ExternalLoginProvider.Facebook;

			FBClient.RedirectUri = new Uri(receiveUrl);
			var loginUri = FBClient.GetLoginUrl(new Dictionary<string, object> { { "state", state } });

			return loginUri.AbsoluteUri;

		}

		/// <see cref="MVCBasics.Areas.ExternalAuthentication.Services.IExternalLoginService.GetFacebookId"/>
		public string GetFacebookId(System.Web.HttpRequest request, string receiveUrl, string appId, string appSecret,
			out OAuthToken oauthToken)
		{
			FacebookOAuthResult oauthResult;
			if (!FacebookOAuthResult.TryParse(request.Url, out oauthResult))
			{
				throw new ExternalLoginException("There was a problem logging in through Facebook!");
			}

			if (!oauthResult.IsSuccess)
			{
				throw new ExternalLoginException(oauthResult.ErrorDescription);
			}

			var oAuthClient = new FacebookOAuthClient(FacebookApplication.Current);
			oAuthClient.AppId = appId;
			oAuthClient.AppSecret = appSecret;

			oAuthClient.RedirectUri = new Uri(receiveUrl);
			dynamic tokenResult = oAuthClient.ExchangeCodeForAccessToken(request["code"]);
			string accessToken = tokenResult.access_token;

			DateTime expiresOn = DateTime.MaxValue;

			if (tokenResult.ContainsKey("expires"))
			{
				DateTimeConvertor.FromUnixTime(tokenResult.expires);
			}

			FacebookClient fbClient = new FacebookClient(accessToken);

			dynamic me = fbClient.Get("me?fields=id");

			oauthToken = new OAuthToken
			{
				Token = tokenResult.access_token,
				Expires = expiresOn,
				IsSession = true
			};

			return me.id;
		}

		/// <see cref="MVCBasics.Areas.ExternalAuthentication.Services.IExternalLoginService.GetTwitterRedirectUrl"/>
		public string GetTwitterRedirectUrl(string receiveUrl, string returnUrl,
			string consumerKey, string consumerSecret)
		{
			receiveUrl +=
				"?returnUrl=" + returnUrl
				+ "&provider=" + ExternalLoginProvider.Twitter;

			var requestToken = OAuthUtility.GetRequestToken(consumerKey, consumerSecret, receiveUrl);

			return "http://twitter.com/oauth/authenticate?oauth_token=" + requestToken.Token;
		}

		/// <see cref="MVCBasics.Areas.ExternalAuthentication.Services.IExternalLoginService.GetTwitterId"/>
		public string GetTwitterId(System.Web.HttpRequest request, string consumerKey, string consumerSecret,
			out OAuthToken oauthToken)
		{

			if (!String.IsNullOrWhiteSpace(request["denied"]))
			{
				throw new ExternalLoginException("Couldn't log you in via Twitter. (Did you deny access?)");
			}

			OAuthTokenResponse tokens = OAuthUtility.GetAccessToken(consumerKey, consumerSecret,
				request["oauth_token"], request["oauth_verifier"]);

			string userId = Convert.ToString(tokens.UserId);

			oauthToken = new OAuthToken
			{
				Expires = DateTime.MaxValue,
				Secret = tokens.TokenSecret,
				Token = tokens.Token,
				IsSession = false
			};

			return userId;
		}
	}
}