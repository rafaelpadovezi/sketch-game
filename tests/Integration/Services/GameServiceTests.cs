using Microsoft.Extensions.DependencyInjection;
using Moq;
using Sketch.DTOs;
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
        private readonly TestingScenarioBuilder _scenario;

        public GameServiceTests()
        {
            _scenario = new TestingScenarioBuilder(DbContext, Server);
        }

        void IDisposable.Dispose()
        {
            Server.Clear();
        }

        [Fact]
        public async Task ShouldSendNewPlayerMessageToGeneralRoomPlayers()
        {
            var mockNewPlayer = _scenario.MockPlayer1InGeneral;

            var sut = Services.GetRequiredService<IGameService>();
            await sut.NewPlayer(mockNewPlayer.Object.Id);

            mockNewPlayer.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("has joined #general"))), Times.Once);
            _scenario.MockPlayer2InGeneral.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("has joined #general"))), Times.Once);
        }

        [Fact]
        public async Task ShouldSendPlayerLeftMessageToGeneralRoomPlayers()
        {
            var mockPlayerLeaving = _scenario.MockPlayer1InGeneral;

            var sut = Services.GetRequiredService<IGameService>();
            await sut.PlayerLeaves(mockPlayerLeaving.Object.Id);

            mockPlayerLeaving.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("has left #general"))), Times.Once);
            _scenario.MockPlayer2InGeneral.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("has left #general"))), Times.Once);
        }

        [Fact]
        public async Task ShouldSendMessageToGeneralRoomPlayers()
        {
            var mockPlayerSendsMessage = _scenario.MockPlayer1InGeneral;

            var sut = Services.GetRequiredService<IGameService>();
            _ = await sut.NewCommand(mockPlayerSendsMessage.Object.Id, "hi!");

            mockPlayerSendsMessage.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("hi!"))), Times.Once);
            _scenario.MockPlayer2InGeneral.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("hi!"))), Times.Once);
        }

        [Fact]
        public async Task ShouldReturnExitCommand()
        {
            var mockPlayerSendsExitCommand = _scenario.MockPlayer1InGeneral;

            var sut = Services.GetRequiredService<IGameService>();
            bool exitCommand = await sut.NewCommand(mockPlayerSendsExitCommand.Object.Id, @"\exit");

            Assert.True(exitCommand);
            mockPlayerSendsExitCommand.Verify(
                x => x.Send(It.IsAny<ChatMessage>()), Times.Never);
            _scenario.MockPlayer2InGeneral.Verify(
                x => x.Send(It.IsAny<ChatMessage>()), Times.Never);
        }

        [Fact]
        public async Task ShouldReturnListOfGameRooms()
        {
            var mockPlayerSendsListCommand = _scenario.MockPlayer1InGeneral;

            var sut = Services.GetRequiredService<IGameService>();
            _ = await sut.NewCommand(mockPlayerSendsListCommand.Object.Id, @"\list");

            mockPlayerSendsListCommand.Verify(
                x => x.Send(It.Is<ChatServerResponse>(x => x.Type == ResponseType.ListGameRooms)), Times.Once);
        }

        [Fact]
        public async Task ShouldEnterGameRoom()
        {
            var mockPlayerEnterGameRoom = _scenario.MockPlayer1InGeneral;
            var gameRoom = _scenario.GameRoomWith2Players;

            var sut = Services.GetRequiredService<IGameService>();
            _ = await sut.NewCommand(mockPlayerEnterGameRoom.Object.Id, $@"\c {gameRoom.Name}");

            var gameroom = DbContext.GameRooms.Single(x => x.Name == gameRoom.Name);
            Assert.Contains(mockPlayerEnterGameRoom.Object.Id, gameroom.Players.Select(x => x.Id));
            mockPlayerEnterGameRoom.Verify(
                x => x.Send(It.Is<ChatServerResponse>(x => x.Type == ResponseType.EnterGameRoom)), Times.Once);
            _scenario.MockPlayer1InGameRoom.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains($"has joined #{gameRoom.Name}"))), Times.Once);
            _scenario.MockPlayer2InGameRoom.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains($"has joined #{gameRoom.Name}"))), Times.Once);
            _scenario.MockPlayer2InGeneral.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("hey all"))), Times.Never);
        }

        [Fact]
        public async Task ShouldSendMessageToGameRoomPlayers()
        {
            var mockPlayerSendMessage = _scenario.MockPlayer1InGameRoom;
            var gameRoom = _scenario.GameRoomWith2Players;

            var sut = Services.GetRequiredService<IGameService>();
            _ = await sut.NewCommand(mockPlayerSendMessage.Object.Id, "hey all");

            mockPlayerSendMessage.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("hey all"))), Times.Once);
            _scenario.MockPlayer2InGameRoom.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("hey all"))), Times.Once);
            _scenario.MockPlayer1InGeneral.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("hey all"))), Times.Never);
        }

        [Fact]
        public async Task ShouldLeaveGameRoom()
        {
            var mockPlayerLeaveGameRoom = _scenario.MockPlayer1InGameRoom;
            var gameRoom = _scenario.GameRoomWith2Players;

            var sut = Services.GetRequiredService<IGameService>();
            _ = await sut.NewCommand(mockPlayerLeaveGameRoom.Object.Id, @"\c general");

            var gameroom = DbContext.GameRooms.Single(x => x.Name == gameRoom.Name);
            Assert.DoesNotContain(mockPlayerLeaveGameRoom.Object.Id, gameroom.Players.Select(x => x.Id));
            mockPlayerLeaveGameRoom.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("has joined #general"))), Times.Once);
            _scenario.MockPlayer1InGeneral.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("has joined #general"))), Times.Once);
            _scenario.MockPlayer2InGameRoom.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("went to #general"))), Times.Once);
            _scenario.MockPlayer2InGameRoom.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("has joined #general"))), Times.Never);
        }

        [Fact]
        public async Task ShouldStartTurnWhenPlayerEnterGameRoomWith1Player()
        {
            var mockNewPlayer = _scenario.MockPlayer1InGeneral;
            string gameRoom = _scenario.GameRoomWith1Player.Name;
            var existingPlayerId = _scenario.GameRoomWith1Player.Players.Single().Id;

            var sut = Services.GetRequiredService<IGameService>();
            _ = await sut.NewCommand(mockNewPlayer.Object.Id, $@"\c {gameRoom}");

            var gameroom = DbContext.GameRooms.Single(x => x.Name == gameRoom);
            var turn = gameroom.Rounds.Single().Turns.Single();
            var existingPlayerTurn = turn.PlayersTurns.ElementAt(0);
            var newPlayerTurn = turn.PlayersTurns.ElementAt(1);
            Assert.True(existingPlayerTurn.IsDrawing);
            Assert.Equal(existingPlayerId, existingPlayerTurn.PlayerId);
            Assert.False(newPlayerTurn.IsDrawing);
            Assert.Equal(mockNewPlayer.Object.Id, newPlayerTurn.PlayerId);
        }
    }
}
