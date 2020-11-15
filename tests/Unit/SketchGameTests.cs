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
                new Word { Content = "Test" }, player1, new Player[] { player1, player2 });

            Assert.Equal(1, round.Count);
            var existingPlayerTurn = turn.PlayersTurns.ElementAt(0);
            var newPlayerTurn = turn.PlayersTurns.ElementAt(1);
            Assert.True(existingPlayerTurn.IsDrawing);
            Assert.False(newPlayerTurn.IsDrawing);
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
    }
}
