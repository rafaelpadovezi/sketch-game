using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Sketch.Models;
using System.Threading.Tasks;

namespace Sketch.Services
{
    public class WordService : IWordService
    {
        private readonly SketchDbContext _context;

        public WordService(SketchDbContext context)
        {
            _context = context;
        }

        public async Task<Word> PickWord(GameRoomType type)
        {
            return await _context.Words.FirstAsync();
        }
    }
}