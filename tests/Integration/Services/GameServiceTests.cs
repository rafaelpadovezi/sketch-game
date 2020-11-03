using Microsoft.Extensions.DependencyInjection;
using Moq;
using Sketch.DTOs;
using Sketch.Infrastructure.Connection;
using Sketch.Services;
using System;
using System.Linq;
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

        [Fact]
        public async Task ShouldEnterGameRoom()
        {
            var server = Services.GetRequiredService<IServerConnection>();
            var testingScenarioBuilder = new TestingScenarioBuilder(DbContext);
            var mockPlayers = await testingScenarioBuilder.BuildScenarioWith3PlayersAndGameRoom(server);

            var sut = Services.GetRequiredService<IGameService>();
            _ = await sut.NewCommand(mockPlayers[0].Object.Id, @"\c gameroom1");

            var gameroom = DbContext.GameRooms.Single(x => x.Name == "gameroom1");
            Assert.Contains(mockPlayers[0].Object.Id, gameroom.Players.Select(x => x.Id));
            mockPlayers[0].Verify(
                x => x.Send(It.Is<ChatServerResponse>(x => x.Type == ResponseType.EnterGameRoom)), Times.Once);
            mockPlayers[0].Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("has joined #gameroom1"))), Times.Once);
            mockPlayers[1].Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("has joined #gameroom1"))), Times.Once);
            mockPlayers[2].Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("has joined #gameroom1"))), Times.Once);

            server.Clear();
        }

        [Fact]
        public async Task ShouldSendMessageToGameRoomPlayers()
        {
            var server = Services.GetRequiredService<IServerConnection>();
            var testingScenarioBuilder = new TestingScenarioBuilder(DbContext);
            var mockPlayers = (await testingScenarioBuilder.BuildScenarioWith3PlayersAndGameRoom(server)).ToList();

            var sut = Services.GetRequiredService<IGameService>();
            _ = await sut.NewCommand(mockPlayers[1].Object.Id, "hey all");

            mockPlayers[0].Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("hey all"))), Times.Never);
            mockPlayers[1].Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("hey all"))), Times.Once);
            mockPlayers[2].Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("hey all"))), Times.Once);

            server.Clear();
        }

        [Fact]
        public async Task ShouldLeaveGameRoom()
        {
            var server = Services.GetRequiredService<IServerConnection>();
            var testingScenarioBuilder = new TestingScenarioBuilder(DbContext);
            var mockPlayers = (await testingScenarioBuilder.BuildScenarioWith3PlayersAndGameRoom(server)).ToList();

            var sut = Services.GetRequiredService<IGameService>();
            _ = await sut.NewCommand(mockPlayers[1].Object.Id, @"\c general");

            var gameroom = DbContext.GameRooms.Single(x => x.Name == "gameroom1");
            Assert.DoesNotContain(mockPlayers[1].Object.Id, gameroom.Players.Select(x => x.Id));

            server.Clear();
        }
    }
}
