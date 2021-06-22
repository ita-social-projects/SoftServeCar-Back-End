﻿using System.Threading.Tasks;
using Car.Data.Entities;
using Car.Domain.Dto;

namespace Car.Domain.Services.Interfaces
{
    public interface IUserPreferencesService
    {
        Task<UserPreferencesDto?> GetPreferencesAsync(int userId);

        Task<UserPreferencesDto?> UpdatePreferencesAsync(UserPreferencesDto preferencesDTO);
    }
}
