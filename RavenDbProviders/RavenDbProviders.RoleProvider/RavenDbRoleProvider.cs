using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Linq;
using System.Linq.Expressions;
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
			if ( string.IsNullOrWhiteSpace(username))
				return new string[]{};

		    using(IDocumentSession session = _store.OpenSession())
		    {
			    return session.Query<Role>()
					.Customize(c => c.WaitForNonStaleResultsAsOfNow())
				    .Where(r => r.Members.Any(rm => rm == username))
				    .Select(r => r.RoleName)
				    .ToArray();
		    }
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
			ModifyUsersInRoles(usernames, roleNames, true);
	    }

	    public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
	    {
			ModifyUsersInRoles(usernames, roleNames, false);
	    }

		private void ModifyUsersInRoles(string[] usernames, string[] roleNames, bool addToRoles)
		{
			if (usernames == null || roleNames == null || usernames.Length == 0 || roleNames.Length == 0)
				return;

			using (IDocumentSession session = _store.OpenSession())
			{
				Expression<Func<Role, bool>> roleNameWhere = PredicateBuilder.False<Role>();
				roleNames.ToList().ForEach(currentRoleName =>
				{
					roleNameWhere = roleNameWhere.Or(r => r.RoleName.Equals(currentRoleName, StringComparison.OrdinalIgnoreCase));
				});
				IList<Role> roles = session.Query<Role>()
					.Customize(c => c.WaitForNonStaleResultsAsOfNow())
					.Where(roleNameWhere.Compile())
					.ToList();

				using (TransactionScope scope = new TransactionScope())
				{
					foreach (Role role in roles)
					{
						foreach (string username in usernames)
						{
							if ( addToRoles )
							{
								if ( !role.Members.Contains(username))	
									role.Members.Add(username);
							}
							else
							{
								if (role.Members.Contains(username))
									role.Members.Remove(username);
							}
						}
						session.Store(role);
						session.SaveChanges();
					}
					scope.Complete();
				}
			}
		}

	    public override string[] GetUsersInRole(string roleName)
	    {
		    using(IDocumentSession session = _store.OpenSession())
		    {
			    Role role = session.Query<Role>().FirstOrDefault(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
				return role == null ? new string[]{} : role.Members.ToArray();
		    }
	    }

	    public override string[] GetAllRoles()
	    {
		    using(IDocumentSession session = _store.OpenSession())
		    {
			    return session.Query<Role>().Customize(c => c.WaitForNonStaleResultsAsOfNow()).Select(r => r.RoleName).ToArray();
		    }
	    }

	    public override string[] FindUsersInRole(string roleName, string usernameToMatch)
	    {
		    if ( string.IsNullOrWhiteSpace(roleName) || string.IsNullOrWhiteSpace(usernameToMatch))
				return new string[]{};

			using(IDocumentSession session = _store.OpenSession())
			{
				Role role = session.Query<Role>().FirstOrDefault(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
				if ( role == null)
					return new string[]{};

				return role.Members.Where(rm => rm.StartsWith(usernameToMatch)).ToArray();
			}
	    }

	    public override string ApplicationName { get; set; }

		private Role GetRavenRole(IDocumentSession session, string roleName)
		{
			return session.Query<Role>().Customize(c => c.WaitForNonStaleResultsAsOfNow()) .FirstOrDefault(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
		}
    }
}
