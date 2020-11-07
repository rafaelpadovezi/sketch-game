﻿using Sketch.Models;
using System.Threading.Tasks;

namespace Sketch.Services
{
    public interface IWordService
    {
        Task<Word> GetWord(GameRoomType type);
    }
}