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
		private const string ROLE_USER = "roleUser";

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
			Roles.CreateRole(ROLE_NAME);
			Roles.AddUserToRole(ROLE_USER, ROLE_NAME);

			//act
			bool actual = Roles.IsUserInRole(ROLE_USER, ROLE_NAME);

			//assert
			Assert.IsTrue(actual);

			Roles.DeleteRole(ROLE_NAME, false);
		}

		[Test]
		public void CanGetRolesForUser()
		{
			//arrange
			Roles.CreateRole(ROLE_NAME);
			Roles.AddUserToRole(ROLE_USER, ROLE_NAME);

			//act
			string[] actual = Roles.GetRolesForUser(ROLE_USER);

			//assert
			Assert.That(actual.Length, Is.EqualTo(1));

			Roles.DeleteRole(ROLE_NAME);
		}

		[Test]
		public void CanCreateRole()
		{
			//act
			Roles.CreateRole(ROLE_NAME);

			//assert
			Assert.IsTrue(Roles.RoleExists(ROLE_NAME));
		}

		[Test]
		public void CanDeleteRole()
		{
			//arrange
			Roles.CreateRole(ROLE_NAME);

			//act
			Roles.DeleteRole(ROLE_NAME);

			//assert
			string[] actual = Roles.GetAllRoles();
			Assert.IsFalse(actual.Any(r => r.Equals(ROLE_NAME, StringComparison.OrdinalIgnoreCase)));
		}

		[Test]
		public void CanCheckIfRoleExists()
		{
			//arrange
			Roles.CreateRole(ROLE_NAME);

			//act
			bool actual = Roles.RoleExists(ROLE_NAME);

			//assert
			Assert.IsTrue(actual);

			Roles.DeleteRole(ROLE_NAME);
		}

		[Test]
		public void CanAddUsersToRole()
		{
			//arrange
			Roles.CreateRole(ROLE_NAME);

			//act
			Roles.AddUserToRole(ROLE_USER, ROLE_NAME);

			//assert
			string[] actual = Roles.GetRolesForUser(ROLE_USER);
			Assert.IsTrue(actual.Any(r => r.Equals(ROLE_NAME, StringComparison.OrdinalIgnoreCase)));

			Roles.DeleteRole(ROLE_NAME);
		}
    }
}
