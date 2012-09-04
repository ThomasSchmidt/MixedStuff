using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Linq;
using System.Text;
using System.Transactions;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Linq;

namespace RavenDbProviders.RoleProvider
{
    public class RavenDbRoleProvider : System.Web.Security.RoleProvider
    {
		private static IDocumentStore _store = new EmbeddableDocumentStore { RunInMemory = true };
		private static NameValueCollection _config;
		private static string _name;
		private bool _isInitialized;

		public override void Initialize(string name, NameValueCollection config)
		{
			lock (this)
			{
				if (_isInitialized)
					return;

				_config = config;
				_name = name;
				ApplicationName = _config["applicationName"] ?? "RavenDbRoleProvider";

				//check if we need to run in memory or on a dedicated server
				string connectionStringName = config["connectionStringName"];
				_store = connectionStringName == null
					? new EmbeddableDocumentStore { RunInMemory = true }
					: new DocumentStore { ConnectionStringName = connectionStringName };

				_store.Initialize();
				_isInitialized = true;

				base.Initialize(name, config);
			}
		}

	    public override bool IsUserInRole(string username, string roleName)
	    {
			if ( string.IsNullOrWhiteSpace(username))
				throw new ProviderException("usename is missing");
			if ( string.IsNullOrWhiteSpace(roleName))
				throw new ProviderException("roleName is missing");

		    using(IDocumentSession session = _store.OpenSession())
		    {
			    bool check = session.Query<Role>()
					.Customize(c => c.WaitForNonStaleResultsAsOfNow())
				    .Any(r =>
					    r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase) &&
					    r.Members.Any(rm => rm.Equals(username, StringComparison.OrdinalIgnoreCase)));
			    return check;
		    }
	    }

	    public override string[] GetRolesForUser(string username)
	    {
		    throw new NotImplementedException();
	    }

	    public override void CreateRole(string roleName)
	    {
			if ( string.IsNullOrWhiteSpace(roleName))
				throw new ProviderException("roleName cannot be empty");
			if ( roleName.Contains(","))
				throw new ProviderException("invalid name provided for role, cannot contain comma");
			if ( RoleExists(roleName))
				throw new ProviderException("role already exists");

			using(TransactionScope scope = new TransactionScope())
			using(IDocumentSession session = _store.OpenSession())
			{
				Role role = new Role {RoleName = roleName};
				session.Store(role);
				session.SaveChanges();
				scope.Complete();
			}
	    }

	    public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
	    {
		    using(IDocumentSession session = _store.OpenSession())
		    {
			    Role role = GetRavenRole(session, roleName);
				if ( role == null )
					throw new ProviderException("role does not exist");

				if ( throwOnPopulatedRole && role.Members.Any())
					throw new ProviderException("cannot delete role as it has members");

				using (TransactionScope scope = new TransactionScope())
				{
					session.Delete(role);
					session.SaveChanges();
					scope.Complete();
					return true;
				}
		    }
	    }

	    public override bool RoleExists(string roleName)
	    {
		    using(IDocumentSession session = _store.OpenSession())
		    {
			    return session.Query<Role>()
					.Customize(c => c.WaitForNonStaleResultsAsOfNow())
					.Any(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
		    }
	    }

	    public override void AddUsersToRoles(string[] usernames, string[] roleNames)
	    {
		    using(IDocumentSession session = _store.OpenSession())
		    {
			    IList<Role> roles = session.Query<Role>()
					.Customize(c => c.WaitForNonStaleResultsAsOfNow())
				    .Where(r => roleNames.Any(rn => rn == r.RoleName))
				    .ToList();

				using (TransactionScope scope = new TransactionScope())
				{
					foreach (Role role in roles)
					{
						foreach (string username in usernames)
						{
							if (!role.Members.Contains(username))
								role.Members.Add(username);
						}
						session.Store(role);
						session.SaveChanges();
					}
					scope.Complete();
				}

		    }
	    }

	    public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
	    {
		    throw new NotImplementedException();
	    }

	    public override string[] GetUsersInRole(string roleName)
	    {
		    throw new NotImplementedException();
	    }

	    public override string[] GetAllRoles()
	    {
		    using(IDocumentSession session = _store.OpenSession())
		    {
			    return session.Query<Role>().Select(r => r.RoleName).ToArray();
		    }
	    }

	    public override string[] FindUsersInRole(string roleName, string usernameToMatch)
	    {
		    throw new NotImplementedException();
	    }

	    public override string ApplicationName { get; set; }

		private Role GetRavenRole(IDocumentSession session, string roleName)
		{
			return session.Query<Role>().FirstOrDefault(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
		}
    }
}
