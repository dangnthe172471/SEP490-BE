using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

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

        // Tạo DbContext InMemory mới cho mỗi test (độc lập với DB thật)
        private DiamondHealthContext NewCtx(string db)
        {
            var opt = new DbContextOptionsBuilder<DiamondHealthContext>()
                .UseInMemoryDatabase(db)
                .EnableSensitiveDataLogging()
                .Options;
            return new DiamondHealthContext(opt);
        }

        // Helper: Seed dữ liệu test
        private async Task SeedTestDataAsync(DiamondHealthContext ctx)
        {
            // Seed Roles
            var roles = new List<Role>
            {
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Patient" },
                new Role { RoleId = 3, RoleName = "Doctor" },
                new Role { RoleId = 4, RoleName = "Nurse" },
                new Role { RoleId = 5, RoleName = "Pharmacy Provider" }
            };
            ctx.Roles.AddRange(roles);

            // Seed Users
            var users = new List<User>
            {
                new User 
                { 
                    UserId = 1, 
                    Phone = "0905123456", 
                    FullName = "John Doe", 
                    Email = "john@example.com",
                    Gender = "Male",
                    Dob = new DateOnly(1990, 1, 1),
                    IsActive = true,
                    RoleId = 2,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456")
                },
                new User 
                { 
                    UserId = 2, 
                    Phone = "0962900476", 
                    FullName = "Jane Smith", 
                    Email = "jane@example.com",
                    Gender = "Female",
                    Dob = new DateOnly(1985, 5, 15),
                    IsActive = false,
                    RoleId = 3,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
                },
                new User 
                { 
                    UserId = 3, 
                    Phone = "0987654321", 
                    FullName = "Bob Johnson", 
                    Email = "bob@example.com",
                    Gender = "Male",
                    Dob = new DateOnly(1992, 8, 20),
                    IsActive = true,
                    RoleId = 2,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("mypassword")
                }
            };
            ctx.Users.AddRange(users);

            // Seed Patients
            var patients = new List<Patient>
            {
                new Patient { PatientId = 1, UserId = 1, Allergies = "Peanuts", MedicalHistory = "Diabetes" },
                new Patient { PatientId = 2, UserId = 3, Allergies = "None", MedicalHistory = "None" }
            };
            ctx.Patients.AddRange(patients);

            await ctx.SaveChangesAsync();
        }

        // ===== LOGIN TESTS =====

        [Fact]
        public async Task GetByPhoneAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            using var ctx = NewCtx(nameof(GetByPhoneAsync_ReturnsUser_WhenUserExists));
            await SeedTestDataAsync(ctx);
            var repo = new UserRepository(ctx);

            // Act
            var result = await repo.GetByPhoneAsync("0905123456", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(1);
            result.Phone.Should().Be("0905123456");
            result.FullName.Should().Be("John Doe");
            result.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetByPhoneAsync_ReturnsNull_WhenUserNotFound()
        {
            // Arrange
            using var ctx = NewCtx(nameof(GetByPhoneAsync_ReturnsNull_WhenUserNotFound));
            await SeedTestDataAsync(ctx);
            var repo = new UserRepository(ctx);

            // Act
            var result = await repo.GetByPhoneAsync("9999999999", CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            using var ctx = NewCtx(nameof(GetByIdAsync_ReturnsUser_WhenUserExists));
            await SeedTestDataAsync(ctx);
            var repo = new UserRepository(ctx);

            // Act
            var result = await repo.GetByIdAsync(1, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(1);
            result.Phone.Should().Be("0905123456");
            result.FullName.Should().Be("John Doe");
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenUserNotFound()
        {
            // Arrange
            using var ctx = NewCtx(nameof(GetByIdAsync_ReturnsNull_WhenUserNotFound));
            await SeedTestDataAsync(ctx);
            var repo = new UserRepository(ctx);

            // Act
            var result = await repo.GetByIdAsync(999, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        // ===== CHANGE PASSWORD TESTS =====

        [Fact]
        public async Task UpdateAsync_UpdatesUserPassword_WhenValidUser()
        {
            // Arrange
            using var ctx = NewCtx(nameof(UpdateAsync_UpdatesUserPassword_WhenValidUser));
            await SeedTestDataAsync(ctx);
            var repo = new UserRepository(ctx);

            var user = await ctx.Users.FirstAsync(u => u.UserId == 1);
            var oldPasswordHash = user.PasswordHash;
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword("newPassword456");
            user.PasswordHash = newPasswordHash;

            // Act
            await repo.UpdateAsync(user, CancellationToken.None);

            // Assert
            var updatedUser = await ctx.Users.FirstAsync(u => u.UserId == 1);
            updatedUser.PasswordHash.Should().Be(newPasswordHash);
            updatedUser.PasswordHash.Should().NotBe(oldPasswordHash);
        }

        [Fact]
        public async Task UpdateAsync_ThrowsException_WhenUserNotFound()
        {
            // Arrange
            using var ctx = NewCtx(nameof(UpdateAsync_ThrowsException_WhenUserNotFound));
            await SeedTestDataAsync(ctx);
            var repo = new UserRepository(ctx);

            var nonExistentUser = new User
            {
                UserId = 999, // Non-existent ID
                Phone = "9999999999",
                FullName = "Non Existent",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password")
            };

            // Act & Assert
            await repo.Invoking(r => r.UpdateAsync(nonExistentUser, CancellationToken.None))
                .Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task UpdateAsync_UpdatesOnlyPassword_WhenOtherFieldsUnchanged()
        {
            // Arrange
            using var ctx = NewCtx(nameof(UpdateAsync_UpdatesOnlyPassword_WhenOtherFieldsUnchanged));
            await SeedTestDataAsync(ctx);
            var repo = new UserRepository(ctx);

            var user = await ctx.Users.FirstAsync(u => u.UserId == 1);
            var originalFullName = user.FullName;
            var originalEmail = user.Email;
            var originalPhone = user.Phone;
            
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword("newPassword456");
            user.PasswordHash = newPasswordHash;

            // Act
            await repo.UpdateAsync(user, CancellationToken.None);

            // Assert
            var updatedUser = await ctx.Users.FirstAsync(u => u.UserId == 1);
            updatedUser.PasswordHash.Should().Be(newPasswordHash);
            updatedUser.FullName.Should().Be(originalFullName);
            updatedUser.Email.Should().Be(originalEmail);
            updatedUser.Phone.Should().Be(originalPhone);
        }

        [Fact]
        public async Task UpdateAsync_HandlesEmptyPassword()
        {
            // Arrange
            using var ctx = NewCtx(nameof(UpdateAsync_HandlesEmptyPassword));
            await SeedTestDataAsync(ctx);
            var repo = new UserRepository(ctx);

            var user = await ctx.Users.FirstAsync(u => u.UserId == 1);
            user.PasswordHash = "";

            // Act
            await repo.UpdateAsync(user, CancellationToken.None);

            // Assert
            var updatedUser = await ctx.Users.FirstAsync(u => u.UserId == 1);
            updatedUser.PasswordHash.Should().Be("");
        }

        [Fact]
        public async Task UpdateAsync_HandlesNullPassword()
        {
            // Arrange
            using var ctx = NewCtx(nameof(UpdateAsync_HandlesNullPassword));
            await SeedTestDataAsync(ctx);
            var repo = new UserRepository(ctx);

            var user = await ctx.Users.FirstAsync(u => u.UserId == 1);
            user.PasswordHash = null;

            // Act
            await repo.UpdateAsync(user, CancellationToken.None);

            // Assert
            var updatedUser = await ctx.Users.FirstAsync(u => u.UserId == 1);
            updatedUser.PasswordHash.Should().BeNull();
        }
    }
}
