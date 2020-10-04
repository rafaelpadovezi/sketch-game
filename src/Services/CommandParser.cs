using System;
using System.Linq;

namespace Sketch.Services
{
    public class ChatCommand
    {
        public CommandType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public DestinataryType DestinataryType { get; set; }
        public string Destinatary { get; set; } = string.Empty;
        public string ChatRoomName { get; set; } = string.Empty;
    }

    public enum CommandType
    {
        PublicMessage,
        PublicMessageToUser,
        PrivateMessageToUser,
        ListChatRooms,
        ChangeChatRoom,
        Kudos,
        Exit,
        Error,
        Help
    }

    public enum DestinataryType
    {
        Self,
        Public,
        Private
    }

    public class CommandParser
    {
        public static readonly string[] CommandList = new string[]
        {
            $"{PublicMessageToUserCommand} {{user}} - Send public message to user",
            $"{PrivateMessageToUserCommand} {{user}} - Send private message to user",
            $"{KudosCommand} {{user}} - Send kudos to user",
            $"{ListChatRoomsCommand} - List chatrooms",
            $"{ChangeChatRoomCommand} {{chatRoom}} - Change chatroom",
            $"{ExitCommand} - Exit chat server"
        };
        private const string ListChatRoomsCommand = @"\list";
        private const string ChangeChatRoomCommand = @"\c";
        private const string ExitCommand = @"\exit";
        private const string HelpCommand = @"\help";
        private const char Separator = ' ';
        private const string PublicMessageToUserCommand = @"\u";
        private const string PrivateMessageToUserCommand = @"\p";
        private const string KudosCommand = @"\kudos";
        private static readonly string[] DestinataryCommands =
            {
                PublicMessageToUserCommand,
                PrivateMessageToUserCommand,
                KudosCommand
            };

        public static ChatCommand Parse(string commandString)
        {
            if (!commandString.StartsWith(@"\"))
            {
                return new ChatCommand
                {
                    DestinataryType = DestinataryType.Public,
                    Type = CommandType.PublicMessage,
                    Message = commandString
                };
            }

            string command = commandString.Split(Separator)[0];
            if (IsDestinataryCommand(command))
            {
                return ParseDestinataryCommand(commandString, command);
            }

            if (Is(ChangeChatRoomCommand, commandString))
            {
                return ParseChangeRoomCommand(commandString);
            }

            if (Is(ListChatRoomsCommand, commandString))
            {
                return new ChatCommand
                {
                    Type = CommandType.ListChatRooms
                };
            }

            if (Is(HelpCommand, commandString))
            {
                return new ChatCommand
                {
                    Type = CommandType.Help
                };
            }

            if (Is(ExitCommand, commandString))
            {
                return new ChatCommand
                {
                    Type = CommandType.Exit
                };
            }

            return InvalidCommand(commandString);
        }

        private static ChatCommand ParseChangeRoomCommand(string commandString)
        {
            if (commandString.Split(Separator).Length == 1)
            {
                return ChatRoomError;
            }

            string chatRoomName = commandString.Split(Separator)[1];
            if (!chatRoomName.Any())
            {
                return ChatRoomError;
            }

            return new ChatCommand
            {
                Type = CommandType.ChangeChatRoom,
                ChatRoomName = chatRoomName
            };
        }

        private static ChatCommand ParseDestinataryCommand(string commandString, string command)
        {
            if (commandString.Split(Separator).Length == 1)
            {
                return DestinaryError;
            }

            string destinatary = commandString.Split(Separator)[1];
            if (!destinatary.Any())
            {
                return DestinaryError;
            }

            return command switch
            {
                PublicMessageToUserCommand => new ChatCommand
                {
                    DestinataryType = DestinataryType.Public,
                    Type = CommandType.PublicMessageToUser,
                    Destinatary = commandString.Split(Separator)[1],
                    Message = string.Join(Separator,
                    commandString.Split(Separator).Skip(2))
                },
                PrivateMessageToUserCommand => new ChatCommand
                {
                    DestinataryType = DestinataryType.Private,
                    Type = CommandType.PrivateMessageToUser,
                    Destinatary = commandString.Split(Separator)[1],
                    Message = string.Join(Separator,
                    commandString.Split(Separator).Skip(2))
                },
                KudosCommand => new ChatCommand
                {
                    DestinataryType = DestinataryType.Public,
                    Type = CommandType.Kudos,
                    Destinatary = destinatary,
                    Message = string.Empty
                },
                _ => InvalidCommand(commandString)
            };
        }

        private static bool Is(string command, string commandString) =>
            commandString.StartsWith($"{command} ") ||
            commandString == command;

        private static ChatCommand InvalidCommand(string commandString) =>
            new ChatCommand
            {
                DestinataryType = DestinataryType.Self,
                Type = CommandType.Error,
                Message = $"InvalidCommand {commandString.Split(Separator)[0]}"
            };

        private static bool IsDestinataryCommand(string command)
        {
            return DestinataryCommands.Contains(command);
        }

        private static ChatCommand DestinaryError =>
            new ChatCommand
            {
                DestinataryType = DestinataryType.Self,
                Type = CommandType.Error,
                Message = "Command must have destinatary"
            };

        private static ChatCommand ChatRoomError =>
            new ChatCommand
            {
                DestinataryType = DestinataryType.Self,
                Type = CommandType.Error,
                Message = "Chat room name must be provided"
            };
    }
}
