using Sketch.DTOs;
using Sketch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [Fact]
        public void ShouldComputePointsWithPlayerLeavingInTheMiddle()
        {
            var response = GameResponse.EndOfRound(CreateRoundWithPlayerLeaving());

            Assert.Equal(new Dictionary<string, int>
            {
                { "user2", 15 },
                { "user1", 10 }
            }, ((RankingViewModel[])response.Details)[0].Results);
        }

        [Fact]
        public void ShouldComputePointsWithPlayerEnteringInTheMiddle()
        {
            var response = GameResponse.EndOfRound(CreateRoundWithPlayerEntering());

            Assert.Equal(new Dictionary<string, int>
            {
                { "user2", 15 },
                { "user1", 10 },
                { "user3", 2 }
            }, ((RankingViewModel[])response.Details)[0].Results);
        }

        private Round CreateRoundWithPlayerLeaving()
        {
            var round = CreateRound();
            round.Turns.First().PlayersTurns.Add(new PlayerTurn
            {
                Player = new Player { Username = "user3" },
                Points = 2
            });
            return round;
        }

        private Round CreateRoundWithPlayerEntering()
        {
            var round = CreateRound();
            round.Turns.Last().PlayersTurns.Add(new PlayerTurn
            {
                Player = new Player { Username = "user3" },
                Points = 2
            });
            return round;
        }

        private static Round CreateRound()
        {
            return new Round
            {
                Turns = new List<Turn>
                {
                    new Turn
                    {
                        StartTimestamp = new DateTime(2020, 11, 1, 10, 00, 00),
                        PlayersTurns = new List<PlayerTurn>
                        {
                            new PlayerTurn
                            {
                                Player = new Player {Username = "user1" },
                                Points = 10
                            },
                            new PlayerTurn
                            {
                                Player = new Player { Username = "user2" },
                                Points = 5
                            }
                        }
                    },
                    new Turn
                    {
                        StartTimestamp = new DateTime(2020, 11, 1, 10, 01, 00),
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
