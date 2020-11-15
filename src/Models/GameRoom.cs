using System;
using System.Collections.Generic;

namespace Sketch.Models
{

    public class GameRoom : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public GameRoomType Type { get; set; }
        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
        public virtual ICollection<Round> Rounds { get; set; } = new List<Round>();
    }

    public class Round : BaseEntity
    {
        public Guid GameRoomId { get; set; }
        public int Count { get; set; } = 1;
        public DateTime StartTimestamp { get; set; } = DateTime.Now;
        public DateTime? EndTimestamp { get; set; }
        public virtual ICollection<Turn> Turns { get; set; } = new List<Turn>();
    }

    public class Turn : BaseEntity
    {
        public Guid RoundId { get; set; }
        public Guid DrawingPlayerId { get; set; }
        public Guid WordId { get; set; }
        public DateTime StartTimestamp { get; set; } = DateTime.Now;
        public DateTime? EndTimestamp { get; set; }
        public virtual Word Word { get; set; } = new Word();
        public virtual ICollection<PlayerTurn> PlayersTurns { get; set; } = new List<PlayerTurn>();
    }

    public class Word : BaseEntity
    {
        public string Content { get; set; } = string.Empty;
        public GameRoomType GameRoomType { get; set; }
    }

    public enum GameRoomType
    {
        General,
        HarryPotter,
        Animals
    }
}
