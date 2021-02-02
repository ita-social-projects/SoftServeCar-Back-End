﻿namespace Car.Data.Entities
{
    public class UserJourney : IEntity
    {
        public int UserId { get; set; }

        public User User { get; set; }

        public int JourneyId { get; set; }

        public Journey Journey { get; set; }

        public bool HasLuggage { get; set; }
    }
}
