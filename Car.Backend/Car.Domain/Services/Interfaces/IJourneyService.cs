﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Car.Domain.Dto;
using Car.Domain.Models.Journey;

namespace Car.Domain.Services.Interfaces
{
    public interface IJourneyService
    {
        Task<IEnumerable<JourneyModel>> GetPastJourneysAsync(int userId);

        Task<IEnumerable<JourneyModel>> GetUpcomingJourneysAsync(int userId);

        Task<IEnumerable<JourneyModel>> GetScheduledJourneysAsync(int userId);

        Task<JourneyModel> GetJourneyByIdAsync(int journeyId);

        Task<List<IEnumerable<StopDto>>> GetStopsFromRecentJourneysAsync(int userId, int countToTake = 5);

        Task DeletePastJourneyAsync();

        Task<JourneyModel> AddJourneyAsync(CreateJourneyModel journeyModel);
    }
}
