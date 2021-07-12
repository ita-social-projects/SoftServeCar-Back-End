using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Car.Data.Entities;
using Car.Data.Infrastructure;
using Car.Domain.Dto;
using Car.Domain.Hubs;
using Car.Domain.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Car.Domain.Services.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly IRepository<Notification> notificationRepository;
        private readonly IHubContext<SignalRHub> notificationHub;
        private readonly IMapper mapper;

        public NotificationService(
            IRepository<Notification> notificationRepository,
            IHubContext<SignalRHub> notificationHub,
            IMapper mapper)
        {
            this.notificationRepository = notificationRepository;
            this.notificationHub = notificationHub;
            this.mapper = mapper;
        }

        public async Task<NotificationDto> GetNotificationAsync(int notificationId) =>
            mapper.Map<Notification, NotificationDto>(
              await notificationRepository.Query(notificationSender => notificationSender!.Sender!)
              .FirstOrDefaultAsync(notification => notification.Id == notificationId));

        public async Task<IEnumerable<Notification>> GetNotificationsAsync(int userId) =>
                await notificationRepository.Query().Include(n => n.Sender)
                .Where(p => p.ReceiverId == userId)
                .OrderByDescending(k => k.CreatedAt)
                .ToListAsync();

        public Task<int> GetUnreadNotificationsNumberAsync(int userId) =>
            notificationRepository.Query()
                .CountAsync(p => p.ReceiverId == userId && !p.IsRead);

        public async Task<NotificationDto> UpdateNotificationAsync(NotificationDto notification)
        {
            var updateNotification = mapper.Map<NotificationDto, Notification>(notification);
            await notificationRepository.UpdateAsync(updateNotification);
            await notificationRepository.SaveChangesAsync();

            return notification;
        }

        public async Task<NotificationDto> AddNotificationAsync(NotificationDto notification)
        {
            var addNotification = mapper.Map<NotificationDto, Notification>(notification);
            await notificationRepository.AddAsync(addNotification);
            await notificationRepository.SaveChangesAsync();

            await NotifyClientAsync(notification);

            return notification;
        }

        public async Task DeleteAsync(int notificationId)
        {
            notificationRepository.Delete(new Notification() { Id = notificationId });
            await notificationRepository.SaveChangesAsync();
        }

        public async Task<NotificationDto> MarkNotificationAsReadAsync(int notificationId)
        {
            var notificationToUpdate = await notificationRepository.Query()
                .FirstOrDefaultAsync(notification => notification.Id == notificationId);
            notificationToUpdate.IsRead = true;
            await notificationRepository.SaveChangesAsync();

            var updatedNotification = mapper.Map<Notification, NotificationDto>(notificationToUpdate);
            await NotifyClientAsync(updatedNotification);

            return updatedNotification;
        }

        public Task<NotificationDto> CreateNewNotificationAsync(CreateNotificationDto createNotificationDto) =>
            Task.Run(() => mapper.Map<CreateNotificationDto, NotificationDto>(createNotificationDto));

        public async Task JourneyUpdateNotifyUserAsync(Journey journey)
        {
            if (journey is null || journey.Participants is null)
            {
                return;
            }

            foreach (var participant in journey.Participants)
            {
                await AddNotificationAsync(new NotificationDto()
                {
                    SenderId = journey.Organizer!.Id,
                    ReceiverId = participant.Id,
                    Type = NotificationType.JourneyDetailsUpdate,
                    IsRead = false,
                    JourneyId = journey.Id,
                    JsonData = JsonSerializer.Serialize(new { }),
                });
            }
        }

        public async Task NotifyParticipantsAboutCancellationAsync(Journey journey)
        {
            if (journey.Participants is null)
            {
                return;
            }

            foreach (var user in journey.Participants)
            {
                await AddNotificationAsync(new NotificationDto()
                {
                    SenderId = journey.Organizer!.Id,
                    ReceiverId = user.Id,
                    Type = NotificationType.JourneyCancellation,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    JourneyId = journey.Id,
                    JsonData = JsonSerializer.Serialize(new { }),
                });
            }
        }

        public async Task DeleteNotificationsAsync(IEnumerable<int> notificationsId)
        {
            if (notificationsId is not null)
            {
                var notificationsToDelete = await notificationRepository
                    .Query()
                    .Where(x => notificationsId
                        .Contains(x.Id))
                    .ToListAsync();

                await notificationRepository.DeleteRangeAsync(notificationsToDelete);
                await notificationRepository.SaveChangesAsync();
            }
        }

        public async Task NotifyDriverAboutParticipantWithdrawal(Journey journey, int participantId) =>
            await AddNotificationAsync(new NotificationDto()
                {
                    SenderId = participantId,
                    ReceiverId = journey.Organizer!.Id,
                    Type = NotificationType.PassengerWithdrawal,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    JourneyId = journey.Id,
                    JsonData = JsonSerializer.Serialize(new { }),
                });

        private async Task NotifyClientAsync(NotificationDto notification)
        {
            await notificationHub.Clients.All.SendAsync("sendToReact", notification);
            await notificationHub.Clients.All.SendAsync(
                "updateUnreadNotificationsNumber",
                await GetUnreadNotificationsNumberAsync(notification.ReceiverId));
        }
    }
}