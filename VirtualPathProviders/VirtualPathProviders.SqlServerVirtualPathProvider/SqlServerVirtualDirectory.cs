using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Hosting;

namespace VirtualPathProviders.SqlServerVirtualPathProvider
{
	public class SqlServerVirtualDirectory : VirtualDirectory
	{
		private string _virtualPath;

		public SqlServerVirtualDirectory(string virtualPath) : base(virtualPath)
		{
			_virtualPath = virtualPath;
		}
		
		public override IEnumerable Directories
		{
			get { throw new NotImplementedException(); }
		}

		public override IEnumerable Files
		{
			get { throw new NotImplementedException(); }
		}

		public override IEnumerable Children
		{
			get { throw new NotImplementedException(); }
		}
	}
}
