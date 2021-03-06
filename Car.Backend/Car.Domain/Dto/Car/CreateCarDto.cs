﻿using Car.Data.Entities;
using Car.Data.Enums;
using Microsoft.AspNetCore.Http;

namespace Car.Domain.Dto
{
    public class CreateCarDto
    {
        public int OwnerId { get; set; }

        public int ModelId { get; set; }

        public Color Color { get; set; }

        public string PlateNumber { get; set; } = string.Empty;

        public IFormFile? Image { get; set; }
    }
}
