﻿using FluentValidation;

namespace Car.Data.FluentValidation
{
    public class CarValidator : AbstractValidator<Entities.Car>
    {
        public CarValidator()
        {
            RuleFor(car => car.Id).GreaterThan(Constants.Constants.IdLength);
            RuleFor(car => car.Color).NotNull();
            RuleFor(car => car.PlateNumber).NotNull().NotEmpty()
                                                     .MinimumLength(Constants.Constants.PlateNumberMinLength)
                                                     .MaximumLength(Constants.Constants.PlateNumberMaxLength);
            RuleFor(car => car.OwnerId).GreaterThan(Constants.Constants.IdLength);
            RuleFor(car => car.Owner).NotNull().SetValidator(new UserValidator()!);
            RuleFor(car => car.Model).NotNull().SetValidator(new ModelValidator()!);
        }
    }
}
