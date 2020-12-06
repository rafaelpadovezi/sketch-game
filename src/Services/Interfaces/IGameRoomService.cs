using Sketch.Models;
using System;
using System.Threading.Tasks;

namespace Sketch.Services
{
    public interface IGameRoomService
    {
        Task EnterGameRoom(string gameRoomName, Player player);
        Task LeaveGameRoom(Player player, string newGameRoom);
        Task GuessOrSendMessage(string message, Player player);
        Task Leave(Player player);
        Task SendDrawing(Player player, Guid gameRoomId, string drawing);
    }
}