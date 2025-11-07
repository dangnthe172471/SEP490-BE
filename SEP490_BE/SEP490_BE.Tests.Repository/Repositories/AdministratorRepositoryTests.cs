using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.Repositories;

namespace SEP490_BE.Tests.Repositories;

public class AdministratorRepositoryTests
{
    private DiamondHealthContext NewCtx(string dbName)
    {
        var options = new DbContextOptionsBuilder<DiamondHealthContext>()
            .UseInMemoryDatabase(dbName)
            .EnableSensitiveDataLogging()
            .Options;
        return new DiamondHealthContext(options);
    }

    private async Task SeedTestDataAsync(DiamondHealthContext ctx)
    {
        // Seed Roles
        var roles = new List<Role>
        {
            new Role { RoleId = 1, RoleName = "Administrator" },
            new Role { RoleId = 2, RoleName = "Patient" },
            new Role { RoleId = 3, RoleName = "Doctor" },
            new Role { RoleId = 4, RoleName = "Nurse" },
            new Role { RoleId = 5, RoleName = "Receptionist" },
            new Role { RoleId = 6, RoleName = "Pharmacy Provider" }
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
            },
            new User
            {
                UserId = 4,
                Phone = "0911111111",
                FullName = "Admin User",
                Email = "admin@example.com",
                Gender = "Male",
                Dob = new DateOnly(1980, 1, 1),
                IsActive = true,
                RoleId = 1,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123")
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

    // ===== GetAllAsync Tests =====

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        // Arrange
        using var ctx = NewCtx(nameof(GetAllAsync_ReturnsAllUsers));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);

        // Act
        var result = await repo.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(4);
        result.Should().Contain(u => u.UserId == 1);
        result.Should().Contain(u => u.UserId == 2);
        result.Should().Contain(u => u.UserId == 3);
        result.Should().Contain(u => u.UserId == 4);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoUsers()
    {
        // Arrange
        using var ctx = NewCtx(nameof(GetAllAsync_ReturnsEmptyList_WhenNoUsers));
        var repo = new AdministratorRepository(ctx);

        // Act
        var result = await repo.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    // ===== GetByIdAsync Tests =====

    [Fact]
    public async Task GetByIdAsync_ReturnsUser_WhenUserExists()
    {
        // Arrange
        using var ctx = NewCtx(nameof(GetByIdAsync_ReturnsUser_WhenUserExists));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);

        // Act
        var result = await repo.GetByIdAsync(1, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(1);
        result.Phone.Should().Be("0905123456");
        result.FullName.Should().Be("John Doe");
        result.Role.Should().NotBeNull();
        result.Role!.RoleName.Should().Be("Patient");
        result.Patient.Should().NotBeNull();
        result.Patient!.Allergies.Should().Be("Peanuts");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenUserNotFound()
    {
        // Arrange
        using var ctx = NewCtx(nameof(GetByIdAsync_ReturnsNull_WhenUserNotFound));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);

        // Act
        var result = await repo.GetByIdAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    // ===== GetByPhoneAsync Tests =====

    [Fact]
    public async Task GetByPhoneAsync_ReturnsUser_WhenUserExists()
    {
        // Arrange
        using var ctx = NewCtx(nameof(GetByPhoneAsync_ReturnsUser_WhenUserExists));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);

        // Act
        var result = await repo.GetByPhoneAsync("0905123456", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(1);
        result.Phone.Should().Be("0905123456");
        result.FullName.Should().Be("John Doe");
        result.Role.Should().NotBeNull();
        result.Patient.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByPhoneAsync_ReturnsNull_WhenUserNotFound()
    {
        // Arrange
        using var ctx = NewCtx(nameof(GetByPhoneAsync_ReturnsNull_WhenUserNotFound));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);

        // Act
        var result = await repo.GetByPhoneAsync("9999999999", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    // ===== GetByEmailAsync Tests =====

    [Fact]
    public async Task GetByEmailAsync_ReturnsUser_WhenUserExists()
    {
        // Arrange
        using var ctx = NewCtx(nameof(GetByEmailAsync_ReturnsUser_WhenUserExists));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);

        // Act
        var result = await repo.GetByEmailAsync("john@example.com", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(1);
        result.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsNull_WhenUserNotFound()
    {
        // Arrange
        using var ctx = NewCtx(nameof(GetByEmailAsync_ReturnsNull_WhenUserNotFound));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);

        // Act
        var result = await repo.GetByEmailAsync("nonexistent@example.com", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    // ===== AddAsync Tests =====

    [Fact]
    public async Task AddAsync_SavesUser()
    {
        // Arrange
        using var ctx = NewCtx(nameof(AddAsync_SavesUser));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);
        var newUser = new User
        {
            Phone = "0909999999",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            FullName = "New User",
            Email = "newuser@example.com",
            RoleId = 2,
            IsActive = true
        };

        // Act
        await repo.AddAsync(newUser, CancellationToken.None);

        // Assert
        var savedUser = await ctx.Users.FirstOrDefaultAsync(u => u.Phone == "0909999999");
        savedUser.Should().NotBeNull();
        savedUser!.FullName.Should().Be("New User");
    }

    [Fact]
    public async Task AddAsync_SavesUserWithPatient_WhenPatientProvided()
    {
        // Arrange
        using var ctx = NewCtx(nameof(AddAsync_SavesUserWithPatient_WhenPatientProvided));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);
        var newUser = new User
        {
            Phone = "0909999999",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            FullName = "New Patient",
            Email = "newpatient@example.com",
            RoleId = 2,
            IsActive = true,
            Patient = new Patient
            {
                PatientId = 100,
                Allergies = "Peanuts",
                MedicalHistory = "Asthma"
            }
        };

        // Act
        await repo.AddAsync(newUser, CancellationToken.None);

        // Assert
        var savedUser = await ctx.Users.Include(u => u.Patient).FirstOrDefaultAsync(u => u.Phone == "0909999999");
        savedUser.Should().NotBeNull();
        savedUser!.Patient.Should().NotBeNull();
        savedUser.Patient!.UserId.Should().Be(savedUser.UserId);
        savedUser.Patient.Allergies.Should().Be("Peanuts");
    }

    // ===== UpdateAsync Tests =====

    [Fact]
    public async Task UpdateAsync_UpdatesUser()
    {
        // Arrange
        using var ctx = NewCtx(nameof(UpdateAsync_UpdatesUser));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);
        var user = await ctx.Users.FirstAsync(u => u.UserId == 1);
        user.FullName = "Updated Name";
        user.Email = "updated@example.com";

        // Act
        await repo.UpdateAsync(user, CancellationToken.None);

        // Assert
        var updatedUser = await ctx.Users.FirstAsync(u => u.UserId == 1);
        updatedUser.FullName.Should().Be("Updated Name");
        updatedUser.Email.Should().Be("updated@example.com");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesPatient_WhenPatientExists()
    {
        // Arrange
        using var ctx = NewCtx(nameof(UpdateAsync_UpdatesPatient_WhenPatientExists));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);
        var user = await ctx.Users.Include(u => u.Patient).FirstAsync(u => u.UserId == 1);
        user.Patient!.Allergies = "Updated Allergies";
        user.Patient.MedicalHistory = "Updated History";

        // Act
        await repo.UpdateAsync(user, CancellationToken.None);

        // Assert
        var updatedPatient = await ctx.Patients.FirstAsync(p => p.PatientId == 1);
        updatedPatient.Allergies.Should().Be("Updated Allergies");
        updatedPatient.MedicalHistory.Should().Be("Updated History");
    }

    [Fact]
    public async Task UpdateAsync_CreatesPatient_WhenNewPatientProvided()
    {
        // Arrange
        using var ctx = NewCtx(nameof(UpdateAsync_CreatesPatient_WhenNewPatientProvided));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);
        var user = await ctx.Users.FirstAsync(u => u.UserId == 4); // Admin user without patient
        user.Patient = new Patient
        {
            PatientId = 100,
            UserId = user.UserId,
            Allergies = "New Allergies",
            MedicalHistory = "New History"
        };

        // Act
        await repo.UpdateAsync(user, CancellationToken.None);

        // Assert
        var newPatient = await ctx.Patients.FirstOrDefaultAsync(p => p.UserId == 4);
        newPatient.Should().NotBeNull();
        newPatient!.Allergies.Should().Be("New Allergies");
    }

    // ===== GetMaxPatientIdAsync Tests =====

    [Fact]
    public async Task GetMaxPatientIdAsync_ReturnsMaxId_WhenPatientsExist()
    {
        // Arrange
        using var ctx = NewCtx(nameof(GetMaxPatientIdAsync_ReturnsMaxId_WhenPatientsExist));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);

        // Act
        var result = await repo.GetMaxPatientIdAsync(CancellationToken.None);

        // Assert
        result.Should().Be(2); // Max PatientId from seed data
    }

    [Fact]
    public async Task GetMaxPatientIdAsync_ReturnsZero_WhenNoPatients()
    {
        // Arrange
        using var ctx = NewCtx(nameof(GetMaxPatientIdAsync_ReturnsZero_WhenNoPatients));
        var repo = new AdministratorRepository(ctx);

        // Act
        var result = await repo.GetMaxPatientIdAsync(CancellationToken.None);

        // Assert
        result.Should().Be(0);
    }

    // ===== DeleteAsync Tests =====

    [Fact]
    public async Task DeleteAsync_DeletesUser_WhenNoDependencies()
    {
        // Arrange
        using var ctx = NewCtx(nameof(DeleteAsync_DeletesUser_WhenNoDependencies));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);
        var user = await ctx.Users.Include(u => u.Patient).FirstAsync(u => u.UserId == 3);

        // Act
        await repo.DeleteAsync(3, CancellationToken.None);

        // Assert
        var deletedUser = await ctx.Users.FirstOrDefaultAsync(u => u.UserId == 3);
        deletedUser.Should().BeNull();
        var deletedPatient = await ctx.Patients.FirstOrDefaultAsync(p => p.PatientId == 2);
        deletedPatient.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ThrowsException_WhenPatientHasAppointments()
    {
        // Arrange
        using var ctx = NewCtx(nameof(DeleteAsync_ThrowsException_WhenPatientHasAppointments));
        await SeedTestDataAsync(ctx);
        
        // Create a room first (required for Doctor)
        var room = new Room
        {
            RoomId = 1,
            RoomName = "Room 1"
        };
        ctx.Rooms.Add(room);
        
        // Create a doctor first (required for Appointment)
        var doctor = new Doctor
        {
            DoctorId = 1,
            UserId = 2, // Use existing user
            Specialty = "General",
            ExperienceYears = 5,
            RoomId = 1
        };
        ctx.Doctors.Add(doctor);
        
        // Add an appointment for patient
        var appointment = new Appointment
        {
            AppointmentId = 1,
            PatientId = 1,
            DoctorId = 1,
            AppointmentDate = DateTime.UtcNow,
            Status = "Scheduled"
        };
        ctx.Appointments.Add(appointment);
        await ctx.SaveChangesAsync();

        var repo = new AdministratorRepository(ctx);

        // Act & Assert
        await repo.Invoking(r => r.DeleteAsync(1, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Không thể xóa vì bệnh nhân còn dữ liệu liên quan (lịch hẹn hoặc chat).");
    }

    [Fact]
    public async Task DeleteAsync_ThrowsException_WhenPatientHasChatLogs()
    {
        // Arrange
        using var ctx = NewCtx(nameof(DeleteAsync_ThrowsException_WhenPatientHasChatLogs));
        await SeedTestDataAsync(ctx);
        
        // Create a receptionist first (required for ChatLog)
        var receptionist = new Receptionist
        {
            ReceptionistId = 1,
            UserId = 4 // Use existing admin user
        };
        ctx.Receptionists.Add(receptionist);
        
        // Add a chat log for patient
        var chatLog = new ChatLog
        {
            ChatId = 1,
            PatientId = 1,
            ReceptionistId = 1,
            RoomChat = "room-123",
            CreatedAt = DateTime.UtcNow
        };
        ctx.ChatLogs.Add(chatLog);
        await ctx.SaveChangesAsync();

        var repo = new AdministratorRepository(ctx);

        // Act & Assert
        await repo.Invoking(r => r.DeleteAsync(1, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Không thể xóa vì bệnh nhân còn dữ liệu liên quan (lịch hẹn hoặc chat).");
    }

    // ===== SearchUsersAsync Tests =====

    [Fact]
    public async Task SearchUsersAsync_ReturnsFilteredUsers_ByFullName()
    {
        // Arrange
        using var ctx = NewCtx(nameof(SearchUsersAsync_ReturnsFilteredUsers_ByFullName));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);
        var request = new SearchUserRequest
        {
            FullName = "John Doe",
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (users, totalCount) = await repo.SearchUsersAsync(request, CancellationToken.None);

        // Assert
        users.Should().NotBeNull();
        users.Should().HaveCount(1);
        users.First().FullName.Should().Contain("John");
        totalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchUsersAsync_ReturnsFilteredUsers_ByPhone()
    {
        // Arrange
        using var ctx = NewCtx(nameof(SearchUsersAsync_ReturnsFilteredUsers_ByPhone));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);
        var request = new SearchUserRequest
        {
            Phone = "0905",
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (users, totalCount) = await repo.SearchUsersAsync(request, CancellationToken.None);

        // Assert
        users.Should().NotBeNull();
        users.Should().HaveCount(1);
        users.First().Phone.Should().Contain("0905");
        totalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchUsersAsync_ReturnsFilteredUsers_ByEmail()
    {
        // Arrange
        using var ctx = NewCtx(nameof(SearchUsersAsync_ReturnsFilteredUsers_ByEmail));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);
        var request = new SearchUserRequest
        {
            Email = "john",
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (users, totalCount) = await repo.SearchUsersAsync(request, CancellationToken.None);

        // Assert
        users.Should().NotBeNull();
        users.Should().HaveCount(1);
        users.First().Email.Should().Contain("john");
        totalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchUsersAsync_ReturnsFilteredUsers_ByRole()
    {
        // Arrange
        using var ctx = NewCtx(nameof(SearchUsersAsync_ReturnsFilteredUsers_ByRole));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);
        var request = new SearchUserRequest
        {
            Role = "Patient",
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (users, totalCount) = await repo.SearchUsersAsync(request, CancellationToken.None);

        // Assert
        users.Should().NotBeNull();
        users.Should().HaveCount(2); // Two patients in seed data
        users.All(u => u.Role!.RoleName == "Patient").Should().BeTrue();
        totalCount.Should().Be(2);
    }

    [Fact]
    public async Task SearchUsersAsync_ReturnsFilteredUsers_ByIsActive()
    {
        // Arrange
        using var ctx = NewCtx(nameof(SearchUsersAsync_ReturnsFilteredUsers_ByIsActive));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);
        var request = new SearchUserRequest
        {
            IsActive = true,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (users, totalCount) = await repo.SearchUsersAsync(request, CancellationToken.None);

        // Assert
        users.Should().NotBeNull();
        users.All(u => u.IsActive == true).Should().BeTrue();
        totalCount.Should().Be(3); // Three active users in seed data
    }

    [Fact]
    public async Task SearchUsersAsync_ReturnsPaginatedResults()
    {
        // Arrange
        using var ctx = NewCtx(nameof(SearchUsersAsync_ReturnsPaginatedResults));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);
        var request = new SearchUserRequest
        {
            PageNumber = 1,
            PageSize = 2
        };

        // Act
        var (users, totalCount) = await repo.SearchUsersAsync(request, CancellationToken.None);

        // Assert
        users.Should().NotBeNull();
        users.Should().HaveCount(2);
        totalCount.Should().Be(4); // Total users in seed data
    }

    [Fact]
    public async Task SearchUsersAsync_ReturnsEmptyList_WhenNoMatches()
    {
        // Arrange
        using var ctx = NewCtx(nameof(SearchUsersAsync_ReturnsEmptyList_WhenNoMatches));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);
        var request = new SearchUserRequest
        {
            FullName = "NonExistent",
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (users, totalCount) = await repo.SearchUsersAsync(request, CancellationToken.None);

        // Assert
        users.Should().NotBeNull();
        users.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    // ===== GetAllPatientsAsync Tests =====

    [Fact]
    public async Task GetAllPatientsAsync_ReturnsActivePatients()
    {
        // Arrange
        using var ctx = NewCtx(nameof(GetAllPatientsAsync_ReturnsActivePatients));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);

        // Act
        var result = await repo.GetAllPatientsAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Two active patients in seed data
        result.All(u => u.Role!.RoleName == "Patient").Should().BeTrue();
        result.All(u => u.IsActive == true).Should().BeTrue();
        result.Should().Contain(u => u.UserId == 1);
        result.Should().Contain(u => u.UserId == 3);
    }

    [Fact]
    public async Task GetAllPatientsAsync_ReturnsEmptyList_WhenNoActivePatients()
    {
        // Arrange
        using var ctx = NewCtx(nameof(GetAllPatientsAsync_ReturnsEmptyList_WhenNoActivePatients));
        var repo = new AdministratorRepository(ctx);

        // Act
        var result = await repo.GetAllPatientsAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllPatientsAsync_ExcludesInactivePatients()
    {
        // Arrange
        using var ctx = NewCtx(nameof(GetAllPatientsAsync_ExcludesInactivePatients));
        await SeedTestDataAsync(ctx);
        var repo = new AdministratorRepository(ctx);

        // Act
        var result = await repo.GetAllPatientsAsync(CancellationToken.None);

        // Assert
        result.Should().NotContain(u => u.UserId == 2); // User 2 is inactive
    }
}

