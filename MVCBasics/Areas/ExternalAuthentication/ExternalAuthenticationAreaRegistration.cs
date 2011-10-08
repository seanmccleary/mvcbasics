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
