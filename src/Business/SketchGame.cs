using Microsoft.EntityFrameworkCore;
using Sketch.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sketch.Business
{
    public class SketchGame
    {
        public static (Round round, Turn turn) NewRound(
            Word word,
            IEnumerable<Player> players)
        {
            var drawingPlayer = players.First();
            var turn = StartTurn(word, drawingPlayer, players);
            var round = new Round();
            round.Turns.Add(turn);
            return (round, turn);
        }

        public static bool GuessWord(Player player, string guess, Turn turn)
        {
            var playerTurn = player.PlayerTurns.Single(x => x.TurnId == turn.Id);
            if (guess.ToLowerInvariant() == turn.Word.Content.ToLowerInvariant())
            {
                playerTurn.Points = 10;
            }

            return playerTurn.Hit;
        }

        public bool IsRoundFinished(GameRoom gameRoom, Round round)
        {
            return round.Turns
                .Select(x => x.DrawingPlayerId)
                .All(gameRoom.Players.Select(x => x.Id).Contains);
        }

        public static int CalculateDrawingPoints(IEnumerable<PlayerTurn> playerTurns)
        {
            bool gotHits = playerTurns.Where(x => !x.IsDrawing).Any(x => x.Hit);

            return gotHits ? 10 : 0;
        }

        public static Turn NextTurn(
            Round round,
            Word word,
            IEnumerable<Player> players)
        {
            var playersWhoDraw = round.Turns.Select(x => x.DrawingPlayerId);
            var drawingPlayer = players.First(x => !playersWhoDraw.Contains(x.Id));
            round.Count++;
            return StartTurn(word, drawingPlayer, players);
        }

        private static Turn StartTurn(Word word,
            Player drawingPlayer,
            IEnumerable<Player> players)
        {
            return new Turn
            {
                DrawingPlayerId = drawingPlayer.Id,
                DrawingPlayer = drawingPlayer,
                PlayersTurns = players.Select(x => new PlayerTurn
                {
                    PlayerId = x.Id,
                    IsDrawing = x.Id == drawingPlayer.Id
                }).ToList(),
                Word = word
            };
        }
    }
}
