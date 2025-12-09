using System.Threading.Tasks;
using Xunit;

namespace LumakaStickerQuestBackend.Tests.Integration
{
    public class UserRouteTests : IClassFixture<TestApiFactory>
    {
        private readonly TestApiFactory _factory;

        public UserRouteTests(TestApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Swagger_Endpoint_Returns_HTML()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/swagger");
            var content = await response.Content.ReadAsStringAsync();

            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("<html", content);
        }
    }
}
