using System;

namespace RavenDbProviders.MembershipProvider
{
	internal class User
	{
		public byte[] PasswordHash { get; set; }
		public byte[] PasswordSalt { get; set; }
	}
}
