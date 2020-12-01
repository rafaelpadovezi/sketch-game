using System;
using System.Collections.Generic;

namespace Sketch.DTOs
{
    public class PlayerViewModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
    }

    public class GameRoomViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class RankingViewModel
    {
        public IDictionary<string, int> Results { get; set; } = new Dictionary<string, int>();
        public Guid? WinnerId { get; set; }
    }
}
