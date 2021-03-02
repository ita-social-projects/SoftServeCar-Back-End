﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Car.Data.Context;
using Car.Data.Entities;
using Car.Data.Infrastructure;
using Car.UnitTests.Base;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Xunit;

namespace Car.UnitTests.Repository
{
    public class RepositoryTest : TestBase
    {
        private readonly IRepository<User> repository;
        private readonly CarContext dbContext;

        public RepositoryTest()
        {
            var options = new DbContextOptionsBuilder<CarContext>()
                .UseInMemoryDatabase(databaseName: "CarTest")
                .Options;
            dbContext = new CarContext(options);
            repository = new Repository<User>(dbContext);
        }

        [Fact]
        public async Task GetByIdAsync_EntityExists_ReturnsEntity()
        {
            // Arrange
            var entities = Fixture.CreateMany<User>();
            await dbContext.Users.AddRangeAsync(entities);
            var entity = entities.First();

            // Act
            var result = await repository.GetByIdAsync(entity.Id);

            // Assert
            result.Should().BeEquivalentTo(entity);
        }

        [Fact]
        public async Task GetByIdAsync_EntityNotExist_ReturnsNull()
        {
            // Arrange
            var entities = Fixture.CreateMany<User>();
            await dbContext.Users.AddRangeAsync(entities);
            var entity = Fixture.Create<User>();

            // Act
            var result = await repository.GetByIdAsync(entity.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Query_ParametersNotPassed_ReturnsIQueryable()
        {
            // Arrange
            var entities = Fixture.CreateMany<User>();
            await dbContext.Users.AddRangeAsync(entities);
            var expected = dbContext.Users.AsQueryable();

            // Act
            var result = repository.Query();

            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task AddAsync_EntityIsValid_ReturnsAddedEntity()
        {
            // Arrange
            var entity = Fixture.Create<User>();

            // Act
            var result = await repository.AddAsync(entity);

            // Assert
            result.Should().BeEquivalentTo(entity);
        }

        [Fact]
        public async Task AddAsync_EntityIsNotValid_ThrowsArgumentNullException()
        {
            // Arrange
            User entity = null;

            // Act
            var result = new Func<Task>(() => repository.AddAsync(entity));

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(result);
        }
    }
}
