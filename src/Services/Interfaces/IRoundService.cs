using Sketch.Models;
using System;
using System.Threading.Tasks;

namespace Sketch.Services
{
    public interface IRoundService
    {
        Task EndTurn(Guid turnId);
        Task EndTurn(GameRoom gameRoom);
        Task GuessWord(GameRoom gameRoom, Player player, Turn turn, string guess);
        Task StartRound(GameRoom gameRoom);
        Task NextTurn(Guid turnId);
        Task AddToTurn(GameRoom gameroom, Turn turn, Player player);
        Task PlayerLeavesTurn(GameRoom gameRoom, Player player);
    }
}