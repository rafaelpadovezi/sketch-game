using System;
using System.Collections.Generic;

namespace Sketch.Models
{
    public class Player : BaseEntity
    {
        public Guid? GameRoomId { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public GameRoom? GameRoom { get; set; }
        public ICollection<PlayerTurn> PlayerTurns { get; set; } = new List<PlayerTurn>();
    }

    public class PlayerTurn : BaseEntity
    {
        public Guid PlayerId { get; set; }
        public Guid TurnId { get; set; }
        public int? Points { get; set; }
        public bool IsDrawing { get; set; } = false;
    }
}
