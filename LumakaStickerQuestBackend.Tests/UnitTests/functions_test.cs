using Xunit; // xUnit Framework importieren
using System.Threading.Tasks;
using LumakaStickerQuestBackend.Functions;
using LumakaStickerQuestBackend.Classes;


namespace LumakaStickerQuestBackend.Tests
{
    // UserS-tests
    public class FunctionTests
    {
        // 1. GetById
        [Fact]
        public async Task GetById_ReturnsNull_ForInvalidId()
        {
            var userService = new Services.UserS();
            var result = await userService.GetById(-1);
            Assert.Null(result);
        }

        // 2. GetByMailAndPwd
        [Fact]
        public async Task GetByMailAndPwd_ReturnsNull_WhenWrongPwd()
        {
            var userService = new Services.UserS();
            var result = await userService.GetByMailAndPwd("test@test.com", "falschespwd");
            Assert.Null(result)
        }

        // 3. RegisterUser
        [Fact]
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