using Microsoft.Extensions.Logging;
using Moq;
using Sketch.Business;
using Sketch.Infrastructure.Connection;
using Sketch.Infrastructure.Database.Repositories.Interfaces;
using Sketch.Models;
using Sketch.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Unit
{
    public class RoundServiceTests
    {
        [Fact]
        public async Task ShouldCreateNewRound()
        {
            var mockWordService = new Mock<IWordService>();
            mockWordService
                .Setup(x => x.PickWord(It.IsAny<GameRoomType>()))
                .ReturnsAsync(new Word { Content = "Test" });
            var mockServer = new Mock<IServerConnection>();
            var mockGameCycleService = new Mock<IGameLifeCycle>();
            var roundService = new RoundService(
                Mock.Of<IGameRoomRepository>(),
                Mock.Of<IPlayerRepository>(),
                mockWordService.Object,
                mockServer.Object,
                mockGameCycleService.Object,
                Mock.Of<ILogger<RoundService>>());

            var player1 = new Player { Id = Guid.NewGuid() };
            var player2 = new Player { Id = Guid.NewGuid() };
            var gameRoom = new GameRoom
            {
                Players = new List<Player>
                {
                    player1, player2
                }
            };
            await roundService.StartRound(gameRoom);

            var turn = gameRoom.Rounds.Single().Turns.Single();
            var existingPlayerTurn = turn.PlayersTurns.ElementAt(0);
            var newPlayerTurn = turn.PlayersTurns.ElementAt(1);
            Assert.True(existingPlayerTurn.IsDrawing);
            Assert.False(newPlayerTurn.IsDrawing);
        }
    }
}
