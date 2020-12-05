using Sketch.Models;
using Sketch.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sketch.DTOs
{
    public enum ResponseType
    {
        ChatMessage,
        Error,
        ListGameRooms,
        Help,
        EnterGameRoom,
        EndOfTurn,
        Hit,
        StartOfTurn,
        EndOfRound
    }

    public class ChatServerResponse
    {
        public string Message { get; protected set; } = string.Empty;
        public ResponseType Type { get; protected set; }
        public IEnumerable<object> Details { get; protected set; } = Array.Empty<string>();

        public static ChatServerResponse Help() =>
            new ChatServerResponse
            {
                Type = ResponseType.Help,
                Details = CommandParser.CommandList
            };

        internal static ChatServerResponse ListChatRooms(IEnumerable<GameRoomViewModel> chatRoomList) =>
            new ChatServerResponse
            {
                Type = ResponseType.ListGameRooms,
                Details = chatRoomList
            };

        public static ChatServerResponse Error(string message) =>
            new ChatServerResponse
            {
                Type = ResponseType.Error,
                Message = message
            };

        internal static ChatServerResponse EnterGameRoom(string name) =>
            new ChatServerResponse
            {
                Type = ResponseType.EnterGameRoom,
                Details = new string[] { name }
            };
    }

    public class GameResponse : ChatServerResponse
    {
        public static GameResponse EndOfTurn(Turn turn) =>
            new GameResponse
            {
                Type = ResponseType.EndOfTurn,
                Details = new RankingViewModel[]
                {
                    new RankingViewModel
                    {
                        Results = turn.PlayersTurns
                            .OrderByDescending(x => x.Points)
                            .ToDictionary(x => x.Player.Username, x => x.Points ?? 0)
                    }
                }
            };

        internal static ChatServerResponse StartTurn(Word word, int duration) =>
            new GameResponse
            {
                Type = ResponseType.StartOfTurn,
                Message = $"The word is `{word.Content}`. Start drawing!",
                Details = new object[] { duration }
            };

        internal static ChatServerResponse StartTurn(Player drawingPlayer, int duration) =>
            new GameResponse
            {
                Type = ResponseType.StartOfTurn,
                Message = $"New turn! {drawingPlayer.Username} is drawing",
                Details = new object[] { duration }
            };

        internal static ChatServerResponse Hit(string guess) =>
            new GameResponse
            {
                Type = ResponseType.Hit,
                Message = $"nice! response is `{guess}`"
            };

        public static ChatServerResponse EndOfRound(Round round) =>
            new GameResponse
            {
                Type = ResponseType.EndOfRound,
                Details = new RankingViewModel[]
                {
                    new RankingViewModel
                    {
                        Results = round.Turns
                            .OrderBy(x => x.StartTimestamp)
                            .Select(turn => turn
                                .PlayersTurns
                                .ToDictionary(x => x.Player.Username, x => x.Points ?? 0))
                            .Aggregate((results, turnResults) =>
                                turnResults.ToDictionary(x => x.Key, x => x.Value + (results.ContainsKey(x.Key) ? results[x.Key] : 0)))
                    }
                }
            };
    }

    public class ChatMessage : ChatServerResponse
    {
        public ChatMessage()
        {
            Type = ResponseType.ChatMessage;
        }

        public static ChatMessage Public(string nickname, string message) =>
            new ChatMessage
            {
                Message = $"{nickname} says: {message}"
            };

        public static ChatMessage ChangeRoom(string nickname, string gameRoom) =>
            new ChatMessage
            {
                Message = $"{nickname} went to #{gameRoom}"
            };

        public static ChatMessage Kudos(string source, string destinatary) =>
            new ChatMessage
            {
                Message = $"{source} sent kudos to {destinatary}: ❤◦.¸¸.  ◦✿"
            };

        public static ChatMessage PublicMessageToUser(string source,
            string destinatary, string message) =>
            new ChatMessage
            {
                Message = $"{source} says to {destinatary}: {message}"
            };

        public static ChatMessage PrivateMessageToUser(string source,
            string destinatary, string message) =>
            new ChatMessage
            {
                Message = $"{source} says privately to {destinatary}: {message}"
            };

        public static ChatMessage NewPlayer(string chatName, string nickname) =>
            new ChatMessage
            {
                Message = $"\"{nickname}\" has joined #{chatName}"
            };

        public static ChatMessage PlayerLeftRoom(string chatName, string nickname) =>
            new ChatMessage
            {
                Message = $"\"{nickname}\" has left #{chatName}"
            };
    }
}
