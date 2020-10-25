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
        Help
    }

    public class ChatServerResponse
    {
        public string Message { get; protected set; } = string.Empty;
        public ResponseType Type { get; protected set; }
        public string[] Details { get; protected set; } = Array.Empty<string>();

        public static ChatServerResponse Help() =>
            new ChatServerResponse
            {
                Type = ResponseType.Help,
                Details = CommandParser.CommandList
            };

        internal static ChatServerResponse ListChatRooms(IEnumerable<string> chatRoomList) =>
            new ChatServerResponse
            {
                Type = ResponseType.ListGameRooms,
                Details = chatRoomList.ToArray()
            };

        public static ChatServerResponse Error(string message) =>
            new ChatServerResponse
            {
                Type = ResponseType.Error,
                Message = message
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
