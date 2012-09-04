using System;
using System.Linq;
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

		internal static byte[] GenerateSaltedHash(string plainText, byte[] salt)
		{
			return GenerateSaltedHash(System.Text.Encoding.UTF8.GetBytes(plainText), salt);
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

			byte[] checkHash = GenerateSaltedHash(password, user.Salt);
			return checkHash.SequenceEqual(user.Password);
		}

		internal static bool ValidateAnswer(User user, string answer)
		{
			if (user == null || string.IsNullOrWhiteSpace(answer))
				return false;

			byte[] checkHash = GenerateSaltedHash(answer, user.Salt);
			return checkHash.SequenceEqual(user.PasswordAnswer);
		}
	}
}
