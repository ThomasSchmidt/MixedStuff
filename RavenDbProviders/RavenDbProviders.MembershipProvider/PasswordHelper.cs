using System;
using System.Security.Cryptography;

namespace RavenDbProviders.MembershipProvider
{
	internal static class PasswordHelper
	{
		internal static string CreateSalt(int size)
		{
			//Generate a cryptographic random number.
			using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
			{
				byte[] buff = new byte[size];
				rng.GetBytes(buff);

				// Return a Base64 string representation of the random number.
				return Convert.ToBase64String(buff);
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
	}
}
