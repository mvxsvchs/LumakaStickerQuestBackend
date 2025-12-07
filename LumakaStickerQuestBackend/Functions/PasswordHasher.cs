using BCrypt.Net;

namespace LumakaStickerQuestBackend.Functions
{
	public static class PasswordHasher
	{
		public static string Hash(string password)
		{
			return BCrypt.Net.BCrypt.EnhancedHashPassword(password);
		}

		public static bool Verify(string password, string hashedPassword)
		{
			return BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
		}
	}
}
