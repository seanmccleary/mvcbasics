/*
 * Copyright 2011 Sean McCleary
 * 
 * This file is part of capt.
 *
 * capt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * capt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with capt.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Text.RegularExpressions;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.RelyingParty;
using Facebook;
using System.Collections.Generic;
using Twitterizer;

namespace MVCBasics.Areas.ExternalAuthentication.Models
{
	/// <summary>
	/// TODO: Make some better exceptions to throw than ApplicationException
	/// </summary>
	public class ExternalLoginService : IExternalLoginService
	{

		/// <see cref="Capt.Models.IExternalLoginService.GetOpenIdRedirectUrl" />
		public string GetOpenIdRedirectUrl(string identifier, string receiveUrl, string returnUrl)
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

			string realmUrl =
				sendBackUri.Scheme + "://"
				+ sendBackUri.Host
				+ (sendBackUri.IsDefaultPort ? "" : ":" + sendBackUri.Port);

			Realm realm = new Realm(realmUrl);
			var openidRequest = openid.CreateRequest(identifier, realm, sendBackUri);

			return openidRequest.RedirectingResponse.Headers["Location"];
		}

		/// <see cref="Capt.Models.IExternalLoginService.GetOpenIdIdentifier"/>
		public string GetOpenIdIdentifier(System.Web.HttpRequest request)
		{
			OpenIdRelyingParty openid = new OpenIdRelyingParty();

			IAuthenticationResponse response;

			if ((response = openid.GetResponse(new HttpRequestInfo(request))) == null)
			{
				throw new ApplicationException("No OpenID response");
			}

			switch (response.Status)
			{
				case AuthenticationStatus.Authenticated:
					return response.ClaimedIdentifier;

				case AuthenticationStatus.Canceled:
					throw new ApplicationException("Canceled at provider");

				case AuthenticationStatus.Failed:
					throw response.Exception;

				default:
					throw new ApplicationException("There was a problem logging in.");
			}

		}

		/// <see cref="Capt.Models.IExternalLoginService.GetFacebookRedirectUrl"/>
		public string GetFacebookRedirectUrl(string receiveUrl, string returnUrl)
		{
			FacebookOAuthClient FBClient = new FacebookOAuthClient(FacebookApplication.Current);

			string state =
				"returnUrl=" + returnUrl
				+ "&provider=" + ExternalLoginProvider.Facebook;

			FBClient.RedirectUri = new Uri(receiveUrl);
			var loginUri = FBClient.GetLoginUrl(new Dictionary<string, object> { { "state", state } });

			return loginUri.AbsoluteUri;

		}

		/// <see cref="Capt.Models.IExternalLoginService.GetFacebookId"/>
		public string GetFacebookId(System.Web.HttpRequest request, string receiveUrl, out OAuthToken oauthToken)
		{
			FacebookOAuthResult oauthResult;
			if (!FacebookOAuthResult.TryParse(request.Url, out oauthResult))
			{
				throw new ApplicationException("There was a problem logging in through Facebook!");
			}

			if (!oauthResult.IsSuccess)
			{
				throw new ApplicationException(oauthResult.ErrorDescription);
			}

			var oAuthClient = new FacebookOAuthClient(FacebookApplication.Current);

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

		/// <see cref="Capt.Models.IExternalLoginService.GetTwitterRedirectUrl"/>
		public string GetTwitterRedirectUrl(string receiveUrl, string returnUrl,
			string consumerKey, string consumerSecret)
		{
			receiveUrl +=
				"?returnUrl=" + returnUrl
				+ "&provider=" + ExternalLoginProvider.Twitter;

			var requestToken = OAuthUtility.GetRequestToken(consumerKey, consumerSecret, receiveUrl);

			return "http://twitter.com/oauth/authenticate?oauth_token=" + requestToken.Token;
		}

		/// <see cref="Capt.Models.IExternalLoginService.GetTwitterId"/>
		public string GetTwitterId(System.Web.HttpRequest request, string consumerKey, string consumerSecret,
			out OAuthToken oauthToken)
		{

			if (!String.IsNullOrWhiteSpace(request["denied"]))
			{
				throw new ApplicationException("Couldn't log you in via Twitter. (Did you deny access?)");
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