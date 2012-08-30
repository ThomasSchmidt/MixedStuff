using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualPathProviders.SqlServerVirtualPathProvider
{
	internal class SqlServerVirtualPathEntry
	{
		public Guid Id { get; set; }
		public string VirtualPath { get; set; }
		public byte[] RawContent { get; set; }
		public string MimeType { get; set; }
		public bool IsDirectory { get; set; }
	}
}
