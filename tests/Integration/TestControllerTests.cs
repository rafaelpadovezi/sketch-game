using System.Net;
using System.Threading.Tasks;
using Tests.Support;
using Xunit;

namespace Tests.Integration
{
    public class TestControllerTests : TestingCaseFixture<TestingStartUp>
    {
        [Theory(DisplayName = "Should make a request and get Ok")]
        [InlineData("/api/v1/test")]
        public async Task ShouldMakeRequestGetHttp200(string url)
        {
            // arrange

            // act
            var response = await Client.GetAsync(url);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
