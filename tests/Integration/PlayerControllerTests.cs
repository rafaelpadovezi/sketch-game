﻿using Sketch.DTOs;
using Sketch.Utils;
using System.Net;
using System.Threading.Tasks;
using Tests.Support;
using Xunit;

namespace Tests.Integration
{
    public class PlayerControllerTests : TestingCaseFixture<TestingStartUp>
    {
        [Theory(DisplayName = "Should create player")]
        [InlineData("api/v1/player/login")]
        public async Task ShouldCreatePlayer(string url)
        {
            // arrange
            var username = "Player123";

            // act
            var response = await Client.PostJsonAsync(url, username);
            var player = await response.Content.ReadAsJsonAsync<PlayerViewModel>();

            // assert
            Assert.NotEqual(default, player.Id);
            Assert.Equal(username, player.Username);
        }

        [Theory(DisplayName = "Should return Login is taken errpr")]
        [InlineData("api/v1/player/login")]
        public async Task ShouldReturnLoginIsTakenError(string url)
        {
            // arrange
            var username = "Player123";
            var testingScenarioBuilder = new TestingScenarioBuilder(DbContext);
            await testingScenarioBuilder.BuildScenarioPlayer(username);

            // act
            var response = await Client.PostJsonAsync(url, username);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
