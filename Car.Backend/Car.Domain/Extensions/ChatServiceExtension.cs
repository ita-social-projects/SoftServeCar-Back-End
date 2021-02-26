﻿using System.Linq;
using Car.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Car.Domain.Extensions
{
    public static class ChatServiceExtension
    {
        public static IQueryable<User> IncludeChats(this IQueryable<User> user)
        {
            return user
                .Include(organizer => organizer.OrganizerJourneys)
                .ThenInclude(journey => journey.Chat)
                .Include(participant => participant.ParticipantJourneys)
                .ThenInclude(journey => journey.Chat);
        }
    }
}