using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using BookSleeve;


namespace Redis.SessionStateProvider
{
	/// <summary>
	/// http://msdn.microsoft.com/en-us/library/ms178589(v=vs.100).aspx
	/// </summary>
	public class RedisSessionStateStoreProvider : SessionStateStoreProviderBase
	{
		private static HttpStaticObjectsCollection _staticObjects = new HttpStaticObjectsCollection();
		private static object _lock = new object();

		#region ProviderBase
		public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
		{
			if (config == null)
				throw new ArgumentNullException("config");

			if (string.IsNullOrWhiteSpace(name))
				name = "RedisSessionStateStoreProvider";

			base.Initialize(name, config);
		}
		#endregion

		#region SessionStateStoreProviderBase 
		public override void Dispose()
		{
		}

		public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
		{
			return false;
		}

		public override void InitializeRequest(HttpContext context)
		{
			
		}

		/// <summary>
		/// Called when SessionState = ReadOnly
		/// </summary>
		public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
		{
			RedisConnection redisConnection = ConnectionUtils.Connect("10.0.0.3:6379");
			{
				redisConnection.Open();

				var result = redisConnection.Strings.Get(0, id);
				byte[] raw = redisConnection.Wait(result);
			
				actions = SessionStateActions.None;

				SessionEntity sessionEntity = GetFromBytes(raw);
				if (sessionEntity == null || sessionEntity.SessionItems == null )
				{
					locked = false;
					lockId = _lock;
					lockAge = TimeSpan.MinValue;
					return null;
				}

				ISessionStateItemCollection sessionItems = new SessionStateItemCollection();
				foreach (string key in sessionEntity.SessionItems.Keys)
				{
					sessionItems[key] = sessionEntity.SessionItems[key];
				}

				SessionStateStoreData data = new SessionStateStoreData(sessionItems, _staticObjects, context.Session.Timeout);

				locked = false;
				lockId = _lock;
				lockAge = TimeSpan.MinValue;
				return data;
			}
		}

		/// <summary>
		/// Takes as input the HttpContext instance for the current request and the SessionID value for the current request. Retrieves session values and information from 
		/// the session data store and locks the session-item data at the data store for the duration of the request. 
		/// 
		/// The GetItemExclusive method sets several output-parameter values that inform the calling SessionStateModule about the state of the current session-state item in the data store.
		/// If no session item data is found at the data store, the GetItemExclusive method sets the locked output parameter to false and returns null. 
		/// This causes SessionStateModule to call the CreateNewStoreData method to create a new SessionStateStoreData object for the request.
		/// 
		/// If session-item data is found at the data store but the data is locked, the GetItemExclusive method sets the locked output parameter to true, 
		/// sets the lockAge output parameter to the current date and time minus the date and time when the item was locked, 
		/// sets the lockId output parameter to the lock identifier retrieved from the data store, and returns null. 
		/// This causes SessionStateModule to call the GetItemExclusive method again after a half-second interval, to attempt to retrieve the session-item information and obtain a lock on the data. 
		/// 
		/// If the value that the lockAge output parameter is set to exceeds the ExecutionTimeout value, SessionStateModule calls the ReleaseItemExclusive 
		/// method to clear the lock on the session-item data and then call the GetItemExclusive method again.
		/// 
		/// The actionFlags parameter is used with sessions whose Cookieless property is true, 
		/// when the regenerateExpiredSessionId attribute is set to true. An actionFlags value set to InitializeItem (1) 
		/// indicates that the entry in the session data store is a new session that requires initialization. 
		/// Uninitialized entries in the session data store are created by a call to the CreateUninitializedItem method. 
		/// If the item from the session data store is already initialized, the actionFlags parameter is set to zero.
		/// If your provider supports cookieless sessions, set the actionFlags output parameter to the value returned from the session data store for the current item. 
		/// If the actionFlags parameter value for the requested session-store item equals the InitializeItem enumeration value (1), 
		/// the GetItemExclusive method should set the value in the data store to zero after setting the actionFlagsout parameter.
		/// </summary>
		public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
		{
			RedisConnection redisConnection = ConnectionUtils.Connect("10.0.0.3:6379");

			var result = redisConnection.Strings.Get(0, id);
			byte[] raw = redisConnection.Wait(result);

			actions = SessionStateActions.None;

			SessionEntity sessionEntity = GetFromBytes(raw);

			if (sessionEntity == null || sessionEntity.SessionItems == null)
			{
				locked = false;
				lockId = _lock;
				lockAge = TimeSpan.MinValue;
				return null;
			}

			ISessionStateItemCollection sessionItems = new SessionStateItemCollection();
			foreach (string key in sessionEntity.SessionItems.Keys)
			{
				sessionItems[key] = sessionEntity.SessionItems[key];
			}

			SessionStateStoreData data = new SessionStateStoreData(sessionItems, _staticObjects, context.Session.Timeout);

			locked = false;
			lockId = _lock;
			lockAge = TimeSpan.MinValue;
			return data;
		}

		public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
		{
			throw new NotImplementedException();
		}

		public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
		{
			StringWriter sw = new StringWriter();
			
			RedisConnection redisConnection = ConnectionUtils.Connect("10.0.0.3:6379", sw);
			{
				var result = redisConnection.Strings.Get(0, id);
				byte[] raw = redisConnection.Wait(result);
				SessionEntity sessionEntity = GetFromBytes(raw) ?? new SessionEntity();

				foreach (var key in item.Items.Keys)
				{
					if (!sessionEntity.SessionItems.ContainsKey(key.ToString()))
					{
						sessionEntity.SessionItems.Add(key.ToString(), item.Items[key.ToString()]);
					}
					else
					{
						sessionEntity.SessionItems[key.ToString()] = item.Items[key.ToString()];
					}
				}

				redisConnection.Strings.Set(0, id, ToBytes(sessionEntity));
			}
		}

		public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
		{
			throw new NotImplementedException();
		}

		public override void ResetItemTimeout(HttpContext context, string id)
		{
			throw new NotImplementedException();
		}

		public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
		{
			HttpStaticObjectsCollection sessionStaticObjects = null;
			if (context != null)
			{
				sessionStaticObjects = SessionStateUtility.GetSessionStaticObjects(context);
			}
			return new SessionStateStoreData(new SessionStateItemCollection(), sessionStaticObjects, timeout);
		}

		public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
		{
			throw new NotImplementedException();
		}

		public override void EndRequest(HttpContext context)
		{
			
		}
		#endregion

		private SessionStateStoreData GetEmpty(int timeout)
		{
			ISessionStateItemCollection sessionItems = new SessionStateItemCollection();
			SessionStateStoreData data = new SessionStateStoreData(sessionItems, _staticObjects, timeout);
			return data;
		}

		private SessionEntity GetFromBytes(byte[] raw)
		{
			if (raw == null || raw.Length == 0)
				return null;

			using (MemoryStream ms = new MemoryStream(raw))
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				SessionEntity sessionEntity = binaryFormatter.Deserialize(ms) as SessionEntity;
				ms.Close();
				return sessionEntity;
			}
		}

		private byte[] ToBytes(SessionEntity sessionEntity)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				binaryFormatter.Serialize(ms, sessionEntity);
				return ms.ToArray();
			}
		}
	}
}
