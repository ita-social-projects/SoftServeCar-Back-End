﻿using FluentValidation;

namespace Car.Data.FluentValidation
{
    public class CarValidator : AbstractValidator<Entities.Car>
    {
        public CarValidator()
        {
            RuleFor(car => car.Id).GreaterThan(Constants.IDLENGTH);
            RuleFor(car => car.Color).NotNull();
            RuleFor(car => car.PlateNumber).NotNull().NotEmpty()
                                                     .MinimumLength(Constants.PLATENUMBERMINLENGTH)
                                                     .MaximumLength(Constants.PLATENUMBERMAXLENGTH);
            RuleFor(car => car.OwnerId).GreaterThan(Constants.IDLENGTH);
            RuleFor(car => car.Owner).SetValidator(new UserValidator());
            RuleFor(car => car.Model).SetValidator(new ModelValidator());
        }
    }
}
