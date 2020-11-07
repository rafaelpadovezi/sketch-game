using System;
using System.Threading.Tasks;

namespace Sketch.Services
{
    public interface IGameService
    {
        Task NewPlayer(Guid playerId);
        Task<bool> NewCommand(Guid playerId, string commandString);
        Task PlayerLeaves(Guid playerId);
    }
}
