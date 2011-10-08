using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCBasics.Areas.ExternalAuthentication.Models
{
	public class OAuthToken
	{
		public DateTime? Expires { get; set; }

		public string Token { get; set; }

		public string Secret { get; set; }

		public bool IsSession { get; set; }
	}
}