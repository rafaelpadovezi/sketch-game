using System.Collections.Generic;
using System.Linq;

namespace Sketch.Models
{
    public static class ModelExtensions
    {
        public static Round CurrentRound(this GameRoom gameRoom)
        {
            return gameRoom
                .Rounds.Single(x => !x.EndTimestamp.HasValue);
        }

        public static Turn CurrentTurn(this GameRoom gameRoom)
        {
            return gameRoom
                .CurrentRound()
                .Turns.Single(x => !x.EndTimestamp.HasValue);
        }

        public static bool IsComplete(this Round round, IEnumerable<Player> players)
        {
            if (round.Turns.Any(x => !x.EndTimestamp.HasValue))
                return false;
            var playersWhoDraw = round.Turns.Select(x => x.DrawingPlayerId);
            return players.All(x => playersWhoDraw.Contains(x.Id));
        }
    }
}
