using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenDbProviders.RoleProvider
{
	internal class Role
	{
		public Role()
		{
			Members = new List<string>();
		}

		public string Id { get; set; }
		public string RoleName { get; set; }
		public IList<string> Members { get; set; }
	}
}
