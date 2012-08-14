using System;
using System.Security.Cryptography;

namespace RavenDbProviders.MembershipProvider
{
	internal static class PasswordHelper
	{
		internal static byte[] CreateSalt(int size)
		{
			//Generate a cryptographic random number.
			using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
			{
				byte[] buff = new byte[size];
				rng.GetBytes(buff);
				return buff;
				// Return a Base64 string representation of the random number.
				//return Convert.ToBase64String(buff);
			}
		}

		internal static byte[] GenerateSaltedHash(byte[] plainText, byte[] salt)
		{
			using (HashAlgorithm algorithm = new SHA256Managed())
			{
				byte[] plainTextWithSaltBytes = new byte[plainText.Length + salt.Length];

				for (int i = 0; i < plainText.Length; i++)
				{
					plainTextWithSaltBytes[i] = plainText[i];
				}
				for (int i = 0; i < salt.Length; i++)
				{
					plainTextWithSaltBytes[plainText.Length + i] = salt[i];
				}

				return algorithm.ComputeHash(plainTextWithSaltBytes);
			}
		}

		internal static bool ValidatePassword(User user, string password)
		{
			if (user == null || string.IsNullOrWhiteSpace(password))
				return false;

			byte[] providedPasswordHash = GenerateSaltedHash(System.Text.Encoding.UTF8.GetBytes(password), user.PasswordHash);
			return providedPasswordHash == user.PasswordHash;
		}
	}
}
