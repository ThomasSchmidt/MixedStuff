using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace RavenDbProviders.MembershipProvider
{
	internal static class MembershipExtensions
	{
		public static User ToUser(this MembershipUser membershipUser)
		{
			return new User
			{
				UserName = membershipUser.UserName,
				Email = membershipUser.Email,
				PasswordQuestion = membershipUser.PasswordQuestion,
				PasswordAnswer = "",
				IsApproved = membershipUser.IsApproved,
				IsLockedOut = membershipUser.IsLockedOut,
				Comment = membershipUser.Comment,
				CreationDate = membershipUser.CreationDate,
				IsOnline = membershipUser.IsOnline,
				LastActivityDate = membershipUser.LastActivityDate,
				LastLockoutDate = membershipUser.LastLockoutDate,
				LastLoginDate = membershipUser.LastLoginDate,
				LastPasswordChangedDate = membershipUser.LastPasswordChangedDate,
				ProviderName = membershipUser.ProviderName,
				ProviderUserKey = membershipUser.ProviderUserKey
			};
		}

		public static MembershipUser ToMembershipUser(this User user)
		{
			return new MembershipUser(RavenDbMembershipProvider.PROVIDER_NAME, user.UserName, user.ProviderUserKey, user.Email, user.PasswordQuestion, user.Comment, user.IsApproved, user.IsLockedOut, user.CreationDate, user.LastLoginDate, user.LastActivityDate, user.LastPasswordChangedDate, user.LastLockoutDate);
		}
	}
}
