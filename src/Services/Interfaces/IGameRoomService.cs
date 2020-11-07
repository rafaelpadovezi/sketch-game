using Sketch.Models;
using System.Threading.Tasks;

namespace Sketch.Services
{
    public interface IGameRoomService
    {
        Task EnterGameRoom(string gameRoomName, Player player);
        Task LeaveGameRoom(Player player, string newGameRoom);
        Task SendMessage(string message, Player player);
    }
}