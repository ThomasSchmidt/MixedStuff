using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using NUnit.Framework;

namespace RavenDbProviders.RoleProvider.Tests
{
	[TestFixture]
    public class RavenDbRoleProviderTests
	{
		private const string ROLE_NAME = "testrole";
		private const string USERNAME = "roleUser";

		[SetUp]
		public void SetUp()
		{
			foreach (string role in Roles.GetAllRoles())
			{
				Roles.DeleteRole(role, false);
			}
		}

		[Test]
		public void CanTestIfIsUserInRole()
		{
			//arrange
			const string roleName = ROLE_NAME + "1";
			const string username = USERNAME + "1";
			Roles.CreateRole(roleName);
			Roles.AddUserToRole(username, roleName);

			//act
			bool actual = Roles.IsUserInRole(username, roleName);

			//assert
			Assert.IsTrue(actual);

			Roles.DeleteRole(roleName, false);
		}

		[Test]
		public void CanGetRolesForUser()
		{
			//arrange
			const string roleName = ROLE_NAME + "2";
			const string username = USERNAME + "2";
			Roles.CreateRole(roleName);
			Roles.AddUserToRole(username, roleName);

			//act
			string[] actual = Roles.GetRolesForUser(username);

			//assert
			Assert.That(actual.Length, Is.EqualTo(1));

			Roles.DeleteRole(roleName, false);
		}

		[Test]
		public void CanCreateRole()
		{
			//act
			const string roleName = ROLE_NAME + "3";
			const string username = USERNAME + "3";
			Roles.CreateRole(roleName);

			//assert
			Assert.IsTrue(Roles.RoleExists(roleName));

			Roles.DeleteRole(roleName);
		}

		[Test]
		public void CanDeleteRole()
		{
			//arrange
			const string roleName = ROLE_NAME + "4";
			Roles.CreateRole(roleName);

			//act
			Roles.DeleteRole(roleName);

			//assert
			string[] actual = Roles.GetAllRoles();
			Assert.IsFalse(actual.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase)));
		}

		[Test]
		public void CanCheckIfRoleExists()
		{
			//arrange
			const string roleName = ROLE_NAME + "5";
			Roles.CreateRole(roleName);

			//act
			bool actual = Roles.RoleExists(roleName);

			//assert
			Assert.IsTrue(actual);

			Roles.DeleteRole(roleName);
		}

		[Test]
		public void CanAddUserToRole()
		{
			//arrange
			const string roleName = ROLE_NAME + "6";
			const string username = USERNAME + "6";
			Roles.CreateRole(roleName);

			//act
			Roles.AddUserToRole(username, roleName);

			//assert
			string[] actual = Roles.GetRolesForUser(username);
			Assert.IsTrue(actual.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase)));

			Roles.DeleteRole(roleName, false);
		}

		[Test]
		public void CanAddUsersToRoles()
		{
			//arrange
			const string roleName = ROLE_NAME + "7";
			const string username = USERNAME + "7";
			Roles.CreateRole(roleName);
			Roles.CreateRole(roleName + "-2");
			string[] roles = new[]{roleName, roleName + "-2"};
			string[] usernames = new []{username, username + "-2"};

			//act
			Roles.AddUsersToRoles(usernames, roles);

			//assert
			string[] actual = Roles.GetRolesForUser(username);
			Assert.IsTrue(actual.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase)));
			Assert.IsTrue(actual.Any(r => r.Equals(roleName + "-2", StringComparison.OrdinalIgnoreCase)));

			Roles.DeleteRole(roleName, false);
			Roles.DeleteRole(roleName + "-2", false);
		}

		[Test]
		public void CanRemoveUserFromRole()
		{
			//arrange
			const string roleName = ROLE_NAME + "8";
			const string username = USERNAME + "8";
			Roles.CreateRole(roleName);
			Roles.AddUserToRole(username, roleName);

			//act
			Roles.RemoveUserFromRole(username, roleName);

			//assert
			string[] actual = Roles.GetRolesForUser(username);
			Assert.That(actual.Length, Is.EqualTo(0));
		}

		[Test]
		public void CanRemoveUsersFromRoles()
		{
			//arrange
			const string roleName = ROLE_NAME + "9";
			const string username = USERNAME + "9";
			Roles.CreateRole(roleName);
			Roles.CreateRole(roleName + "-2");
			string[] roles = new[] { roleName, roleName + "-2" };
			string[] usernames = new[] { username, username + "-2" };
			Roles.AddUsersToRoles(usernames, roles);

			//act
			Roles.RemoveUsersFromRoles(usernames, roles);

			//assert
			string[] actual = Roles.GetRolesForUser(username);
			Assert.IsFalse(actual.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase)));
			Assert.IsFalse(actual.Any(r => r.Equals(roleName + "-2", StringComparison.OrdinalIgnoreCase)));

			string[] actual2 = Roles.GetRolesForUser(username + "-2");
			Assert.IsFalse(actual.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase)));
			Assert.IsFalse(actual.Any(r => r.Equals(roleName + "-2", StringComparison.OrdinalIgnoreCase)));

			Roles.DeleteRole(roleName, false);
			Roles.DeleteRole(roleName + "-2", false);
		}

		[Test]
		public void FindUsersInRole()
		{
			//arrange
			const string roleName = ROLE_NAME + "10";
			const string username = USERNAME + "10";
			Roles.CreateRole(roleName);
			string[] roles = new[] { roleName };
			string[] usernames = new[] { username, username + "-2", "3-" + username};
			Roles.AddUsersToRoles(usernames, roles);

			//act
			string[] actual = Roles.FindUsersInRole(roleName, username);

			//assert
			Assert.IsTrue(actual.Length == 2);
			Assert.IsTrue(actual.Any(r => r == username));
			Assert.IsTrue(actual.Any(r => r == username + "-2"));
			Assert.IsFalse(actual.Any(r => r == "3-" + username));

			Roles.DeleteRole(roleName, false);
		}
	}
}
