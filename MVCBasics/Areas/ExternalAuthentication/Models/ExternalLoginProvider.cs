using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCBasics.Areas.ExternalAuthentication.Models
{
	public enum ExternalLoginProvider
	{
		GenericOpenId = 1,
		Facebook,
		Twitter
	}
}