﻿using AutoMapper;
using Car.Data.Entities;
using Car.Domain.Dto;
using Car.Domain.Models.Notification;

namespace Car.Domain.Mapping
{
    public class NotificationMapper : Profile
    {
        public NotificationMapper()
        {
            CreateMap<Notification, NotificationDto>().ReverseMap();
            CreateMap<CreateNotificationDto, Notification>().ReverseMap();
            CreateMap<CreateNotificationDto, NotificationDto>().ReverseMap();
        }
    }
}
