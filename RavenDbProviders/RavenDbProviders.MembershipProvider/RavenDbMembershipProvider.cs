using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Transactions;
using System.Web.Configuration;
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
		private bool _isInitialized;

		private static bool _enablePasswordRetrieval;
		private static bool _enablePasswordReset;
		private static bool _requiresQuestionAndAnswer;
		private static bool _requiresUniqueEmail;
		private static int _maxInvalidPasswordAttempts;
		private static int _minRequiredPasswordLength;
		private static int _minRequiredNonalphanumericCharacters;
		private static int _passwordAttemptWindow;

		public const string ProviderName = "RavenDbMembershipProvider";

		public override void Initialize(string name, NameValueCollection config)
		{
			lock (this)
			{
				if ( _isInitialized )
					return;

				_config = config;
				_name = name;
				_applicationName = _config["applicationName"];

				bool.TryParse(_config["enablePasswordRetrieval"], out _enablePasswordRetrieval);
				bool.TryParse(_config["enablePasswordReset"], out _enablePasswordReset);
				bool.TryParse(_config["requiresQuestionAndAnswer"], out _requiresQuestionAndAnswer);
				bool.TryParse(_config["requiresUniqueEmail"], out _requiresUniqueEmail);
				int.TryParse(_config["maxInvalidPasswordAttempts"], out _maxInvalidPasswordAttempts);
				int.TryParse(_config["minRequiredPasswordLength"], out _minRequiredPasswordLength);
				int.TryParse(_config["minRequiredNonalphanumericCharacters"], out _minRequiredNonalphanumericCharacters);
				int.TryParse(_config["passwordAttemptWindow"], out _passwordAttemptWindow);

				//check if we need to run in memory or on a dedicated server
				string connectionStringName = config["connectionStringName"];
				_store = connectionStringName == null 
					? new EmbeddableDocumentStore {RunInMemory = true} 
					: new DocumentStore{ConnectionStringName = connectionStringName};

				_store.Initialize();
				_isInitialized = true;

				base.Initialize(name, config);
			}
		}

		public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
		{
			ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(args);
            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

			if (RequiresUniqueEmail && !string.IsNullOrEmpty(GetUserNameByEmail(email)))
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

			MembershipUser membershipUser = GetUser(username, true);
			if (membershipUser != null)
			{
				status = MembershipCreateStatus.DuplicateUserName;
				return null;
			}

			using (TransactionScope scope = new TransactionScope())
			{
				byte[] passwordSalt = PasswordHelper.CreateSalt(32);
				byte[] passwordHash = PasswordHelper.GenerateSaltedHash(System.Text.Encoding.UTF8.GetBytes(password), passwordSalt);
				byte[] answerHash = PasswordHelper.GenerateSaltedHash(System.Text.Encoding.UTF8.GetBytes(passwordAnswer ?? ""),passwordSalt);

				User user = new User
				{
					Username = username,
					Salt = passwordSalt,
					Password = passwordHash,
					Email = email,
					PasswordQuestion = passwordQuestion,
					PasswordAnswer = answerHash,
					IsApproved = isApproved,
					ProviderUserKey = providerUserKey,
					Comment = string.Empty,
					CreationDate = DateTime.Now,
					IsLockedOut = false,
					LastActivityDate = DateTime.Now,
					LastLockoutDate = DateTime.MinValue,
					LastLoginDate = DateTime.MinValue,
					LastPasswordChangedDate = DateTime.MinValue,
					ProviderName = ProviderName
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

		public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
		{
			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(newPasswordQuestion) || string.IsNullOrWhiteSpace(newPasswordAnswer))
				return false;

			
			using(IDocumentSession session = _store.OpenSession())
			{
				User user = GetRavenUser(session, username);
				if (user == null)
					return false;
				if ( !PasswordHelper.ValidatePassword(user, password))
					return false;

				using (TransactionScope scope = new TransactionScope())
				{
					byte[] answerHash = PasswordHelper.GenerateSaltedHash(System.Text.Encoding.UTF8.GetBytes(newPasswordAnswer), user.Salt);

					user.PasswordQuestion = newPasswordQuestion;
					user.PasswordAnswer = answerHash;
					session.Store(user);
					session.SaveChanges();
					scope.Complete();
					return true;
				}
			}
		}

		public override string GetPassword(string username, string answer)
		{
			throw new NotSupportedException("password retrieval is not supported");
		}

		public override bool ChangePassword(string username, string oldPassword, string newPassword)
		{
			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
				return false;

			using (IDocumentSession session = _store.OpenSession())
			{
				User user = GetRavenUser(session, username);
				if (user == null)
					return false;

				if (!PasswordHelper.ValidatePassword(user, oldPassword))
					return false;

				byte[] passwordHash = PasswordHelper.GenerateSaltedHash(System.Text.Encoding.UTF8.GetBytes(newPassword), user.Salt);

				using(TransactionScope scope = new TransactionScope())
				{
					user.Password = passwordHash;
					session.Store(user);
					session.SaveChanges();
					scope.Complete();
				}

				return true;
			}
		}

		public override string ResetPassword(string username, string answer)
		{
			if (string.IsNullOrWhiteSpace(username))
				throw new MembershipPasswordException("username missing");
	
			if ( !EnablePasswordReset )
				throw new NotSupportedException("password reset is not supported");

			if ( RequiresQuestionAndAnswer && string.IsNullOrWhiteSpace(answer))
				throw new MembershipPasswordException("no password answer provided");

			using(IDocumentSession session = _store.OpenSession())
			{
				User user = GetRavenUser(session, username);
				if ( user == null )
					throw new MembershipPasswordException("user not found");

				if ( RequiresQuestionAndAnswer && !PasswordHelper.ValidateAnswer(user, answer) )
					throw new MembershipPasswordException("answer is invalid");

				string newPassword = Membership.GeneratePassword(Membership.MinRequiredPasswordLength, Membership.MinRequiredNonAlphanumericCharacters);

				byte[] newPasswordHash = PasswordHelper.GenerateSaltedHash(newPassword, user.Salt);
				using(TransactionScope scope = new TransactionScope())
				{
					user.Password = newPasswordHash;
					session.Store(user);
					session.SaveChanges();
					scope.Complete();
				}

				return newPassword;
			}
		}

		public override void UpdateUser(MembershipUser user)
		{
			if ( string.IsNullOrEmpty(user.UserName) )
				return;

			using(IDocumentSession session = _store.OpenSession())
			{
				User ravenUser = GetRavenUser(session, user.UserName);
				if ( ravenUser == null )
					return;
				
				using(TransactionScope scope = new TransactionScope())
				{
					ravenUser.Comment = user.Comment;
					ravenUser.Email = user.Email;
					ravenUser.IsApproved = user.IsApproved;
					session.Store(ravenUser);
					session.SaveChanges();
					scope.Complete();
				}
			}
		}

		public override bool ValidateUser(string username, string password)
		{
			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
				return false;

			using(TransactionScope scope = new TransactionScope())
			using (IDocumentSession session = _store.OpenSession())
			{
				User user = GetRavenUser(session, username);
				if (user == null)
					return false;

				bool passwordIsValid = PasswordHelper.ValidatePassword(user, password);

				if ( passwordIsValid )
				{
					user.LastActivityDate = DateTime.Now;
					session.Store(user);
				}

				scope.Complete();

				return passwordIsValid;
			}
		}

		public override bool UnlockUser(string username)
		{
			using(IDocumentSession session = _store.OpenSession())
			{
				User user = GetRavenUser(session, username);
				if ( user == null )
					return false;
				user.IsLockedOut = false;

				using(TransactionScope scope = new TransactionScope())
				{
					session.Store(user);
					scope.Complete();
				}
				return true;
			}
		}

		public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
		{
			if ((providerUserKey as string) == null)
				return null;

			using(IDocumentSession session = _store.OpenSession())
			{
				User user = GetRavenUserById(session, providerUserKey.ToString());
				if ( user == null )
					return null;

				if ( userIsOnline )
				{
					using (TransactionScope scope = new TransactionScope())
					{
						user.LastActivityDate = DateTime.Now;
						session.Store(user);
						scope.Complete();
					}
				}
				return user.ToMembershipUser();
			}
		}

		public override MembershipUser GetUser(string username, bool userIsOnline)
		{
			using(IDocumentSession session = _store.OpenSession())
			{
				User user = GetRavenUser(session, username);
				if ( user == null )
					return null;

				if ( userIsOnline)
				{
					using(TransactionScope scope = new TransactionScope())
					{
						user.LastActivityDate = DateTime.Now;
						session.Store(user);
						scope.Complete();
					}
				}
				return user.ToMembershipUser();
			}
		}

		public override string GetUserNameByEmail(string email)
		{
			using(IDocumentSession session = _store.OpenSession())
			{
				User user = GetRavenUserByEmail(session, email);
				if ( user == null )
					return null;

				return user.Username;
			}
		}

		public override bool DeleteUser(string username, bool deleteAllRelatedData)
		{
			using(TransactionScope scope = new TransactionScope())
			using(IDocumentSession session = _store.OpenSession())
			{
				User deleteMe = GetRavenUser(session, username);
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
			using(IDocumentSession session = _store.OpenSession())
			{
				int skip = pageSize * pageIndex;
				RavenQueryStatistics stats;
				IEnumerable<User> users = session.Query<User>()
					.Customize(c => c.WaitForNonStaleResultsAsOfNow())
					.Statistics(out stats)
					.Skip(skip)
					.Take(pageSize)
					.ToList();
				totalRecords = stats.TotalResults;
				return users.ToMembershipUserCollection();
			}
		}

		public override int GetNumberOfUsersOnline()
		{
			/*
			 * http://msdn.microsoft.com/en-us/library/system.web.security.membershipuser.isonline.aspx
			 A user is considered online if the current date and time minus the UserIsOnlineTimeWindow property value is earlier than the LastActivityDate for the user.
			 The LastActivityDate for a user is updated to the current date and time by the CreateUser, UpdateUser and ValidateUser methods, and can be updated by some of the overloads of the GetUser method.
			 */
			using (IDocumentSession session = _store.OpenSession())
			{
				DateTime cutOff = DateTime.Now.AddMinutes(-Membership.UserIsOnlineTimeWindow);
				return session.Query<User>().Count(u => u.LastActivityDate >= cutOff);
			}
		}

		public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			using(IDocumentSession session = _store.OpenSession())
			{
				int skip = pageSize * pageIndex;
				RavenQueryStatistics stats;
				IEnumerable<User> users = session.Query<User>()
					.Customize(c => c.WaitForNonStaleResultsAsOfNow())
					.Statistics(out stats)
					.Skip(skip)
					.Take(pageSize)
					.Where(u => u.Username.StartsWith(usernameToMatch))
					.ToList();
				totalRecords = stats.TotalResults;
				return users.ToMembershipUserCollection();
			}
		}

		public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			using (IDocumentSession session = _store.OpenSession())
			{
				int skip = pageSize * pageIndex;
				RavenQueryStatistics stats;
				IEnumerable<User> users = session.Query<User>()
					.Statistics(out stats)
					.Customize(c => c.WaitForNonStaleResultsAsOfNow())
					.Skip(skip)
					.Take(pageSize)
					.Where(u => u.Email.StartsWith(emailToMatch))
					.ToList();
				totalRecords = stats.TotalResults;
				return users.ToMembershipUserCollection();
			}
		}

		public override bool EnablePasswordRetrieval
		{
			get { return _enablePasswordRetrieval; }
		}

		public override bool EnablePasswordReset
		{
			get { return _enablePasswordReset; }
		}

		public override bool RequiresQuestionAndAnswer
		{
			get { return _requiresQuestionAndAnswer; }
		}

		public override string ApplicationName
		{
			get { return _applicationName; }
			set { _applicationName = value; }
		}

		public override int MaxInvalidPasswordAttempts
		{
			get { return _maxInvalidPasswordAttempts; }
		}

		public override int PasswordAttemptWindow
		{
			get { return _passwordAttemptWindow; }
		}

		public override bool RequiresUniqueEmail
		{
			get { return _requiresUniqueEmail; }
		}

		public override MembershipPasswordFormat PasswordFormat
		{
			get { return MembershipPasswordFormat.Hashed; }
		}

		public override int MinRequiredPasswordLength
		{
			get { return _minRequiredPasswordLength; }
		}

		public override int MinRequiredNonAlphanumericCharacters
		{
			get { return _minRequiredNonalphanumericCharacters; }
		}

		public override string PasswordStrengthRegularExpression
		{
			get { throw new NotImplementedException(); }
		}

		
		private User GetRavenUser(IDocumentSession session, string username)
		{
			if ( string.IsNullOrWhiteSpace(username))
				return null;

			return session.Query<User>()
				.Customize(c => c.WaitForNonStaleResultsAsOfNow())
				.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
		}

		private User GetRavenUserByEmail(IDocumentSession session, string email)
		{
			if (string.IsNullOrWhiteSpace(email))
				return null;

			return session.Query<User>()
				.Customize(c => c.WaitForNonStaleResultsAsOfNow())
				.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
		}

		private User GetRavenUserById(IDocumentSession session, string id)
		{
			if (string.IsNullOrWhiteSpace(id))
				return null;

			return session.Query<User>()
				.Customize(c => c.WaitForNonStaleResultsAsOfNow())
				.FirstOrDefault(u => u.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
		}
	}
}
