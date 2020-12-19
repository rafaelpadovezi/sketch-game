using Sketch.Business;
using Sketch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests.Unit
{
    public class SketchGameTests
    {
        [Fact]
        public void ShouldInitGameRoundAndTurn()
        {
            var player1 = new Player { Id = Guid.NewGuid() };
            var player2 = new Player { Id = Guid.NewGuid() };

            var (round, turn) = SketchGame.NewRound(
                new Word { Content = "Test" },
                new Player[] { player1, player2 });

            Assert.Equal(1, round.Count);
            var existingPlayerTurn = turn.PlayersTurns.ElementAt(0);
            var newPlayerTurn = turn.PlayersTurns.ElementAt(1);
            Assert.True(existingPlayerTurn.IsDrawing);
            Assert.False(newPlayerTurn.IsDrawing);
        }

        [Fact]
        public void ShouldStartNextTurn()
        {
            var player1 = new Player { Id = Guid.NewGuid() };
            var player2 = new Player { Id = Guid.NewGuid() };
            var round = new Round
            {
                Turns = new List<Turn>
                {
                    new Turn { DrawingPlayerId = player1.Id }
                }
            };

            var turn = SketchGame.NextTurn(
                round,
                new Word { Content = "Test" },
                new Player[] { player1, player2 });

            Assert.Equal(2, round.Count);
            var player1Turn = turn.PlayersTurns.ElementAt(0);
            var player2Turn = turn.PlayersTurns.ElementAt(1);
            Assert.False(player1Turn.IsDrawing);
            Assert.True(player2Turn.IsDrawing);
        }

        [Fact]
        public void ShouldGuessWrongWord()
        {
            var turn = new Turn { Id = Guid.NewGuid() };
            Player player = new Player
            {
                Id = Guid.NewGuid(),
                PlayerTurns = new List<PlayerTurn>
                {
                    new PlayerTurn { TurnId = turn.Id },
                    new PlayerTurn { TurnId = Guid.NewGuid() }
                }
            };

            bool hit = SketchGame.GuessWord(player, "guess", turn);

            Assert.False(hit);
            Assert.Null(player.PlayerTurns.First().Points);
        }

        [Fact]
        public void ShouldGuessCorrectly()
        {
            var turn = new Turn
            {
                Id = Guid.NewGuid(),
                Word = new Word
                {
                    Content = "Right guess"
                }
            };
            Player player = new Player
            {
                Id = Guid.NewGuid(),
                PlayerTurns = new List<PlayerTurn>
                {
                    new PlayerTurn { TurnId = turn.Id },
                    new PlayerTurn { TurnId = Guid.NewGuid() }
                }
            };

            bool hit = SketchGame.GuessWord(player, "right guess", turn);

            Assert.True(hit);
            Assert.Equal(SketchGame.BasePointsHit, player.PlayerTurns.First().Points);
        }

        [Fact]
        public void ShouldGuessCorrectlySeconds()
        {
            var turn = new Turn
            {
                Id = Guid.NewGuid(),
                Word = new Word
                {
                    Content = "Right guess"
                }
            };
            Player player = new Player
            {
                Id = Guid.NewGuid(),
                PlayerTurns = new List<PlayerTurn>
                {
                    new PlayerTurn { TurnId = turn.Id },
                    new PlayerTurn { TurnId = Guid.NewGuid() }
                }
            };
            turn.PlayersTurns = new List<PlayerTurn>
            {
                new PlayerTurn { PlayerId = player.Id },
                new PlayerTurn { PlayerId = Guid.NewGuid(), IsDrawing = true },
                new PlayerTurn { PlayerId = Guid.NewGuid(), Points = SketchGame.BasePointsDrawing }
            };

            bool hit = SketchGame.GuessWord(player, "right guess", turn);

            Assert.True(hit);
            Assert.Equal(SketchGame.BasePointsHit - 1, player.PlayerTurns.First().Points);
        }

        [Fact]
        public void ShouldGuessCorrectlyAfter6Plyers()
        {
            var turn = new Turn
            {
                Id = Guid.NewGuid(),
                Word = new Word
                {
                    Content = "Right guess"
                }
            };
            Player player = new Player
            {
                Id = Guid.NewGuid(),
                PlayerTurns = new List<PlayerTurn>
                {
                    new PlayerTurn { TurnId = turn.Id },
                    new PlayerTurn { TurnId = Guid.NewGuid() }
                }
            };
            turn.PlayersTurns = new List<PlayerTurn>
            {
                new PlayerTurn { PlayerId = player.Id },
                new PlayerTurn { PlayerId = Guid.NewGuid(), IsDrawing = true },
                new PlayerTurn { PlayerId = Guid.NewGuid(), Points = SketchGame.BasePointsDrawing },
                new PlayerTurn { PlayerId = Guid.NewGuid(), Points = SketchGame.BasePointsDrawing },
                new PlayerTurn { PlayerId = Guid.NewGuid(), Points = SketchGame.BasePointsDrawing },
                new PlayerTurn { PlayerId = Guid.NewGuid(), Points = SketchGame.BasePointsDrawing },
                new PlayerTurn { PlayerId = Guid.NewGuid(), Points = SketchGame.BasePointsDrawing },
                new PlayerTurn { PlayerId = Guid.NewGuid(), Points = SketchGame.BasePointsDrawing }
            };

            bool hit = SketchGame.GuessWord(player, "right guess", turn);

            Assert.True(hit);
            Assert.Equal(5, player.PlayerTurns.First().Points);
        }

        [Fact]
        public void ShouldCalculateDrawingPointsNoHits()
        {
            var drawingPlayer = new PlayerTurn
            {
                IsDrawing = true
            };
            var points = SketchGame.CalculateDrawingPoints(new List<PlayerTurn>
            {
                drawingPlayer,
                new PlayerTurn
                {
                    IsDrawing = false
                }
            });

            Assert.Equal(0, points);
        }

        [Fact]
        public void ShouldCalculateDrawingPointsWith1Hit()
        {
            var drawingPlayer = new PlayerTurn
            {
                IsDrawing = true
            };
            var points = SketchGame.CalculateDrawingPoints(new List<PlayerTurn>
            {
                drawingPlayer,
                new PlayerTurn
                {
                    IsDrawing = false,
                    Points = SketchGame.BasePointsHit
                }
            });

            Assert.Equal(SketchGame.BasePointsDrawing, points);
        }

        [Fact]
        public void ShouldCalculateDrawingPointsWith2Hits()
        {
            var drawingPlayer = new PlayerTurn
            {
                IsDrawing = true
            };
            var points = SketchGame.CalculateDrawingPoints(new List<PlayerTurn>
            {
                drawingPlayer,
                new PlayerTurn
                {
                    IsDrawing = false,
                    Points = SketchGame.BasePointsHit
                },
                new PlayerTurn
                {
                    IsDrawing = false,
                    Points = SketchGame.BasePointsHit - 1
                }
            });

            Assert.Equal(SketchGame.BasePointsDrawing + 1, points);
        }

        [Fact]
        public void ShouldCalculateDrawingPointsWith7Hits()
        {
            var drawingPlayer = new PlayerTurn
            {
                IsDrawing = true
            };
            var playerTurns = Enumerable.Range(SketchGame.BasePointsHit, 7)
                .Select(x => new PlayerTurn { IsDrawing = false, Points = x })
                .ToList();
            playerTurns.Add(drawingPlayer);

            var points = SketchGame.CalculateDrawingPoints(playerTurns);

            Assert.Equal(SketchGame.BasePointsDrawing + 5, points);
        }
    }
}
