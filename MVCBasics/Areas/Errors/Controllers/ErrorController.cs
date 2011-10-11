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
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;


namespace MVCBasics.Areas.Errors.Controllers
{
	/// <summary>
	/// Controller actions for various errors that could occur
	/// </summary>
	public class ErrorController : Controller
	{
		/// <summary>
		/// Action for the "default" error.  This is the one that shows the "Oops we goofed up" page and send
		/// an email with error details to the address specified in the config file.
		/// </summary>
		/// <param name="exception">The exception that was thrown to cause this error</param>
		/// <returns></returns>
		public ActionResult Error(Exception exception)
		{
			try
			{
				// Let's try and send us an email with the details of the error, shall we?
				string smtpServer = System.Configuration.ConfigurationManager.AppSettings["SmtpServer"];
				if (smtpServer != null)
				{
					SmtpClient client = new SmtpClient(smtpServer);

					// Do we need to use a special port to send mail?
					if (System.Configuration.ConfigurationManager.AppSettings["SmtpPort"] != null)
					{
						client.Port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["SmtpPort"]);
					}

					// How's about an SSL connection?
					if (
						System.Configuration.ConfigurationManager.AppSettings["SmtpSsl"] != null
						&& System.Configuration.ConfigurationManager.AppSettings["SmtpSsl"].ToUpper().Trim() == "TRUE"
						)
					{
						client.EnableSsl = true;
					}

					// A username and password?
					if (
						System.Configuration.ConfigurationManager.AppSettings["SmtpLogin"] != null
						&& System.Configuration.ConfigurationManager.AppSettings["SmtpPassword"] != null
						)
					{
						System.Net.NetworkCredential nc = new System.Net.NetworkCredential(
							System.Configuration.ConfigurationManager.AppSettings["SmtpLogin"],
							System.Configuration.ConfigurationManager.AppSettings["SmtpPassword"]
						);

						client.Credentials = nc;
					}

					// Now build the email
					var message = new MailMessage();

					message.To.Add(new MailAddress(
						System.Configuration.ConfigurationManager.AppSettings["ErrorEmailsTo"]
					));

					message.From = new MailAddress(
						System.Configuration.ConfigurationManager.AppSettings["EmailsFrom"]
					);
					message.Subject = "Error!";

					message.IsBodyHtml = false;
					message.Body =
						"URL: " + Request.Url + "\n"
						+ "IP Address: " + Request.UserHostAddress + "\n"
						+ "Host: " + Request.UserHostAddress + "\n";

					if (exception != null)
					{
						message.Subject += " " + exception.GetType().ToString();
						message.Body += "\n\n" + exception.Message + "\n\n" + exception.StackTrace;
					}

					// This is necessary for mono but not windows, oddly.
					ServicePointManager.ServerCertificateValidationCallback =
						delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
						{ return true; };


					client.Send(message);
				}
			}
			catch
			{
				// well nuts.
			}

			return View();
		}

		/// <summary>
		/// Just show the 404 page.
		/// </summary>
		/// <returns></returns>
		public ActionResult Error404()
		{
			return View();
		}

	}
}
