using Microsoft.Extensions.DependencyInjection;
using Moq;
using Sketch.DTOs;
using Sketch.Infrastructure.Connection;
using Sketch.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Support;
using Xunit;

namespace Tests.Integration.Services
{
    public class GameServiceTests : TestingCaseFixture<TestingStartUp>, IDisposable
    {
        [Fact]
        public async Task ShouldSendNewPlayerMessageToGeneralRoomPlayers()
        {
            var server = Services.GetRequiredService<IServerConnection>();
            var testingScenarioBuilder = new TestingScenarioBuilder(DbContext);
            var mockedPlayers = await testingScenarioBuilder.BuildScenarioWith3ConnectedPlayers(server);

            var sut = Services.GetRequiredService<IGameService>();
            await sut.NewPlayer(mockedPlayers.First().Object.Id);

            foreach (var mock in mockedPlayers)
            {
                mock.Verify(
                    x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("has joined #general"))),
                    Times.Once);
            }

            server.Clear();
        }

        [Fact]
        public async Task ShouldSendPlayerLeftMessageToGeneralRoomPlayers()
        {
            var server = Services.GetRequiredService<IServerConnection>();
            var testingScenarioBuilder = new TestingScenarioBuilder(DbContext);
            var mockedPlayers = await testingScenarioBuilder.BuildScenarioWith3ConnectedPlayers(server);

            var sut = Services.GetRequiredService<IGameService>();
            await sut.PlayerLeaves(mockedPlayers.First().Object.Id);

            foreach (var mock in mockedPlayers)
            {
                mock.Verify(
                    x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("has left #general"))),
                    Times.Once);
            }

            server.Clear();
        }

        [Fact]
        public async Task ShouldSendMessageToGeneralRoomPlayers()
        {
            var server = Services.GetRequiredService<IServerConnection>();
            var testingScenarioBuilder = new TestingScenarioBuilder(DbContext);
            var mockedPlayers = await testingScenarioBuilder.BuildScenarioWith3ConnectedPlayers(server);

            var sut = Services.GetRequiredService<IGameService>();
            _ = await sut.NewCommand(mockedPlayers.First().Object.Id, "hi!");

            foreach (var mock in mockedPlayers)
            {
                mock.Verify(x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("hi!"))), Times.Once);
            }

            server.Clear();
        }

        [Fact]
        public async Task ShouldReturnExitCommand()
        {
            var server = Services.GetRequiredService<IServerConnection>();
            var testingScenarioBuilder = new TestingScenarioBuilder(DbContext);
            var mockedPlayers = await testingScenarioBuilder.BuildScenarioWith3ConnectedPlayers(server);

            var sut = Services.GetRequiredService<IGameService>();
            bool exitCommand = await sut.NewCommand(mockedPlayers.First().Object.Id, @"\exit");

            Assert.True(exitCommand);
            foreach (var mock in mockedPlayers)
            {
                mock.Verify(x => x.Send(It.IsAny<ChatMessage>()), Times.Never);
            }

            server.Clear();
        }

        [Fact]
        public async Task ShouldReturnListOfGameRooms()
        {
            var server = Services.GetRequiredService<IServerConnection>();
            var testingScenarioBuilder = new TestingScenarioBuilder(DbContext);
            var mockedPlayers = await testingScenarioBuilder.BuildScenarioWith3ConnectedPlayers(server);

            var sut = Services.GetRequiredService<IGameService>();
            _ = await sut.NewCommand(mockedPlayers.First().Object.Id, @"\list");

            mockedPlayers.First().Verify(
                x => x.Send(It.Is<ChatServerResponse>(x => x.Type == ResponseType.ListGameRooms)),
                Times.Once);

            server.Clear();
        }
    }
}
