using System;

namespace RavenDbProviders.MembershipProvider
{
	internal class User
	{
		public string Id { get; set; }
		public byte[] Password { get; set; }
		public byte[] Salt { get; set; }
		public string Comment { get; set; }
		public DateTime CreationDate { get; set; }
		public string Email { get; set; }
		public bool IsApproved { get; set; }
		public bool IsLockedOut { get; set; }
		public DateTime LastActivityDate { get; set; }
		public DateTime LastLockoutDate { get; set; }
		public DateTime LastLoginDate { get; set; }
		public DateTime LastPasswordChangedDate { get; set; }
		public string PasswordQuestion { get; set; }
		public byte[] PasswordAnswer { get; set; }
		public string ProviderName { get; set; }
		public object ProviderUserKey { get; set; }
		public string Username { get; set; }
	}
}
