﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoFixture;
using Car.Data.Entities;
using Car.Data.Infrastructure;
using Car.Domain.Dto;
using Car.Domain.Dto.ChatDto;
using Car.Domain.Filters;
using Car.Domain.Services.Implementation;
using Car.Domain.Services.Interfaces;
using Car.UnitTests.Base;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace Car.UnitTests.Services
{
    public class ChatServiceTest : TestBase
    {
        private readonly IChatService chatService;
        private readonly Mock<IRepository<Chat>> chatRepository;
        private readonly Mock<IRepository<User>> userRepository;
        private readonly Mock<IRepository<Message>> messageRepository;

        public ChatServiceTest()
        {
            chatRepository = new Mock<IRepository<Chat>>();
            userRepository = new Mock<IRepository<User>>();
            messageRepository = new Mock<IRepository<Message>>();
            chatService = new ChatService(userRepository.Object, chatRepository.Object, messageRepository.Object, Mapper);
        }

        [Theory]
        [AutoEntityData]
        public async Task GetChatByIdAsync_WhenChatExists_ReturnsCorrectType(IEnumerable<Message> chats)
        {
            // Arrange
            var chat = chats.First();

            messageRepository.Setup(repo => repo.Query(It.IsAny<Expression<Func<Message, object>>[]>()))
                .Returns(chats.AsQueryable().BuildMock().Object);

            // Act
            var result = await chatService.GetMessagesByChatIdAsync(chat.Id, chat.Id);

            // Assert
            result.Should().BeOfType<List<MessageDto>>();
        }

        [Theory]
        [AutoEntityData]
        public async Task GetChatByIdAsync_WhenChatNotExist_ReturnsNull(IEnumerable<Message> chats)
        {
            // Arrange
            var id = chats.Max(chat => chat.Id) + 1;

            messageRepository.Setup(repo => repo.Query(It.IsAny<Expression<Func<Message, object>>[]>()))
                .Returns(chats.AsQueryable().BuildMock().Object);

            // Act
            var result = await chatService.GetMessagesByChatIdAsync(id, chats.First().Id);

            // Assert
            result.Should().BeEmpty();
        }

        [Theory]
        [AutoEntityData]
        public async Task AddChatAsync_WhenChatIsValid_ReturnsChatObject(CreateChatDto chat)
        {
            // Arrange
            var chatEntity = Mapper.Map<CreateChatDto, Chat>(chat);
            chatRepository.Setup(repo => repo.AddAsync(It.IsAny<Chat>())).ReturnsAsync(chatEntity);

            // Act
            var result = await chatService.AddChatAsync(chat);

            // Assert
            result.Should().BeEquivalentTo(chatEntity);
        }

        [Theory]
        [AutoEntityData]
        public async Task AddChatAsync_WhenChatIsNotValid_ReturnsNull(CreateChatDto chat)
        {
            // Arrange
            var chatEntity = Mapper.Map<CreateChatDto, Chat>(chat);
            chatRepository.Setup(repo => repo.AddAsync(chatEntity)).ReturnsAsync((Chat)null);

            // Act
            var result = await chatService.AddChatAsync(chat);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [AutoEntityData]
        public async Task GetUserChatsAsync_WhenChatsExist_ReturnsChatCollection(IEnumerable<User> users)
        {
            // Arrange
            var user = users.First();
            var organizerJourneys = Fixture.Build<Journey>()
                .With(j => j.Organizer, user)
                .With(j => j.OrganizerId, user.Id).CreateMany();
            var participantJourneys = Fixture.Build<Journey>()
                .With(j => j.Participants, new List<User>() { user }).CreateMany();
            user.OrganizerJourneys = organizerJourneys.ToList();
            user.ParticipantJourneys = participantJourneys.ToList();

            var chats = organizerJourneys.Select(j => j.Chat)
                .Union(participantJourneys.Select(j => j.Chat))
                .Except(new List<Chat>() { null });
            var expectedChats = Mapper.Map<IEnumerable<Chat>, IEnumerable<ChatDto>>(chats);
            userRepository.Setup(repo => repo.Query())
                .Returns(users.AsQueryable().BuildMock().Object);

            // Act
            var result = await chatService.GetUserChatsAsync(user.Id);

            // Assert
            result.Should().BeEquivalentTo(expectedChats);
        }

        [Theory]
        [AutoEntityData]
        public async Task GetUserChatsAsync_WhenChatsNotExist_ReturnsEmptyCollection(IEnumerable<User> users)
        {
            // Arrange
            var user = users.First();
            var organizerJourneys = Fixture.Build<Journey>()
                .With(j => j.Organizer, user)
                .With(j => j.OrganizerId, user.Id)
                .With(j => j.Chat, (Chat)null).CreateMany();
            var participantJourneys = Fixture.Build<Journey>()
                .With(j => j.Participants, new List<User>() { user })
                .With(j => j.Chat, (Chat)null).CreateMany();
            user.OrganizerJourneys = organizerJourneys.ToList();
            user.ParticipantJourneys = participantJourneys.ToList();

            userRepository.Setup(repo => repo.Query())
                .Returns(users.AsQueryable().BuildMock().Object);

            // Act
            var result = await chatService.GetUserChatsAsync(user.Id);

            // Assert
            result.Should().BeEmpty();
        }

        [Theory]
        [AutoEntityData]
        public async Task GetUserChatsAsync_WhenUserNotExist_ReturnsNull(IEnumerable<User> users)
        {
            // Arrange
            var user = Fixture.Build<User>().With(user => user.Id, users.Max(user => user.Id) + 1).Create();
            var organizerJourneys = Fixture.Build<Journey>()
                .With(j => j.Chat, (Chat)null).CreateMany();
            var participantJourneys = Fixture.Build<Journey>()
                .With(j => j.Chat, (Chat)null).CreateMany();

            userRepository.Setup(repo => repo.Query())
                .Returns(users.AsQueryable().BuildMock().Object);

            // Act
            var result = await chatService.GetUserChatsAsync(user.Id);

            // Assert
            result.Should().BeEmpty();
        }

        [Theory]
        [AutoEntityData]
        public async Task AddMessageAsync_WhenMessageIsValid_ReturnsMessageObject(Message message)
        {
            // Arrange
            messageRepository.Setup(repo => repo.AddAsync(message)).ReturnsAsync(message);

            // Act
            var result = await chatService.AddMessageAsync(message);

            // Assert
            result.Should().BeEquivalentTo(message);
        }

        [Theory]
        [AutoEntityData]
        public async Task AddMessageAsync_WhenMessageIsNotValid_ReturnsNull(Message message)
        {
            // Arrange
            messageRepository.Setup(repo => repo.AddAsync(message)).ReturnsAsync((Message)null);

            // Act
            var result = await chatService.AddMessageAsync(message);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [AutoEntityData]
        public async Task GetChatByIdAsync_ChatExist_ReturnsChat(Chat chat)
        {
            // Arrange
            chatRepository.Setup(repo => repo.GetByIdAsync(chat.Id)).ReturnsAsync(chat);

            // Act
            var result = await chatService.GetChatByIdAsync(chat.Id);

            // Assert
            result.Should().Be(chat);
        }

        [Theory]
        [AutoEntityData]
        public async Task GetChatByIdAsync_IdNotMatchesChat_ReturnsNull(Chat chat)
        {
            // Arrange
            chatRepository.Setup(repo => repo.GetByIdAsync(chat.Id)).ReturnsAsync(chat);

            // Act
            var result = await chatService.GetChatByIdAsync(chat.Id + 1);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("test message", "test")]
        [InlineData("we@ther", "@th")]
        public async Task GetFilteredChatsAsync_MessagesContainSearchText_ReturnsChats(string message, string searchText)
        {
            // Arrange
            IEnumerable<Message> messages = null;
            ChatFilter filter = null;
            IEnumerable<ChatDto> expectedResult = null;
            GetInitializedChatsData(out messages, out filter, out expectedResult);

            messages.First().Text = message;
            filter.SearchText = searchText;
            expectedResult.First().MessageText = message;

            messageRepository.Setup(repo => repo.Query())
                .Returns(messages.AsQueryable().BuildMock().Object);

            // Act
            var result = await chatService.GetFilteredChatsAsync(filter);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Theory]
        [InlineData("test message", "msg")]
        [InlineData("we@ther", "ath")]
        public async Task GetFilteredChatsAsync_MessagesNotContainSearchText_ReturnsEmptyCollection(string message, string searchText)
        {
            // Arrange
            IEnumerable<Message> messages = null;
            ChatFilter filter = null;
            IEnumerable<ChatDto> expectedResult = null;
            GetInitializedChatsData(out messages, out filter, out expectedResult);

            messages.First().Text = message;
            filter.SearchText = searchText;
            expectedResult = new List<ChatDto>();

            messageRepository.Setup(repo => repo.Query())
                .Returns(messages.AsQueryable().BuildMock().Object);

            // Act
            var result = await chatService.GetFilteredChatsAsync(filter);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        private void GetInitializedChatsData(out IEnumerable<Message> messages, out ChatFilter filter, out IEnumerable<ChatDto> expectedResult)
        {
            var chat = Fixture.Create<Chat>();
            var chatDto = Mapper.Map<ChatDto>(chat);
            var chatDtoList = new List<ChatDto>()
            {
                chatDto,
            };

            messages = Fixture.Build<Message>()
            .With(x => x.ChatId, chat.Id)
            .With(x => x.Chat, chat)
            .CreateMany();

            filter = Fixture.Build<ChatFilter>()
            .With(x => x.SearchText, messages.First().Text)
            .With(x => x.Chats, chatDtoList)
            .Create();

            expectedResult = chatDtoList;
            expectedResult.First().MessageText = messages.First().Text;
            expectedResult.First().MessageId = messages.First().Id;
        }
    }
}
