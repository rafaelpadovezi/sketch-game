using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Sketch.Models;
using System;
using System.Linq;
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
            var query = _context.Words
                .Where(x => x.GameRoomType == type);
            Random rand = new Random();
            int toSkip = rand.Next(1, await query.CountAsync());
            return await query
                .OrderBy(x => x.Id)
                .Skip(toSkip)
                .FirstAsync();
        }
    }
}