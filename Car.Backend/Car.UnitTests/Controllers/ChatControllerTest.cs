﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Car.Data.Entities;
using Car.Domain.Dto;
using Car.Domain.Dto.ChatDto;
using Car.Domain.Filters;
using Car.Domain.Services.Interfaces;
using Car.UnitTests.Base;
using Car.WebApi.Controllers;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Car.UnitTests.Controllers
{
    public class ChatControllerTest : TestBase
    {
        private readonly Mock<IChatService> chatService;
        private readonly ChatController chatController;

        public ChatControllerTest()
        {
            chatService = new Mock<IChatService>();
            chatController = new ChatController(chatService.Object);
        }

        [Theory]
        [AutoEntityData]
        public async Task GetUserChats_WhenUserExists_ReturnsOkObjectResult(User user, List<ChatDto> chats)
        {
            // Arrange
            chatService.Setup(x => x.GetUserChatsAsync(It.IsAny<int>()))
                .ReturnsAsync(chats);

            // Act
            var result = await chatController.GetUserChats(user.Id);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            (result as OkObjectResult)?.Value.Should().BeEquivalentTo(chats);
        }

        [Theory]
        [AutoEntityData]
        public async Task GetUserChats_WhenUserNotExist_ReturnsOkObjectResult(User user)
        {
            // Arrange
            chatService.Setup(x => x.GetUserChatsAsync(It.IsAny<int>()))
                .ReturnsAsync((List<ChatDto>)null);

            // Act
            var result = await chatController.GetUserChats(user.Id);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            (result as OkObjectResult)?.Value.Should().BeNull();
        }

        [Theory]
        [AutoEntityData]
        public async Task GetChat_WhenChatExists_ReturnsOkObjectResult(IEnumerable<MessageDto> chat)
        {
            // Arrange
            chatService.Setup(x => x.GetMessagesByChatIdAsync(chat.FirstOrDefault().ChatId, 0))
                .ReturnsAsync(chat);

            // Act
            var result = await chatController.GetChat(chat.FirstOrDefault().ChatId, 0);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<OkObjectResult>();
                (result as OkObjectResult)?.Value.Should().Be(chat);
            }
        }

        [Theory]
        [AutoEntityData]
        public async Task GetChat_WhenChatNotExist_ReturnsOkObjectResult(Chat chat)
        {
            // Arrange
            chatService.Setup(x => x.GetMessagesByChatIdAsync(chat.Id, 0))
                .ReturnsAsync((IEnumerable<MessageDto>)null);

            // Act
            var result = await chatController.GetChat(chat.Id, 0);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<OkObjectResult>();
                (result as OkObjectResult)?.Value.Should().BeNull();
            }
        }

        [Theory]
        [AutoEntityData]
        public async Task AddChat_WhenChatIsValid_ReturnsOkObjectResult(Chat chat)
        {
            // Arrange
            var createChatDto = Mapper.Map<Chat, CreateChatDto>(chat);
            chatService.Setup(x => x.AddChatAsync(createChatDto)).ReturnsAsync(chat);

            // Act
            var result = await chatController.AddChat(createChatDto);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<OkObjectResult>();
                (result as OkObjectResult)?.Value.Should().Be(chat);
            }
        }

        [Theory]
        [AutoEntityData]
        public async Task GetFiltered_WhenMessagesExist_ReturnsOkObjectResult(ChatFilter filter, IEnumerable<ChatDto> chats)
        {
            // Arrange
            chatService.Setup(x => x.GetFilteredChatsAsync(filter))
                .ReturnsAsync(chats);

            // Act
            var result = await chatController.GetFiltered(filter);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<OkObjectResult>();
                (result as OkObjectResult)?.Value.Should().Be(chats);
            }
        }

        [Theory]
        [AutoEntityData]
        public async Task GetFiltered_WhenMessagesNotExist_ReturnsOkObjectResult(ChatFilter filter, List<ChatDto> chats)
        {
            // Arrange
            chatService.Setup(x => x.GetFilteredChatsAsync(filter))
                .ReturnsAsync(chats);

            // Act
            var result = await chatController.GetFiltered(filter);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<OkObjectResult>();
                (result as OkObjectResult)?.Value.Should().Be(chats);
            }
        }
    }
}