﻿using Sketch.Services;
using Xunit;

namespace Tests.Unit
{
    public class CommandServiceTests
    {
        [Fact]
        public void ShouldParsePublicMessage()
        {
            var command = CommandParser.Parse("Oi!");

            Assert.Equal(CommandType.PublicMessage, command.Type);
            Assert.Equal("Oi!", command.Message);
        }

        [Fact]
        public void ShouldParsePublicMessageToUser()
        {
            var command = CommandParser.Parse(@"\u user1 Hi user1!");

            Assert.Equal(CommandType.PublicMessageToUser, command.Type);
            Assert.Equal("user1", command.Destinatary);
            Assert.Equal("Hi user1!", command.Message);
        }

        [Fact]
        public void ShouldParsePrivateMessageToUser()
        {
            var command = CommandParser.Parse(@"\p user1 Hi user1!");

            Assert.Equal(CommandType.PrivateMessageToUser, command.Type);
            Assert.Equal("user1", command.Destinatary);
            Assert.Equal("Hi user1!", command.Message);
        }

        [Fact]
        public void ShouldParseExit()
        {
            var command = CommandParser.Parse(@"\exit");

            Assert.Equal(CommandType.Exit, command.Type);
        }

        [Theory]
        [InlineData(@"\invalid", @"InvalidCommand \invalid")]
        [InlineData(@"\invalid 123", @"InvalidCommand \invalid")]
        [InlineData(@"\", @"InvalidCommand \")]
        public void ShouldParseInvalidCommand(string commandString, string messageResult)
        {
            var command = CommandParser.Parse(commandString);

            Assert.Equal(CommandType.Error, command.Type);
            Assert.Equal(messageResult, command.Message);
        }

        [Fact]
        public void ShouldParseKudos()
        {
            var command = CommandParser.Parse(@"\kudos user1");

            Assert.Equal(CommandType.Kudos, command.Type);
            Assert.Equal("user1", command.Destinatary);
        }

        [Fact]
        public void ShouldParseKudosWithoutDestinatary()
        {
            var command = CommandParser.Parse(@"\kudos");

            Assert.Equal(CommandType.Error, command.Type);
            Assert.Equal("Command must have destinatary", command.Message);
        }

        [Fact]
        public void ShouldParseHelp()
        {
            var command = CommandParser.Parse(@"\help");

            Assert.Equal(CommandType.Help, command.Type);
            Assert.Equal(DestinataryType.Self, command.DestinataryType);
        }

        [Fact]
        public void ShouldParseList()
        {
            var command = CommandParser.Parse(@"\list teste");

            Assert.Equal(CommandType.ListChatRooms, command.Type);
            Assert.Equal(DestinataryType.Self, command.DestinataryType);
        }

        [Fact]
        public void ShouldParseChangeGameRoom()
        {
            var command = CommandParser.Parse(@"\c gameroom 1");

            Assert.Equal(CommandType.ChangeGameRoom, command.Type);
            Assert.Equal(DestinataryType.Self, command.DestinataryType);
            Assert.Equal("gameroom 1", command.GameRoomName);
        }

        [Fact]
        public void ShouldParseChangChatRoomWithoutName()
        {
            var command = CommandParser.Parse(@"\c ");

            Assert.Equal(CommandType.Error, command.Type);
            Assert.Equal(DestinataryType.Self, command.DestinataryType);
        }

        [Fact]
        public void ShouldParsePath()
        {
            var command = CommandParser.Parse(@"\path M150 0 L75 200 L225 200 Z");

            Assert.Equal(CommandType.Drawing, command.Type);
            Assert.Equal("M150 0 L75 200 L225 200 Z", command.Drawing);
        }
    }
}
