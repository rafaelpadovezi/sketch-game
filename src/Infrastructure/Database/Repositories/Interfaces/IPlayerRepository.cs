using Sketch.Models;
using System.Threading.Tasks;

namespace Sketch.Infrastructure.Database.Repositories.Interfaces
{
    public interface IPlayerRepository : IRepository<Player>
    {
        Task<bool> NicknameIsInUse(string username);
    }
}