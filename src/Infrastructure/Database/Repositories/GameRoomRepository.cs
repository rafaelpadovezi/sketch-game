using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Sketch.Infrastructure.Database.Repositories.Interfaces;
using Sketch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sketch.Infrastructure.Database.Repositories
{
    public class GameRoomRepository : EntityRepository<GameRoom>, IGameRoomRepository
    {
        public GameRoomRepository(SketchDbContext context)
            : base(context)
        {
        }

        public async Task<(GameRoom gameRoom, Round round, Turn turn)>
            GetRoomRoundAndTurn(Guid turnId)
        {
            var gameRoom = await Context.GameRooms.SingleOrDefaultAsync(
                gameRoom => gameRoom.Rounds.Any(
                    round => round.Turns.Any(
                        turn => turn.Id == turnId)))
                ?? throw new Exception($"Game room not found for turn id {turnId}");
            var round = gameRoom.Rounds.SingleOrDefault(
                    round => round.Turns.Any(
                        turn => turn.Id == turnId))
                ?? throw new Exception($"Round not found for turn id {turnId}");
            var turn = round.Turns.SingleOrDefault(
                turn => turn.Id == turnId)
                ?? throw new Exception($"Turn not found for id {turnId}");

            return (gameRoom, round, turn);
        }
    }
}
