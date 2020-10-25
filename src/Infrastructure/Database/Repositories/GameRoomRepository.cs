using Infrastructure.Database;
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
    }
}
