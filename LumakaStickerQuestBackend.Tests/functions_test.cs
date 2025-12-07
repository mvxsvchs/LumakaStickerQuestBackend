using System.Threading.Tasks;
using LumakaStickerQuestBackend.Functions;
using LumakaStickerQuestBackend.Classes;
using Xunit;

namespace LumakaStickerQuestBackend.Tests
{
	// Dummytest
	public class DummyTests
	{
		[Fact]
		public void Addition_isCorrect()
		{
			Assert.Equal(4, 2 + 2);
		}
	}

	// UserS-tests (integration-style, skipped until a test database is available)
	public class FunctionTests
	{
		[Fact(Skip = "Requires database connection")]
		public async Task GetById_ReturnsNull_ForInvalidId()
		{
			var userService = new Services.UserS();
			var result = await userService.GetById(-1);
			Assert.Null(result);
		}

		[Fact(Skip = "Requires database connection")]
		public async Task GetByMailAndPwd_ReturnsNull_WhenWrongPwd()
		{
			var userService = new Services.UserS();
			var result = await userService.GetByMailAndPwd("test@test.com", "falschespwd");
			Assert.Null(result);
		}

		[Fact(Skip = "Requires database connection")]
		public async Task RegisterUser_ReturnsFalse_ForInvalidUser()
		{
			var userService = new Services.UserS();
			var invalidUser = new FeRegister
			{
				Username = null, // ungueltiger Username
				Mail = "test@test.com",
				Password = "pass123"
			};
			var result = await userService.RegisterUser(invalidUser);
			Assert.False(result);
		}
	}

	public class PasswordHasherTests
	{
		[Fact]
		public void HashAndVerify_WorksForValidPassword()
		{
			const string password = "MySecurePassword123!";

			var hash = PasswordHasher.Hash(password);

			Assert.True(PasswordHasher.Verify(password, hash));
		}

		[Fact]
		public void Verify_FailsForWrongPassword()
		{
			const string password = "correct-horse-battery-staple";
			const string wrongPassword = "Tr0ub4dor&3";

			var hash = PasswordHasher.Hash(password);

			Assert.False(PasswordHasher.Verify(wrongPassword, hash));
		}
	}
}
