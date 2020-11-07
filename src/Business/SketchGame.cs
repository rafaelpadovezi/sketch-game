using Sketch.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sketch.Business
{
    public class SketchGame
    {
        public List<Round> InitGame(Player player, GameRoom gameroom, Word word)
        {
            return new List<Round>
            {
                new Round
                {
                    Count = 1,
                    Turns = new List<Turn>
                    {
                        new Turn
                        {
                            StartTimestamp = DateTime.Now,
                            DrawingPlayerId = gameroom.Players.First().Id,
                            Word = word,
                            PlayersTurns = new List<PlayerTurn>
                            {
                                new PlayerTurn
                                {
                                    IsDrawing = true,
                                    PlayerId = gameroom.Players.First().Id
                                },
                                new PlayerTurn
                                {
                                    IsDrawing = false,
                                    PlayerId = player.Id
                                }
                            }
                        }
                    },
                }
            };
        }
    }
}
