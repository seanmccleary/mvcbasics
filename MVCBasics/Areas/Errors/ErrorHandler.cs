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
using System.Web.Routing;

namespace MVCBasics.Areas.Errors
{
	public class ErrorHandler
	{
		public void Handle()
		{
			var exception = System.Web.HttpContext.Current.Server.GetLastError();

			if (exception is System.Threading.ThreadAbortException)
			{
				// Sigh... ignore these.
				return;
			}

			var httpException = exception as HttpException;
			System.Web.HttpContext.Current.Response.Clear();
			System.Web.HttpContext.Current.Server.ClearError();
			var routeData = new RouteData();
			routeData.Values["controller"] = "Error";
			routeData.Values["action"] = "Error";
			routeData.Values["exception"] = exception;
			routeData.DataTokens["Area"] = "Errors";

			System.Web.HttpContext.Current.Response.StatusCode = 500;

			if (httpException != null)
			{
				System.Web.HttpContext.Current.Response.StatusCode = httpException.GetHttpCode();
				switch (System.Web.HttpContext.Current.Response.StatusCode)
				{
					case 404:
						routeData.Values["action"] = "Error404";
						break;
				}
			}

			IController errorsController = new MVCBasics.Areas.Errors.Controllers.ErrorController();
			var rc = new RequestContext(new HttpContextWrapper(System.Web.HttpContext.Current), routeData);
			errorsController.Execute(rc);
		}
	}
}