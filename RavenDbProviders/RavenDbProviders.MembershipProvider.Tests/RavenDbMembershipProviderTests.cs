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
			//arrange+act
			string username = "unit-test-" + Guid.NewGuid();
			MembershipUser actual = Membership.CreateUser(username, "password");

			//assert
			Assert.That(actual.UserName, Is.EqualTo(username));
		}
	}
}
