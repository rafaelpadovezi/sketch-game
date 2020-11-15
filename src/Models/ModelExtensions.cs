using System.Linq;

namespace Sketch.Models
{
    public static class ModelExtensions
    {
        public static Turn CurrentTurn(this GameRoom gameRoom)
        {
            return gameRoom
                .Rounds.Single(x => !x.EndTimestamp.HasValue)
                .Turns.Single(x => !x.EndTimestamp.HasValue);
        }
    }
}
