using System;

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
}
