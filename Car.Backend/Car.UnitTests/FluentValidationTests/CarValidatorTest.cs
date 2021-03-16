﻿using Car.Data;
using Car.Data.Enums;
using Car.Data.FluentValidation;
using FluentValidation.TestHelper;
using NUnit.Framework;
using Xunit;

namespace Car.UnitTests.FluentValidationTests
{
    [TestFixture]
    public class CarValidatorTest
    {
        private CarValidator validator;

        public CarValidatorTest()
        {
            validator = new CarValidator();
        }

        [Xunit.Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Id_IsNotValid_GeneratesValidationError(int value)
        {
            validator.ShouldHaveValidationErrorFor(car => car.Id, value);
        }

        [Xunit.Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void Id_IsSpecified_NotGeneratesValidationError(int value)
        {
            validator.ShouldNotHaveValidationErrorFor(car => car.Id, value);
        }

        [Xunit.Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void OwnerId_IsNotValid_GeneratesValidationError(int value)
        {
            validator.ShouldHaveValidationErrorFor(car => car.OwnerId, value);
        }

        [Xunit.Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OwnerId_IsSpecified_NotGeneratesValidationError(int value)
        {
            validator.ShouldNotHaveValidationErrorFor(car => car.OwnerId, value);
        }

        [Xunit.Theory]
        [InlineData(Color.Black)]
        public void Color_IsSpecified_NotGeneratesValidationError(Color value)
        {
            validator.ShouldNotHaveValidationErrorFor(car => car.Color, value);
        }

        [Xunit.Theory]
        [InlineData("")]
        [InlineData("123")]
        [InlineData(null)]
        public void PlateNumber_IsNull_GeneratesValidationError(string value)
        {
            validator.ShouldHaveValidationErrorFor(car => car.PlateNumber, value);
        }

        [Fact]
        public void PlateNumber__IsNotValid_GeneratesValidationError()
        {
            string longCommnt = new string('*', Constants.PLATENUMBER_MAX_LENGTH + 1);
            validator.ShouldHaveValidationErrorFor(journey => journey.PlateNumber, longCommnt);
        }

        [Xunit.Theory]
        [InlineData("12345")]
        public void PlateNumber_IsSpecified_NotGeneratesValidationError(string value)
        {
            validator.ShouldNotHaveValidationErrorFor(car => car.PlateNumber, value);
        }
    }
}
