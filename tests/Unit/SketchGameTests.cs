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
            Assert.Equal(10, player.PlayerTurns.First().Points);
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
        public void ShouldCalculateDrawingPointsWithHits()
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
                    Points = 10
                }
            });

            Assert.Equal(10, points);
        }
    }
}
