using Sketch.DTOs;

namespace Sketch.Services.Interfaces
{
    public interface IPlayerService
    {
        System.Threading.Tasks.ValueTask<(bool result, PlayerViewModel? player)> Login(string username);
    }
}