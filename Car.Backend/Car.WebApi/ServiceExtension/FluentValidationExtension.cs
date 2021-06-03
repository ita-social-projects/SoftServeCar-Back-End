using Car.Data.Entities;
using Car.Data.FluentValidation;
using Car.Domain.Dto;
using Car.Domain.FluentValidation;
using Car.Domain.Models.Journey;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Car.WebApi.ServiceExtension
{
    public static class FluentValidationExtension
    {
        public static void AddFluentValidators(this IServiceCollection services)
        {
            services.AddTransient<IValidator<JourneyDto>, JourneyDtoValidator>();
            services.AddTransient<IValidator<RequestDto>, RequestDtoValidator>();
            services.AddTransient<IValidator<LocationDto>, LocationDtoValidator>();
            services.AddTransient<IValidator<CreateCarDto>, CreateCarModelValidator>();
        }
    }
}