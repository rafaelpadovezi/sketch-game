using Sketch.Models;
using System;
using System.Threading.Tasks;

namespace Sketch.Infrastructure.Database.Repositories.Interfaces
{
    public interface IGameRoomRepository : IRepository<GameRoom>
    {
        Task<GameRoom> GetRoomByTurnId(Guid turnId);
        Task<(GameRoom gameRoom, Round round, Turn turn)> GetRoomRoundAndTurn(Guid turnId);
    }
}
