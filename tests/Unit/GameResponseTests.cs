using Sketch.DTOs;
using Sketch.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Tests.Unit
{
    public class GameResponseTests
    {
        [Fact]
        public void ShouldComputePoints()
        {
            var response = GameResponse.EndOfRound(CreateRound());

            Assert.Equal(new Dictionary<string, int>
            {
                { "user2", 15 },
                { "user1", 10 }
            }, ((RankingViewModel[])response.Details)[0].Results);
        }

        private static Round CreateRound()
        {
            return new Round
            {
                Turns = new List<Turn>
                {
                    new Turn
                    {
                        PlayersTurns = new List<PlayerTurn>
                        {
                            new PlayerTurn
                            {
                                Player = new Player {Username = "user1"},
                                Points = 10
                            },
                            new PlayerTurn
                            {
                                Player = new Player {Username = "user2"},
                                Points = 5
                            }
                        }
                    },
                    new Turn
                    {
                        PlayersTurns = new List<PlayerTurn>
                        {
                            new PlayerTurn
                            {
                                Player = new Player {Username = "user1"},
                                Points = 0
                            },
                            new PlayerTurn
                            {
                                Player = new Player {Username = "user2"},
                                Points = 10
                            }
                        }
                    }
                }
            };
        }
    }
}
