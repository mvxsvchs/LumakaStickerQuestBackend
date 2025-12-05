using Xunit; // xUnit Framework importieren
using System.Threading.Tasks;
using LumakaStickerQuestBackend.Functions;
using LumakaStickerQuestBackend.Classes;


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

    // UserS-tests
    public class FunctionTests
    {
        // 1. GetById
        [fact]
        public async Task GetById_ReturnsNull_ForInvalidId()
        {
            var userService = new Services.UserS();
            var result = await userService.GetById(-1);
            Assert.Null(result);
        }

        // 2. GetByMailAndPwd
        [fact]
        public async Task GetByMailAndPwd_ReturnsNull_WhenWrongPwd()
        {
            var userService = new Services.UserS();
            var result = await userService.GetByMailAndPwd("test@test.com", "falschespwd");
            Assert.Null(result)
        }

        // 3. RegisterUser
        [fact]
        public async Task RegisterUser_ReturnsFalse_ForInvalidUser()
        {
            var userService = new Services.UserS();
            var invalidUser = new FeRegister
            {
                Username = null, // ungültiger Username
                Mail = "test@test.com",
                Password = "pass123"
            };
            var result = await userService.RegisterUser(invalidUser);
            Assert.False(result);
        }

    }

}