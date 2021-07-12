using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Internal;
using Car.Data.Constants;
using Car.Data.Entities;
using Car.Data.Enums;
using Car.Data.Infrastructure;
using Car.Domain.Dto;
using Car.Domain.Extensions;
using Car.Domain.Filters;
using Car.Domain.Models.Journey;
using Car.Domain.Services.Interfaces;
using Geolocation;
using Microsoft.EntityFrameworkCore;

namespace Car.Domain.Services.Implementation
{
    public class JourneyService : IJourneyService
    {
        private readonly IRepository<Journey> journeyRepository;
        private readonly IRepository<Request> requestRepository;
        private readonly INotificationService notificationService;
        private readonly IRequestService requestService;
        private readonly IMapper mapper;

        public JourneyService(
            IRepository<Journey> journeyRepository,
            IRepository<Request> requestRepository,
            INotificationService notificationService,
            IRequestService requestService,
            IMapper mapper)
        {
            this.journeyRepository = journeyRepository;
            this.requestRepository = requestRepository;
            this.notificationService = notificationService;
            this.requestService = requestService;
            this.mapper = mapper;
        }

        public async Task<JourneyModel> GetJourneyByIdAsync(int journeyId, bool withCancelledStops = false)
        {
            var journeyQueryable = journeyRepository
                .Query()
                .IncludeAllParticipants()
                .IncludeStopsWithAddresses()
                .IncludeJourneyPoints();

            var journey = withCancelledStops ?
                await journeyQueryable
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(j => j.Id == journeyId)
                :
                await journeyQueryable
                    .FilterUncancelledJourneys()
                    .FirstOrDefaultAsync(j => j.Id == journeyId);

            return mapper.Map<Journey, JourneyModel>(journey);
        }

        public async Task<IEnumerable<JourneyModel>> GetPastJourneysAsync(int userId)
        {
            var journeys = await journeyRepository
                .Query()
                .FilterUncancelledJourneys()
                .IncludeJourneyInfo(userId)
                .FilterPast()
                .ToListAsync();

            return mapper.Map<IEnumerable<Journey>, IEnumerable<JourneyModel>>(journeys);
        }

        public async Task<IEnumerable<JourneyModel>> GetScheduledJourneysAsync(int userId)
        {
            var journeys = await journeyRepository
                .Query(journey => journey!.Schedule!)
                .FilterUncancelledJourneys()
                .IncludeJourneyInfo(userId)
                .Where(journey => journey.Schedule != null)
                .ToListAsync();

            return mapper.Map<IEnumerable<Journey>, IEnumerable<JourneyModel>>(journeys);
        }

        public async Task<IEnumerable<JourneyModel>> GetUpcomingJourneysAsync(int userId)
        {
            var journeys = await journeyRepository
                .Query()
                .FilterUncancelledJourneys()
                .IncludeJourneyInfo(userId)
                .FilterUpcoming()
                .ToListAsync();

            return mapper.Map<IEnumerable<Journey>, IEnumerable<JourneyModel>>(journeys);
        }

        public async Task<List<IEnumerable<StopDto>>> GetStopsFromRecentJourneysAsync(int userId, int countToTake = 5)
        {
            var stops = await journeyRepository
                .Query()
                .FilterUncancelledJourneys()
                .IncludeStopsWithAddresses()
                .FilterByUser(userId)
                .OrderByDescending(journey => journey.DepartureTime)
                .Take(countToTake)
                .SelectStartAndFinishStops()
                .ToListAsync();

            return stops;
        }

        public async Task DeletePastJourneyAsync()
        {
            var now = DateTime.Now;
            var termInDays = 14;

            var journeysToDelete = journeyRepository
                .Query()
                .AsEnumerable()
                .Where(j => (now - j.DepartureTime).TotalDays >= termInDays)
                .ToList();

            await journeyRepository.DeleteRangeAsync(journeysToDelete);
            await journeyRepository.SaveChangesAsync();
        }

        public async Task<JourneyModel> AddJourneyAsync(JourneyDto journeyModel)
        {
            var journey = mapper.Map<JourneyDto, Journey>(journeyModel);

            var addedJourney = await journeyRepository.AddAsync(journey);
            await journeyRepository.SaveChangesAsync();

            await CheckForSuitableRequests(addedJourney);

            return mapper.Map<Journey, JourneyModel>(addedJourney);
        }

        public IEnumerable<Journey> GetFilteredJourneys(JourneyFilter filter) =>
            journeyRepository
                .Query()
                .FilterUncancelledJourneys()
                .IncludeAllParticipants()
                .IncludeStopsWithAddresses()
                .IncludeJourneyPoints()
                .FilterUnsuitableJourneys(filter);

        public async Task DeleteAsync(int journeyId)
        {
            var journeyToDelete = await journeyRepository
                .Query()
                .IncludeAllParticipants()
                .FirstOrDefaultAsync(j => j.Id == journeyId);

            if (journeyToDelete is null)
            {
                return;
            }

            journeyRepository.Delete(journeyToDelete);

            await journeyRepository.SaveChangesAsync();
        }

        public async Task CancelAsync(int journeyId)
        {
            var journeyToCancel = await journeyRepository
                .Query()
                .FilterUncancelledJourneys()
                .IncludeNotifications()
                .IncludeAllParticipants()
                .FirstOrDefaultAsync(j => j.Id == journeyId);

            if (journeyToCancel is not null)
            {
                journeyToCancel.IsCancelled = true;
                journeyToCancel.DepartureTime = DateTime.UtcNow;
                await journeyRepository.SaveChangesAsync();

                var notificationsToDelete = journeyToCancel.Notifications.Select(item => item.Id);
                await notificationService.DeleteNotificationsAsync(notificationsToDelete);
                await notificationService.NotifyParticipantsAboutCancellationAsync(journeyToCancel);
            }
        }

        public async Task<JourneyModel> UpdateRouteAsync(JourneyDto journeyDto)
        {
            var journey = await journeyRepository.Query()
                    .FilterUncancelledJourneys()
                    .IncludeStopsWithAddresses()
                    .IncludeJourneyPoints()
                    .FirstOrDefaultAsync(j => j.Id == journeyDto.Id);

            if (journey is null)
            {
                return null!;
            }

            var updatedJourney = mapper.Map<JourneyDto, Journey>(journeyDto);

            journey.Duration = updatedJourney.Duration;
            journey.Stops = updatedJourney.Stops;
            journey.JourneyPoints = updatedJourney.JourneyPoints;
            journey.RouteDistance = updatedJourney.RouteDistance;

            await journeyRepository.SaveChangesAsync();

            await notificationService.JourneyUpdateNotifyUserAsync(journey);

            return mapper.Map<Journey, JourneyModel>(journey);
        }

        public async Task<JourneyModel> UpdateDetailsAsync(JourneyDto journeyDto)
        {
            var journey = mapper.Map<JourneyDto, Journey>(journeyDto);

            journey = await journeyRepository.UpdateAsync(journey);
            await journeyRepository.SaveChangesAsync();

            if (journey != null)
            {
                await notificationService.JourneyUpdateNotifyUserAsync(await journeyRepository
                    .Query()
                    .IncludeAllParticipants()
                    .FirstOrDefaultAsync(j => j.Id == journey.Id));
            }

            return mapper.Map<Journey, JourneyModel>(journey!);
        }

        public IEnumerable<ApplicantJourney> GetApplicantJourneys(JourneyFilter filter)
        {
            var journeysResult = new List<ApplicantJourney>();

            var filteredJourneys = GetFilteredJourneys(filter);

            foreach (var journey in filteredJourneys)
            {
                journeysResult.Add(new ApplicantJourney()
                {
                    Journey = mapper.Map<Journey, JourneyModel>(journey),
                    ApplicantStops = GetApplicantStops(filter, journey),
                });
            }

            return journeysResult;
        }

        public async Task CheckForSuitableRequests(Journey journey)
        {
            var requests = requestRepository
                .Query()
                .Where(r => journey.OrganizerId != r.UserId)
                .FilterUnsuitableRequests(journey, request => mapper.Map<Request, JourneyFilter>(request));

            foreach (var request in requests.ToList())
            {
                await requestService.NotifyUserAsync(
                    mapper.Map<Request, RequestDto>(request),
                    journey,
                    GetApplicantStops(
                        mapper.Map<Request, JourneyFilter>(request),
                        journey));
            }
        }

        public async Task<bool> IsCanceled(int journeyId)
        {
            var journey = await journeyRepository
                .Query()
                .FirstOrDefaultAsync(journey => journey.Id == journeyId);

            return journey?.IsCancelled ?? true;
        }

        public async Task DeleteUserFromJourney(int journeyId, int userId)
        {
            var journey = await journeyRepository
                .Query()
                .IncludeAllParticipants()
                .FirstOrDefaultAsync(j => j.Id == journeyId);

            var userToDelete = journey?.Participants.FirstOrDefault(u => u.Id == userId);

            if (journey?.Participants.Remove(userToDelete!) ?? false)
            {
                journey!.Stops
                    .Where(stop => stop.UserId == userToDelete!.Id)
                    .ToList()
                    .ForEach(stop => stop.IsCancelled = true);

                await notificationService.NotifyDriverAboutParticipantWithdrawal(journey, userId);
                await journeyRepository.SaveChangesAsync();
            }
        }

        private IEnumerable<StopDto> GetApplicantStops(JourneyFilter filter, Journey journey)
        {
            var applicantStops = new List<StopDto>();

            var distances = journey.JourneyPoints.Select(p => p.CalculateDistance(
                filter.FromLatitude,
                filter.FromLongitude)).ToList();

            var startPointIndex = distances.IndexOf(distances.Min());
            var startRoutePoint = journey.JourneyPoints.ToList()[startPointIndex];

            distances = journey.JourneyPoints.ToList()
                .GetRange(startPointIndex, journey.JourneyPoints.Count - startPointIndex)
                .Select(p => p.CalculateDistance(
                    filter.ToLatitude,
                    filter.ToLongitude)).ToList();

            var endRoutePoint = journey.JourneyPoints.ToList()[startPointIndex + distances.IndexOf(distances.Min())];

            applicantStops.Add(new StopDto()
            {
                Id = 0,
                Index = 0,
                UserId = filter.ApplicantId,
                Address = new Dto.Address.AddressDto()
                {
                    Latitude = startRoutePoint.Latitude,
                    Longitude = startRoutePoint.Longitude,
                    Name = "Start",
                },
                Type = StopType.Intermediate,
            });

            applicantStops.Add(new StopDto()
            {
                Id = 0,
                Index = 1,
                UserId = filter.ApplicantId,
                Address = new Dto.Address.AddressDto()
                {
                    Latitude = endRoutePoint.Latitude,
                    Longitude = endRoutePoint.Longitude,
                    Name = "Finish",
                },
                Type = StopType.Intermediate,
            });

            return applicantStops;
        }
    }
}
