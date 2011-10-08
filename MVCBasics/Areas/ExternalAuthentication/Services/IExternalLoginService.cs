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
using MVCBasics.Areas.ExternalAuthentication.Models;

namespace MVCBasics.Areas.ExternalAuthentication.Services
{
	public interface IExternalLoginService
	{

		/// <summary>
		/// Get the URL to send a user to, to do his open ID login.
		/// TODO: Nix that "System.Uri" parameter and get all needed info
		/// from the returnUrl param.
		/// </summary>
		/// <param name="identifier">His OpenID identifier</param>
		/// <param name="request">The Request object used for this web request</param>
		/// <param name="returnUrl">The URL the user should be redirected back to</param>
		/// <param name="realmUrl">The OpenID realm. For www.yoursite.com you may want to set it to "http://*.yoursite.com"</param>
		/// <returns>The URL he needs to visit to log in</returns>
		string GetOpenIdRedirectUrl(string identifier, string receiveUrl, string returnUrl, string realmUrl);

		/// <summary>
		/// Get the user's OpenID identifier from the response back from the external authenticating
		/// server
		/// </summary>
		/// <param name="request">The HTTP request that we received from the remote external authenticator</param>
		/// <returns>The user's Open ID identifier</returns>
		string GetOpenIdIdentifier(System.Web.HttpRequest request);

		/// <summary>
		/// Send the user off to Facebook to log in there.
		/// </summary>
		/// <param name="receiveUrl">The URL facebook should send the user back to, to process the response</param>
		/// <param name="returnUrl">The URL we should return the user to once we're through logging him in</param>
		/// <param name="appId">The Facebook application ID</param>
		/// <param name="appSecret">The Facebook application secret</param>
		/// <returns></returns>
		string GetFacebookRedirectUrl(string receiveUrl, string returnUrl,
			 string appId, string appSecret);

		/// <summary>
		/// Get the Facebook ID of the user from the request we got when ol' Facey sent the
		/// user back to us.
		/// </summary>
		/// <param name="request">The HTTP request from Facebook. Just pass it in OK?</param>
		/// <param name="receiveUrl">The URL To which Facebook sent the user back.</param>
		/// <param name="appId">The Facebook application ID</param>
		/// <param name="appSecret">The Facebook application secret</param>
		/// <param name="oauthToken">An OAuthToken object that will be populated</param>
		/// <returns>The user's Facebook ID.</returns>
		string GetFacebookId(System.Web.HttpRequest request, string receiveUrl,
			string appId, string appSecret,
			out OAuthToken oauthToken);

		/// <summary>
		/// Send the user off to the Twitter so's they can log him in, log him right in, yessir.
		/// </summary>
		/// <param name="receiveUrl">The URL Twitter should send the user back to, to process the response</param>
		/// <param name="returnUrl">The URL we should return the user to once we're through logging him in</param>
		/// <returns></returns>
		string GetTwitterRedirectUrl(string receiveUrl, string returnUrl,
			string consumerKey, string consumerSecret);

		/// <summary>
		/// Receive the user when Twitter sends him back, and get his Twitter ID
		/// </summary>
		/// <param name="request">The HTTP request from Twitter.</param>
		/// <param name="consumerKey">The Twitter consumer key</param>
		/// <param name="consumerSecret">The Twitter consumer secret</param>
		/// <param name="oauthToken">An OAuthToken object that will be populated</param>
		/// <returns></returns>
		string GetTwitterId(System.Web.HttpRequest request, string consumerKey, string consumerSecret,
			out OAuthToken oauthToken);
	}
}
