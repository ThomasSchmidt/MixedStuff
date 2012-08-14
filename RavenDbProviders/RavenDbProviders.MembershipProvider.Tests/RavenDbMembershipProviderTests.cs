using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Web.Security;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace RavenDbProviders.MembershipProvider.Tests
{
	[TestFixture]
	public class RavenDbMembershipProviderTests
	{
		[Test]
		public void ApplicationNameMatchesConfigSetting()
		{
			//arrange
			string configPath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase + ".config").LocalPath;
			XDocument appConfig = null;
			using (FileStream fs = new FileStream(configPath, FileMode.Open, FileAccess.Read))
			{
				appConfig = XDocument.Load(fs);
				fs.Close();
			}
			string appName = appConfig.Descendants().Where(x => x.Name == "providers").First().Element("add").Attribute("applicationName").Value;

			//act+assert
			Assert.That(Membership.ApplicationName, Is.EqualTo(appName));
		}

		[Test]
		public void CanCreateUser()
		{
			//arrange
			string username = "unit-test-" + Guid.NewGuid();
			MembershipUser created = Membership.CreateUser(username, "password", "thomas.schmidt@valtech.dk");

			//act
			MembershipUser actual = Membership.GetUser(username);

			//assert
			Assert.That(actual.UserName, Is.EqualTo(created.UserName));

			Membership.DeleteUser(username);
		}

		[Test]
		public void CanDeleteUser()
		{
			//arrange
			string username = "unit-test-" + Guid.NewGuid();
			MembershipUser created = Membership.CreateUser(username, "password", "thomas.schmidt@valtech.dk");

			//act
			bool actual = Membership.DeleteUser(username);

			//assert
			Assert.That(Membership.GetUser(username), Is.Null);
		}

		[Test]
		public void CanChangePasswordQuestionAndAnswer()
		{
			//arrange
			string username = "unit-test-" + Guid.NewGuid();
			MembershipUser created = Membership.CreateUser(username, "password", "thomas.schmidt@valtech.dk");
			RavenDbMembershipProvider provider = new RavenDbMembershipProvider();

			//act
			bool actual = provider.ChangePasswordQuestionAndAnswer(username, "password", "new question", "new answer");

			//assert
			Assert.IsTrue(actual);
		}

		[Test]
		public void PasswordHashShouldNotChangeAfterBeingStoredInRavenDb()
		{
			//arrange
			string username = "unit-test-" + Guid.NewGuid();
			MembershipUser created = Membership.CreateUser(username, "password", "thomas.schmidt@valtech.dk");

			//act
			bool actual = Membership.ValidateUser(username, "password");

			//assert
			Assert.IsTrue(actual);
		}
	}
}
