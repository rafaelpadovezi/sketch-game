using Infrastructure.Database;
using Moq;
using Sketch.Infrastructure.Connection;
using Sketch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.Support
{
    internal class TestingScenarioBuilder
    {
        private readonly SketchDbContext _context;

        public GameRoom EmptyGameRoom { get; private set; }
        public GameRoom GameRoomWith1Player { get; private set; }
        public GameRoom GameRoomWith2Players { get; private set; }
        public Mock<IPlayerConnection> MockPlayer1InGeneral { get; private set; }
        public Mock<IPlayerConnection> MockPlayer2InGeneral { get; private set; }
        public Mock<IPlayerConnection> MockPlayer1InGameRoom { get; private set; }
        public Mock<IPlayerConnection> MockPlayer2InGameRoom { get; private set; }
        public Mock<IPlayerConnection> MockPlayerAloneInGameRoom { get; private set; }

        public TestingScenarioBuilder(SketchDbContext context, IServerConnection server)
        {
            _context = context;

            var player1InGeneral = new Player { Username = "player1InGeneral" };
            var player2InGeneral = new Player { Username = "player2InGeneral" };
            var player1InGameRoom = new Player { Username = "player1InGameRoom" };
            var player2InGameRoom = new Player { Username = "player2InGameRoom" };
            var playerAloneInGameRoom = new Player { Username = "player1InGameRoom" };
            _context.AddRange(player1InGeneral, player2InGeneral, player1InGameRoom, player2InGameRoom, playerAloneInGameRoom);
            EmptyGameRoom = new GameRoom
            {
                Name = "emptyGameRoom",
                Players = new List<Player> { }
            };
            GameRoomWith1Player = new GameRoom
            {
                Name = "gameRoom1Player",
                Players = new List<Player> { playerAloneInGameRoom }
            };
            GameRoomWith2Players = new GameRoom
            {
                Name = "gameRoomWith2Players",
                Players = new List<Player> { player1InGameRoom, player2InGameRoom },
                Rounds = new List<Round>
                {
                    new Round
                    {
                        Turns = new List<Turn>
                        {
                            new Turn
                            {
                                PlayersTurns = new List<PlayerTurn>
                                {
                                    new PlayerTurn
                                    {
                                        PlayerId = player1InGameRoom.Id,
                                        IsDrawing = true
                                    },
                                    new PlayerTurn
                                    {
                                        PlayerId = player2InGameRoom.Id,
                                        IsDrawing = false
                                    }
                                }
                            }
                        }
                    }
                }
            };
            _context.AddRange(EmptyGameRoom, GameRoomWith1Player, GameRoomWith2Players);
            _context.SaveChanges();

            MockPlayer1InGeneral = CreateMockPlayerAndAddToServer(player1InGeneral, server);
            MockPlayer2InGeneral = CreateMockPlayerAndAddToServer(player2InGeneral, server);
            MockPlayer1InGameRoom = CreateMockPlayerAndAddToServer(player1InGameRoom, server);
            MockPlayer2InGameRoom = CreateMockPlayerAndAddToServer(player2InGameRoom, server);
            MockPlayerAloneInGameRoom = CreateMockPlayerAndAddToServer(playerAloneInGameRoom, server);
        }

        private static Mock<IPlayerConnection> CreateMockPlayerAndAddToServer(Player player, IServerConnection server)
        {
            var mock = new Mock<IPlayerConnection>();
            mock.Setup(m => m.Id).Returns(player.Id);
            mock.Setup(m => m.IsConnected).Returns(true);

            server.AddPlayerConnection(mock.Object);

            return mock;
        }
    }
}
