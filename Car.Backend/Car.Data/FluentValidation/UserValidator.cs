﻿using System;
using FluentValidation;

namespace Car.Data.FluentValidation
{
    public class UserValidator : AbstractValidator<Entities.User>
    {
        public UserValidator()
        {
            RuleFor(user => user.Id).GreaterThan(Constants.IDLENGTH);
            RuleFor(user => user.Name).NotNull().NotEmpty().MaximumLength(Constants.STRINGMAXLENGTH);
            RuleFor(user => user.Surname).NotNull().NotEmpty().MaximumLength(Constants.STRINGMAXLENGTH);
            RuleFor(user => user.Position).NotNull().NotEmpty().MaximumLength(Constants.POSITIONMAXLENGTH);
            RuleFor(user => user.Location).NotNull().NotEmpty().MaximumLength(Constants.LOCATIONMAXLENGTH);
            RuleFor(user => user.HireDate).NotNull().LessThanOrEqualTo(DateTime.Now);
            RuleFor(user => user.Email).NotNull().MinimumLength(Constants.EMAILMINLENGTH)
                                                 .MaximumLength(Constants.EMAILMAXLENGTH)
                                                 .EmailAddress();
        }
    }
}
