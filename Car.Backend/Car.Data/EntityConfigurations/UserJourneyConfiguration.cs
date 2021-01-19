﻿using Car.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Car.Data.EntityConfigurations
{
    public class UserJourneyConfiguration : IEntityTypeConfiguration<UserJourney>
    {
        public void Configure(EntityTypeBuilder<UserJourney> builder)
        {
            builder.HasKey(userJourney => new
            {
                userJourney.UserId,
                userJourney.JourneyId,
            });

            builder.HasOne(userJourney => userJourney.User)
                .WithMany(user => user.UserJourneys)
                .HasForeignKey(userJourney => userJourney.UserId);

            builder.HasOne(userJourney => userJourney.Journey)
                .WithMany(journey => journey.Participants)
                .HasForeignKey(userJourney => userJourney.JourneyId);
        }
    }
}