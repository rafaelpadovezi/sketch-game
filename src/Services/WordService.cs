using Sketch.Models;
using System.Threading.Tasks;

namespace Sketch.Services
{
    public class WordService : IWordService
    {
        public Task<Word> GetWord(GameRoomType type)
        {
            return Task.FromResult(new Word { Content = "Bola" });
        }
    }
}