using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Transactions;
using System.Web.Security;
using System.Linq;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Linq;

namespace RavenDbProviders.MembershipProvider
{
	public class RavenDbMembershipProvider : System.Web.Security.MembershipProvider
	{
		private static IDocumentStore _store = new EmbeddableDocumentStore{RunInMemory = true};
		private static NameValueCollection _config;
		private static string _name;
		private static string _applicationName;
		private bool _isInitialized = false;
		public const string PROVIDER_NAME = "RavenDbMembershipProvider";

		public override void Initialize(string name, NameValueCollection config)
		{
			lock (this)
			{
				if ( _isInitialized )
					return;

				_config = config;
				_name = name;
				_applicationName = _config["applicationName"];
				base.Initialize(name, config);

				//check if we need to run in memory or on a dedicated server
				ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings["RavenDbMembershipDb"];
				_store = connectionString == null 
					? new EmbeddableDocumentStore {RunInMemory = true} 
					: new DocumentStore{ConnectionStringName = "RavenDbMembershipDb"};

				_store.Initialize();
				_isInitialized = true;
			}
		}

		public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
		{
			MembershipUser membershipUser = GetUser(username, true);
			if ( membershipUser == null )
			{
				using (TransactionScope scope = new TransactionScope())
				{
					byte[] passwordSalt = PasswordHelper.CreateSalt(32);
					byte[] passwordHash = PasswordHelper.GenerateSaltedHash(System.Text.Encoding.UTF8.GetBytes(password), passwordSalt);

					User user = new User
					{
						UserName = username,
						PasswordSalt = passwordSalt,
						PasswordHash = passwordHash,
						Email = email,
						PasswordQuestion = passwordQuestion,
						PasswordAnswer = passwordAnswer,
						IsApproved = isApproved,
						ProviderUserKey = providerUserKey,
						Comment = string.Empty,
						CreationDate = DateTime.Now,
						IsLockedOut = false,
						IsOnline = false,
						LastActivityDate = DateTime.Now,
						LastLockoutDate = DateTime.MinValue,
						LastLoginDate = DateTime.MinValue,
						LastPasswordChangedDate = DateTime.MinValue,
						ProviderName = PROVIDER_NAME
					};
					using (IDocumentSession session = _store.OpenSession())
					{
						session.Store(user);
						session.SaveChanges();
					}
					status = MembershipCreateStatus.Success;
					scope.Complete();
					return user.ToMembershipUser();
				}
			}
			else
			{
				status = MembershipCreateStatus.DuplicateUserName;
			}
			return null;
		}

		public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
		{
			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(newPasswordQuestion) || string.IsNullOrWhiteSpace(newPasswordAnswer))
				return false;

			using(TransactionScope scope = new TransactionScope())
			using(IDocumentSession session = _store.OpenSession())
			{
				User user = GetRavenUser(username);
				if (user == null)
					return false;
				if ( !PasswordHelper.ValidatePassword(user, password))
					return false;

				user.PasswordQuestion = newPasswordQuestion;
				user.PasswordAnswer = newPasswordAnswer;
				session.Store(user);
				session.SaveChanges();
				scope.Complete();
				return true;
			}
		}

		public override string GetPassword(string username, string answer)
		{
			throw new NotImplementedException();
		}

		public override bool ChangePassword(string username, string oldPassword, string newPassword)
		{
			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
				return false;

			User user = GetRavenUser(username);
			if (user == null)
				return false;

			if (!PasswordHelper.ValidatePassword(user, oldPassword))
				return false;

			return true;
		}

		public override string ResetPassword(string username, string answer)
		{
			throw new NotImplementedException();
		}

		public override void UpdateUser(MembershipUser user)
		{
			throw new NotImplementedException();
		}

		public override bool ValidateUser(string username, string password)
		{
			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
				return false;

			User user = GetRavenUser(username);
			if (user == null)
				return false;

			byte[] checkHash = PasswordHelper.GenerateSaltedHash(System.Text.Encoding.UTF8.GetBytes(password), user.PasswordSalt);
			return checkHash.SequenceEqual(user.PasswordHash);
		}

		public override bool UnlockUser(string userName)
		{
			throw new NotImplementedException();
		}

		public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
		{
			throw new NotImplementedException();
		}

		public override MembershipUser GetUser(string username, bool userIsOnline)
		{
			using(IDocumentSession session = _store.OpenSession())
			{
				User user = GetRavenUser(username);
				if ( user == null )
					return null;
				return user.ToMembershipUser();
			}
		}

		public override string GetUserNameByEmail(string email)
		{
			throw new NotImplementedException();
		}

		public override bool DeleteUser(string username, bool deleteAllRelatedData)
		{
			using(TransactionScope scope = new TransactionScope())
			using(IDocumentSession session = _store.OpenSession())
			{
				User deleteMe = GetRavenUser(username);
				if ( deleteMe != null )
				{
					session.Delete(deleteMe);
					session.SaveChanges();
				}
				scope.Complete();
				return true;
			}
		}

		public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override int GetNumberOfUsersOnline()
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override bool EnablePasswordRetrieval
		{
			get { throw new NotImplementedException(); }
		}

		public override bool EnablePasswordReset
		{
			get { throw new NotImplementedException(); }
		}

		public override bool RequiresQuestionAndAnswer
		{
			get { throw new NotImplementedException(); }
		}

		public override string ApplicationName
		{
			get { return _applicationName; }
			set { _applicationName = value; }
		}

		public override int MaxInvalidPasswordAttempts
		{
			get { throw new NotImplementedException(); }
		}

		public override int PasswordAttemptWindow
		{
			get { throw new NotImplementedException(); }
		}

		public override bool RequiresUniqueEmail
		{
			get { throw new NotImplementedException(); }
		}

		public override MembershipPasswordFormat PasswordFormat
		{
			get { throw new NotImplementedException(); }
		}

		public override int MinRequiredPasswordLength
		{
			get { throw new NotImplementedException(); }
		}

		public override int MinRequiredNonAlphanumericCharacters
		{
			get { throw new NotImplementedException(); }
		}

		public override string PasswordStrengthRegularExpression
		{
			get { throw new NotImplementedException(); }
		}

		private User GetRavenUser(string username)
		{
			if ( string.IsNullOrWhiteSpace(username))
				return null;

			using(IDocumentSession session = _store.OpenSession())
			{
				return session.Query<User>().FirstOrDefault(u => u.UserName == username);
			}
		}
	}
}
