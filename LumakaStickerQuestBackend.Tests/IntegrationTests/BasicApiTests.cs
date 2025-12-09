using System.Threading.Tasks;
using Xunit;

namespace LumakaStickerQuestBackend.Tests.IntegrationTests
{
    public class BasicApiTests : IClassFixture<TestApiFactory>
    {
        private readonly TestApiFactory _factory;

        public BasicApiTests(TestApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Api_Starts_And_Returns_404_For_Unknown_Route()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/unknown");

            Assert.Equal(System.NEthernet.HttpStatusCode.NotFound, response.StatusCode)
        }
    }
}