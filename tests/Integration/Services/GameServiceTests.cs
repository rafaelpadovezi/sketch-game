using Microsoft.Extensions.DependencyInjection;
using Moq;
using Sketch.Business;
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
        private readonly IServiceScope _scope;
        private readonly IGameService _sut;

        public GameServiceTests()
        {
            _scenario = new TestingScenarioBuilder(DbContext, Server);
            _scope = Services.CreateScope();
            _sut = _scope.ServiceProvider.GetRequiredService<IGameService>();
        }

        void IDisposable.Dispose()
        {
            Server.Clear();
            _scope.Dispose();
        }

        [Fact]
        public async Task ShouldSendNewPlayerMessageToGeneralRoomPlayers()
        {
            var mockNewPlayer = _scenario.MockPlayer1InGeneral;

            await _sut.NewPlayer(mockNewPlayer.Object.Id);

            mockNewPlayer.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("has joined #general"))), Times.Once);
            _scenario.MockPlayer2InGeneral.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("has joined #general"))), Times.Once);
        }

        [Fact]
        public async Task ShouldSendPlayerLeftMessageToGeneralRoomPlayers()
        {
            var mockPlayerLeaving = _scenario.MockPlayer1InGeneral;

            await _sut.PlayerLeaves(mockPlayerLeaving.Object.Id);

            mockPlayerLeaving.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("has left #general"))), Times.Once);
            _scenario.MockPlayer2InGeneral.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("has left #general"))), Times.Once);
        }

        [Fact]
        public async Task ShouldSendMessageToGeneralRoomPlayers()
        {
            var mockPlayerSendsMessage = _scenario.MockPlayer1InGeneral;

            _ = await _sut.NewCommand(mockPlayerSendsMessage.Object.Id, "hi!");

            mockPlayerSendsMessage.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("hi!"))), Times.Once);
            _scenario.MockPlayer2InGeneral.Verify(
                x => x.Send(It.Is<ChatMessage>(m => m.Message.Contains("hi!"))), Times.Once);
        }

        [Fact]
        public async Task ShouldReturnExitCommand()
        {
            var mockPlayerSendsExitCommand = _scenario.MockPlayer1InGeneral;

            bool exitCommand = await _sut.NewCommand(mockPlayerSendsExitCommand.Object.Id, @"\exit");

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

            _ = await _sut.NewCommand(mockPlayerSendsListCommand.Object.Id, @"\list");

            mockPlayerSendsListCommand.Verify(
                x => x.Send(It.Is<ChatServerResponse>(x => x.Type == ResponseType.ListGameRooms)), Times.Once);
        }

        [Fact]
        public async Task ShouldEnterGameRoom()
        {
            var mockPlayerEnterGameRoom = _scenario.MockPlayer1InGeneral;
            var gameRoom = _scenario.GameRoomWith2Players;

            _ = await _sut.NewCommand(mockPlayerEnterGameRoom.Object.Id, $@"\c {gameRoom.Name}");

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

            _ = await _sut.NewCommand(mockPlayerSendMessage.Object.Id, "hey all");

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

            _ = await _sut.NewCommand(mockPlayerLeaveGameRoom.Object.Id, @"\c general");

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
            var mockExistingPlayer = _scenario.MockPlayerAloneInGameRoom;
            string gameRoom = _scenario.GameRoomWith1Player.Name;

            _ = await _sut.NewCommand(mockNewPlayer.Object.Id, $@"\c {gameRoom}");

            var gameroom = DbContext.GameRooms.Single(x => x.Name == gameRoom);
            Assert.Single(gameroom.Rounds);
            mockExistingPlayer
                .Verify(x =>
                    x.Send(It.Is<GameResponse>(r => r.Message.Contains("Start drawing!"))),
                    Times.Once);
            mockNewPlayer
                .Verify(x =>
                    x.Send(It.Is<GameResponse>(r => r.Message.Contains($"is drawing"))),
                    Times.Once);
        }

        [Fact]
        public async Task ShouldEndTurnWhenTimeIsUp()
        {
            var mockNewPlayer = _scenario.MockPlayer1InGeneral;
            var mockExistingPlayer = _scenario.MockPlayerAloneInGameRoom;
            string gameRoom = _scenario.GameRoomWith1Player.Name;
            var gameCycle = (GameLifeCycle)_scope.ServiceProvider.GetService<IGameLifeCycle>();
            gameCycle.TurnDuration = 10;
            gameCycle.AutoCreateNewTurn = false;

            _ = await _sut.NewCommand(mockNewPlayer.Object.Id, $@"\c {gameRoom}");
            await Task.Delay(2000); // Wait end turn and hope this works

            mockExistingPlayer
                .Verify(x =>
                    x.Send(It.Is<GameResponse>(r => r.Type == ResponseType.EndOfTurn)),
                    Times.Once);
            mockNewPlayer
                .Verify(x =>
                    x.Send(It.Is<GameResponse>(r => r.Type == ResponseType.EndOfTurn)),
                    Times.Once);
        }

        [Fact]
        public async Task ShouldEndTurnWhenPlayerGuessCorrectly()
        {
            var drawingPlayer = _scenario.MockPlayer1InGameRoom;
            var guessingPlayer = _scenario.MockPlayer2InGameRoom;
            string gameRoom = _scenario.GameRoomWith2Players.Name;

            _ = await _sut.NewCommand(guessingPlayer.Object.Id, $@"TestWord");

            guessingPlayer
                .Verify(x =>
                    x.Send(It.Is<GameResponse>(r => r.Type == ResponseType.EndOfTurn)),
                    Times.Once);
            drawingPlayer
                .Verify(x =>
                    x.Send(It.Is<GameResponse>(r => r.Type == ResponseType.EndOfTurn)),
                    Times.Once);
        }

        [Fact]
        public async Task ShouldStartTurnAfterLastTurn()
        {
            var enteringPlayer = _scenario.MockPlayer1InGeneral;
            var existingPlayer = _scenario.MockPlayerAloneInGameRoom;
            string gameRoom = _scenario.GameRoomWith1Player.Name;

            _ = await _sut.NewCommand(enteringPlayer.Object.Id, $@"\c {gameRoom}");
            _ = await _sut.NewCommand(enteringPlayer.Object.Id, $@"TestWord");
            var gameCycle = (GameLifeCycle)_scope.ServiceProvider.GetService<IGameLifeCycle>();
            await gameCycle.CheckEndedTurns();

            var gameroom = DbContext.GameRooms.Single(x => x.Name == gameRoom);
            Assert.Equal(2, gameroom.Rounds.Single().Turns.Count);
            enteringPlayer
                .Verify(x =>
                    x.Send(It.Is<GameResponse>(r => r.Message.Contains("Start drawing!"))),
                    Times.Once);
            existingPlayer
                .Verify(x =>
                    x.Send(It.Is<GameResponse>(r => r.Message.Contains($"is drawing"))),
                    Times.Once);
        }
    }
}
