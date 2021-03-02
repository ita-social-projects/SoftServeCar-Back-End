﻿using FluentValidation;

namespace Car.Data.FluentValidation
{
    public class AddressValidator : AbstractValidator<Entities.Address>
    {
        public AddressValidator()
        {
            RuleFor(address => address.Id).GreaterThan(Constants.IDLENGTH);
            RuleFor(address => address.City).NotNull().NotEmpty().MaximumLength(Constants.STRINGMAXLENGTH);
            RuleFor(address => address.Street).NotNull().NotEmpty().MaximumLength(Constants.STRINGMAXLENGTH);
            RuleFor(address => address.Latitude).NotNull();
            RuleFor(address => address.Longitude).NotNull();
            RuleFor(address => address.Location).SetValidator(new LocationValidator());
        }
    }
}
