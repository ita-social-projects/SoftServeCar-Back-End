﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoFixture;
using Car.Data.Entities;
using Car.Data.Infrastructure;
using Car.Domain.Models.Notification;
using Car.Domain.Services.Implementation;
using Car.Domain.Services.Interfaces;
using Car.UnitTests.Base;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using Xunit;

namespace Car.UnitTests.Services
{
    public class NotificationServiceTest : TestBase
    {
        private readonly INotificationService notificationService;
        private readonly Mock<IRepository<Notification>> notificationRepository;
        private readonly Mock<IRepository<User>> userRepository;

        public NotificationServiceTest()
        {
            notificationRepository = new Mock<IRepository<Notification>>();
            userRepository = new Mock<IRepository<User>>();
            notificationService = new NotificationService(notificationRepository.Object, Mapper);
        }

        [Fact]
        public async Task GetNotificationAsync_WhenNotificationExist_ReturnsNotification()
        {
            // Arrange
            var notifications = Fixture.CreateMany<Notification>().ToList();
            var notification = notifications.First();

            notificationRepository.Setup(
                    repository => repository
                        .Query(
                            It.IsAny<
                                Expression<Func<Notification, object>>[]
                            >()))
                .Returns(notifications.AsQueryable().BuildMock().Object);

            // Act
            var result = await notificationService.GetNotificationAsync(notification.Id);

            // Assert
            result.Should().BeEquivalentTo(notification);
        }

        [Fact]
        public async Task GetNotificationsAsync_WhenUserExist_ReturnsNotifications()
        {
            // Arrange
            var users = Fixture.CreateMany<User>(10).ToList();
            var user = users.First();
            var notifications = Fixture
                .Build<Notification>()
                .With(n => n.ReceiverId, user.Id)
                .CreateMany(10)
                .ToList();
            var expectedNotifications = notifications
                .Where(n => n.ReceiverId == user.Id)
                .ToList();

            notificationRepository.Setup(
                    repository => repository
                        .Query(It.IsAny<Expression<Func<Notification, object>>[]>()))
                .Returns(notifications.AsQueryable().BuildMock().Object);

            userRepository.Setup(repository => repository.Query())
                .Returns(users.AsQueryable().BuildMock().Object);

            // Act
            var result = await notificationService.GetNotificationsAsync(user.Id);

            // Assert
            CollectionAssert.AreEquivalent(result, expectedNotifications);
        }

        [Fact]
        public async Task GetUnreadNotificationsAsync_WhenUserExist_ReturnsUnreadNotificationNumber()
        {
            // Arrange
            var users = Fixture.CreateMany<User>().ToList();
            var user = users.First();
            var notifications = Fixture
                .Build<Notification>()
                .With(n => n.ReceiverId, user.Id)
                .With(n => n.IsRead, true)
                .CreateMany()
                .ToList();
            var expectedNotificationsNumber = notifications
                .Count(n => n.ReceiverId == user.Id && !n.IsRead);

            notificationRepository.Setup(
                    repository => repository
                        .Query(It.IsAny<Expression<Func<Notification, object>>[]>()))
                .Returns(notifications.AsQueryable().BuildMock().Object);

            userRepository.Setup(repository => repository.Query())
                .Returns(users.AsQueryable().BuildMock().Object);

            // Act
            var result = await notificationService.GetUnreadNotificationsNumberAsync(user.Id);

            // Assert
            result.Should().Be(expectedNotificationsNumber);
        }

        [Fact]
        public async Task UpdateNotificationAsync_WhenNotificationExist_ReturnsUpdatedNotification()
        {
            // Arrange
            var notification = Fixture.Create<Notification>();
            var notifications = Fixture
                .Build<Notification>()
                .With(n => n.Id, notification.Id)
                .CreateMany()
                .ToList();
            var updatedNotification = notifications.First();

            notificationRepository.Setup(
                    repository => repository
                        .Query(It.IsAny<Expression<Func<Notification, object>>[]>()))
                .Returns(notifications.AsQueryable().BuildMock().Object);

            // Act
            var result = await notificationService.UpdateNotificationAsync(updatedNotification);

            // Assert
            result.Should().Be(updatedNotification);
        }

        [Fact]
        public async Task AddNotificationAsync_WhenNotificationExist_ReturnsAddedNotification()
        {
            // Arrange
            var notification = Fixture.Create<Notification>();

            notificationRepository.Setup(repo => repo.AddAsync(notification)).ReturnsAsync(notification);

            // Act
            var result = await notificationService.AddNotificationAsync(notification);

            // Assert
            result.Should().BeEquivalentTo(notification);
        }

        [Fact]
        public async Task UpdateNotificationAsync_WhenNotExist_ReturnsNull()
        {
            // Arrange
            var notification = (Notification)null;

            notificationRepository
                .Setup(repo => repo.AddAsync(notification))
                .ReturnsAsync((Notification)null);

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            var result = await notificationService.UpdateNotificationAsync(notification);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_WhenNotificationIsNotExist_ThrowDbUpdateConcurrencyException()
        {
            // Arrange
            var idNotificationToDelete = Fixture.Create<int>();
            notificationRepository.Setup(repo => repo.SaveChangesAsync()).Throws<DbUpdateConcurrencyException>();

            // Act
            var result = notificationService.Invoking(service => service.DeleteAsync(idNotificationToDelete));

            // Assert
            await result.Should().ThrowAsync<DbUpdateConcurrencyException>();
        }

        [Fact]
        public async Task DeleteAsync_WhenNotificationExist_ExecuteOnce()
        {
            // Arrange
            var idNotificationToDelete = Fixture.Create<int>();

            // Act
            await notificationService.DeleteAsync(idNotificationToDelete);

            // Assert
            notificationRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once());
        }

        [Fact]
        public async Task CreateNewNotificationAsync_WhenNotificationExist_ReturnsNull()
        {
            // Arrange
            var notifications = Fixture
                .CreateMany<Notification>()
                .ToList();
            var notification = notifications.First();
            var notificationModel = Fixture.Build<CreateNotificationModel>()
                .With(n => n.Type, notification.Type)
                .With(n => n.JsonData, notification.JsonData)
                .With(n => n.ReceiverId, notification.ReceiverId)
                .With(n => n.SenderId, notification.SenderId)
                .Create();

            notificationRepository
                .Setup(repo => repo.Query())
                .Returns(notifications
                    .AsQueryable()
                    .BuildMock()
                    .Object);

            // Act
            var result = await notificationService.CreateNewNotificationAsync(notificationModel);

            // Assert
            result
                .Should()
                .BeEquivalentTo(
                    notification,
                    options => options
                        .Excluding(o => o.Id)
                        .Excluding(o => o.Receiver)
                        .Excluding(o => o.Sender)
                        .Excluding(o => o.CreatedAt)
                        .Excluding(o => o.IsRead));
        }

        [Fact]
        public async Task MarkNotificationAsReadAsync_WhenNotificationExist_ReturnsReadNotification()
        {
            // Arrange
            var notifications = Fixture
                .CreateMany<Notification>()
                .ToList();
            var notification = notifications.First();

            notificationRepository
                .Setup(repo => repo.Query())
                .Returns(notifications
                    .AsQueryable()
                    .BuildMock()
                    .Object);

            // Act
            var result = await notificationService.MarkNotificationAsReadAsync(notification.Id);

            // Assert
            result.IsRead.Should().BeTrue();
        }
    }
}