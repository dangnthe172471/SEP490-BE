using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.Tests.Repositories
{
    public class UserRepositoryTests
    {
        private readonly DiamondHealthContext _dbContext;
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<DiamondHealthContext>()
                .UseInMemoryDatabase("UserRepoTestDB")
                .Options;

            _dbContext = new DiamondHealthContext(options);
            _repository = new UserRepository(_dbContext);
        }

        [Fact]
        public async Task AddAsync_ShouldSaveUser()
        {
            // Arrange
            var user = new User { Phone = "0909123456", PasswordHash = "hash", FullName = "Test User", RoleId = 1, IsActive = true };

            // Act
            await _repository.AddAsync(user);

            // Assert
            var savedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Phone == "0909123456");
            Assert.NotNull(savedUser);
        }

        [Fact]
        public async Task AddAsync_ShouldLinkPatient_WhenExists()
        {
            // Arrange
            var user = new User
            {
                Phone = "0909123456",
                PasswordHash = "hash",
                FullName = "Patient Test",
                RoleId = 2,
                IsActive = true,
                Patient = new Patient { PatientId = 1, Allergies = "Peanuts" }
            };

            // Act
            await _repository.AddAsync(user);

            // Assert
            var savedPatient = await _dbContext.Patients.FirstOrDefaultAsync(p => p.PatientId == 1);
            Assert.NotNull(savedPatient);
            Assert.Equal(user.UserId, savedPatient.UserId);
        }
    }
}
