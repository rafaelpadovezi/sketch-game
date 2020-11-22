using Sketch.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Tests.Unit
{
    public class ModelExtensionsTests
    {
        [Fact]
        public void ShouldBeCompleteIfAllPlayersHaveDraw()
        {
            var players = new List<Player>
            {
                new Player { Id = Guid.NewGuid() },
                new Player { Id = Guid.NewGuid() },
                new Player { Id = Guid.NewGuid() }
            };
            var round = new Round
            {
                Turns = new List<Turn>
                {
                    new Turn
                    {
                        EndTimestamp = DateTime.Now,
                        DrawingPlayerId = players[0].Id
                    },
                    new Turn
                    {
                        EndTimestamp = DateTime.Now,
                        DrawingPlayerId = players[1].Id
                    },
                    new Turn
                    {
                        EndTimestamp = DateTime.Now,
                        DrawingPlayerId = players[2].Id
                    },
                }
            };

            Assert.True(round.IsComplete(players));
        }

        [Fact]
        public void ShouldBeCompleteIfAllPlayersHaveDrawEvenIfSomeoneLeft()
        {
            var playerWhoLeft = new Player { Id = Guid.NewGuid() };
            var players = new List<Player>
            {
                new Player { Id = Guid.NewGuid() },
                new Player { Id = Guid.NewGuid() }
            };
            var round = new Round
            {
                Turns = new List<Turn>
                {
                    new Turn
                    {
                        EndTimestamp = DateTime.Now,
                        DrawingPlayerId = playerWhoLeft.Id
                    },
                    new Turn
                    {
                        EndTimestamp = DateTime.Now,
                        DrawingPlayerId = players[0].Id
                    },
                    new Turn
                    {
                        EndTimestamp = DateTime.Now,
                        DrawingPlayerId = players[1].Id
                    },
                }
            };

            Assert.True(round.IsComplete(players));
        }

        [Fact]
        public void ShouldBeInCompleteIfAtLeastOnePlayerHasntDraw()
        {
            var players = new List<Player>
            {
                new Player { Id = Guid.NewGuid() },
                new Player { Id = Guid.NewGuid() },
                new Player { Id = Guid.NewGuid() },
                new Player { Id = Guid.NewGuid() }
            };
            var round = new Round
            {
                Turns = new List<Turn>
                {
                    new Turn
                    {
                        EndTimestamp = DateTime.Now,
                        DrawingPlayerId = players[0].Id
                    },
                    new Turn
                    {
                        EndTimestamp = DateTime.Now,
                        DrawingPlayerId = players[1].Id
                    },
                    new Turn
                    {
                        EndTimestamp = DateTime.Now,
                        DrawingPlayerId = players[2].Id
                    },
                }
            };

            Assert.False(round.IsComplete(players));
        }
    }
}
