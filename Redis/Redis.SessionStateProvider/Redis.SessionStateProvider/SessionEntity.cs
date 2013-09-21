using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis.SessionStateProvider
{
	[Serializable]
	public class SessionEntity
	{
		public SessionEntity()
		{
			SessionItems = new Dictionary<string, object>();
		}

		public DateTime Created { get; set; }
		public DateTime Expires { get; set; }
		public int Flags { get; set; }
		public int LockCookie { get; set; }
		public DateTime LockDate { get; set; }
		public bool Locked { get; set; }
		public string SessionId { get; set; }
		public Dictionary<string,object> SessionItems { get; set; }
		public int Timeout { get; set; }
	}
}
