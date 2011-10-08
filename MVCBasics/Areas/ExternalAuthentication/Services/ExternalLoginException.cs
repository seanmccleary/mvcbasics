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

namespace MVCBasics.Areas.ExternalAuthentication.Services
{
	public class ExternalLoginException : Exception
	{
		public ExternalLoginException(string message) 
			: base(message)
		{
		}

		public ExternalLoginException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}