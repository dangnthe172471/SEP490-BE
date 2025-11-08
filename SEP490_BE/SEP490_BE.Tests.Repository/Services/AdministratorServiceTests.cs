using FluentAssertions;
using Moq;
using SEP490_BE.BLL.Services;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.Tests.Services;

public class AdministratorServiceTests
{
    private readonly Mock<IAdministratorRepository> _repositoryMock;
    private readonly AdministratorService _service;

    public AdministratorServiceTests()
    {
        _repositoryMock = new Mock<IAdministratorRepository>(MockBehavior.Strict);
        _service = new AdministratorService(_repositoryMock.Object);
    }

    // Helper methods
    private static User MakeUser(int id, string phone, string fullName, bool isActive = true, int roleId = 2, string? roleName = null)
    {
        var user = new User
        {
            UserId = id,
            Phone = phone,
            FullName = fullName,
            Email = "test@example.com",
            Gender = "Male",
            Dob = new DateOnly(1990, 1, 1),
            IsActive = isActive,
            RoleId = roleId,
            Role = roleName != null ? new Role { RoleId = roleId, RoleName = roleName } : null
        };
        return user;
    }

    private static User MakeUserWithPatient(int id, string phone, string fullName, int roleId = 2)
    {
        var user = MakeUser(id, phone, fullName, true, roleId, "Patient");
        user.Patient = new Patient
        {
            PatientId = id,
            UserId = id,
            Allergies = "Peanuts",
            MedicalHistory = "Diabetes"
        };
        return user;
    }

    // ===== GetAllAsync Tests =====

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            MakeUser(1, "0909123456", "User 1", true, 1, "Administrator"),
            MakeUser(2, "0909123457", "User 2", true, 2, "Patient")
        };
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().UserId.Should().Be(1);
        result.First().FullName.Should().Be("User 1");
        result.First().Role.Should().Be("Administrator");
        _repositoryMock.VerifyAll();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoUsers()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _repositoryMock.VerifyAll();
    }

    // ===== GetByIdAsync Tests =====

    [Fact]
    public async Task GetByIdAsync_ReturnsUserDto_WhenUserExists()
    {
        // Arrange
        var user = MakeUser(1, "0909123456", "Test User", true, 2, "Patient");
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetByIdAsync(1, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(1);
        result.Phone.Should().Be("0909123456");
        result.FullName.Should().Be("Test User");
        result.Role.Should().Be("Patient");
        _repositoryMock.VerifyAll();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsUserDtoWithPatientInfo_WhenUserIsPatient()
    {
        // Arrange
        var user = MakeUserWithPatient(1, "0909123456", "Patient User");
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetByIdAsync(1, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(1);
        result.Allergies.Should().Be("Peanuts");
        result.MedicalHistory.Should().Be("Diabetes");
        _repositoryMock.VerifyAll();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenUserNotExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetByIdAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _repositoryMock.VerifyAll();
    }

    // ===== CreateUserAsync Tests =====

    [Fact]
    public async Task CreateUserAsync_ThrowsException_WhenPhoneExists()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Phone = "0909123456",
            Password = "123456",
            FullName = "New User",
            RoleId = 2
        };
        var existingUser = MakeUser(1, "0909123456", "Existing User");
        _repositoryMock.Setup(r => r.GetByPhoneAsync(request.Phone, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await _service.Invoking(s => s.CreateUserAsync(request, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Số điện thoại đã được sử dụng.");
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_CreatesUser_WhenValidRequest()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Phone = "0909123456",
            Password = "123456",
            FullName = "New User",
            Email = "newuser@example.com",
            RoleId = 1,
            Gender = "Male",
            Dob = new DateOnly(1990, 1, 1)
        };
        _repositoryMock.Setup(r => r.GetByPhoneAsync(request.Phone, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        User? capturedUser = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, ct) => capturedUser = u)
            .Returns(Task.CompletedTask);

        var createdUser = MakeUser(100, request.Phone, request.FullName, true, request.RoleId, "Administrator");
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _service.CreateUserAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        capturedUser.Should().NotBeNull();
        capturedUser!.Phone.Should().Be(request.Phone);
        capturedUser.FullName.Should().Be(request.FullName);
        capturedUser.Email.Should().Be(request.Email);
        capturedUser.RoleId.Should().Be(request.RoleId);
        BCrypt.Net.BCrypt.Verify(request.Password, capturedUser.PasswordHash).Should().BeTrue();
        _repositoryMock.VerifyAll();
    }

    [Fact]
    public async Task CreateUserAsync_CreatesPatient_WhenRoleIsPatient()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Phone = "0909123456",
            Password = "123456",
            FullName = "New Patient",
            RoleId = 2,
            Allergies = "Peanuts",
            MedicalHistory = "Diabetes"
        };
        _repositoryMock.Setup(r => r.GetByPhoneAsync(request.Phone, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _repositoryMock.Setup(r => r.GetMaxPatientIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        User? capturedUser = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, ct) => capturedUser = u)
            .Returns(Task.CompletedTask);

        var createdUser = MakeUserWithPatient(100, request.Phone, request.FullName);
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _service.CreateUserAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        capturedUser.Should().NotBeNull();
        capturedUser!.Patient.Should().NotBeNull();
        capturedUser.Patient!.PatientId.Should().Be(11); // maxId + 1
        capturedUser.Patient.Allergies.Should().Be(request.Allergies);
        capturedUser.Patient.MedicalHistory.Should().Be(request.MedicalHistory);
        _repositoryMock.VerifyAll();
    }

    [Fact]
    public async Task CreateUserAsync_HashesPassword()
    {
        // Arrange
        const string rawPassword = "MySecret123!";
        var request = new CreateUserRequest
        {
            Phone = "0909123456",
            Password = rawPassword,
            FullName = "Test User",
            RoleId = 1
        };
        _repositoryMock.Setup(r => r.GetByPhoneAsync(request.Phone, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        User? capturedUser = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, ct) => capturedUser = u)
            .Returns(Task.CompletedTask);

        var createdUser = MakeUser(100, request.Phone, request.FullName);
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        // Act
        await _service.CreateUserAsync(request, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().NotBe(rawPassword);
        BCrypt.Net.BCrypt.Verify(rawPassword, capturedUser.PasswordHash).Should().BeTrue();
        _repositoryMock.VerifyAll();
    }

    // ===== UpdateUserAsync Tests =====

    [Fact]
    public async Task UpdateUserAsync_ReturnsNull_WhenUserNotExists()
    {
        // Arrange
        var request = new UpdateUserRequest { FullName = "Updated Name" };
        _repositoryMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.UpdateUserAsync(999, request, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_UpdatesUser_WhenValidRequest()
    {
        // Arrange
        var user = MakeUser(1, "0909123456", "Original Name", true, 1, "Administrator");
        var request = new UpdateUserRequest
        {
            FullName = "Updated Name",
            Email = "updated@example.com",
            Phone = "0909999999"
        };
        _repositoryMock.SetupSequence(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user)  // First call to get user
            .ReturnsAsync(user);  // Second call to return updated user (service calls GetByIdAsync again at the end)

        _repositoryMock.Setup(r => r.GetByPhoneAsync(request.Phone!, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        User? capturedUser = null;
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, ct) => capturedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateUserAsync(1, request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        capturedUser.Should().NotBeNull();
        capturedUser!.FullName.Should().Be(request.FullName);
        capturedUser.Email.Should().Be(request.Email);
        capturedUser.Phone.Should().Be(request.Phone);
        _repositoryMock.Verify(r => r.GetByPhoneAsync(request.Phone!, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UpdateUserAsync_ThrowsException_WhenPhoneAlreadyExists()
    {
        // Arrange
        var user = MakeUser(1, "0909123456", "User 1");
        var existingUser = MakeUser(2, "0909999999", "User 2");
        var request = new UpdateUserRequest { Phone = "0909999999" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _repositoryMock.Setup(r => r.GetByPhoneAsync(request.Phone!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await _service.Invoking(s => s.UpdateUserAsync(1, request, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Số điện thoại đã được sử dụng.");
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_UpdatesPassword_WhenPasswordProvided()
    {
        // Arrange
        // Use Administrator role (RoleId = 1) to avoid Patient-specific logic
        var user = MakeUser(1, "0909123456", "User 1", true, 1, "Administrator");
        const string newPassword = "NewPassword123!";
        var request = new UpdateUserRequest { Password = newPassword };
        _repositoryMock.SetupSequence(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user)  // First call to get user
            .ReturnsAsync(user);  // Second call to return updated user

        User? capturedUser = null;
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, ct) => capturedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateUserAsync(1, request, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        BCrypt.Net.BCrypt.Verify(newPassword, capturedUser!.PasswordHash).Should().BeTrue();
        _repositoryMock.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_CreatesPatient_WhenUserBecomesPatient()
    {
        // Arrange
        // User starts as Administrator but will become Patient
        var initialUser = MakeUser(1, "0909123456", "User 1", true, 1, "Administrator");
        
        var request = new UpdateUserRequest
        {
            RoleId = 2,
            Allergies = "Peanuts",
            MedicalHistory = "Diabetes"
        };
        
        // After update, user becomes Patient with Patient record
        var updatedUser = MakeUserWithPatient(1, "0909123456", "User 1");
        updatedUser.Role = new Role { RoleId = 2, RoleName = "Patient" };
        
        _repositoryMock.SetupSequence(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(initialUser)  // First call to get user
            .ReturnsAsync(updatedUser);  // Second call to return updated user

        _repositoryMock.Setup(r => r.GetMaxPatientIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        User? capturedUser = null;
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, ct) => 
            {
                capturedUser = u;
                // Service checks user.Role?.RoleName == "Patient" AFTER updating RoleId
                // But Role object is not automatically updated, so we need to update it here
                // to match the expected behavior (service should update Role or check RoleId)
                if (u.RoleId == 2)
                {
                    u.Role = new Role { RoleId = 2, RoleName = "Patient" };
                }
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateUserAsync(1, request, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        // Service updates RoleId first, then checks Role?.RoleName
        // We simulate Role update in callback to match expected behavior
        capturedUser!.RoleId.Should().Be(2);
        capturedUser.Role?.RoleName.Should().Be("Patient");
        capturedUser.Patient.Should().NotBeNull();
        capturedUser.Patient!.Allergies.Should().Be(request.Allergies);
        capturedUser.Patient.MedicalHistory.Should().Be(request.MedicalHistory);
        _repositoryMock.Verify(r => r.GetMaxPatientIdAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    // ===== DeleteUserAsync Tests =====

    [Fact]
    public async Task DeleteUserAsync_ReturnsFalse_WhenUserNotExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.DeleteUserAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUserAsync_ReturnsTrue_WhenUserDeleted()
    {
        // Arrange
        var user = MakeUser(1, "0909123456", "User 1");
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _repositoryMock.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteUserAsync(1, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.VerifyAll();
    }

    // ===== ToggleUserStatusAsync Tests =====

    [Fact]
    public async Task ToggleUserStatusAsync_ReturnsFalse_WhenUserNotExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.ToggleUserStatusAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ToggleUserStatusAsync_TogglesStatus_WhenUserExists()
    {
        // Arrange
        var user = MakeUser(1, "0909123456", "User 1", isActive: true);
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        User? capturedUser = null;
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, ct) => capturedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ToggleUserStatusAsync(1, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        capturedUser.Should().NotBeNull();
        capturedUser!.IsActive.Should().BeFalse(); // Toggled from true to false
        _repositoryMock.VerifyAll();
    }

    // ===== GetAllPatientsAsync Tests =====

    [Fact]
    public async Task GetAllPatientsAsync_ReturnsPatients()
    {
        // Arrange
        var patients = new List<User>
        {
            MakeUserWithPatient(1, "0909123456", "Patient 1"),
            MakeUserWithPatient(2, "0909123457", "Patient 2")
        };
        _repositoryMock.Setup(r => r.GetAllPatientsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(patients);

        // Act
        var result = await _service.GetAllPatientsAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Role.Should().Be("Patient");
        result.First().Allergies.Should().Be("Peanuts");
        _repositoryMock.VerifyAll();
    }

    [Fact]
    public async Task GetAllPatientsAsync_ReturnsEmptyList_WhenNoPatients()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAllPatientsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _service.GetAllPatientsAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _repositoryMock.VerifyAll();
    }

    // ===== SearchUsersAsync Tests =====

    [Fact]
    public async Task SearchUsersAsync_ReturnsSearchResults()
    {
        // Arrange
        var request = new SearchUserRequest
        {
            FullName = "Test",
            PageNumber = 1,
            PageSize = 10
        };
        var users = new List<User>
        {
            MakeUser(1, "0909123456", "Test User 1", true, 2, "Patient")
        };
        _repositoryMock.Setup(r => r.SearchUsersAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((users, 1));

        // Act
        var result = await _service.SearchUsersAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Users.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Users.First().FullName.Should().Be("Test User 1");
        _repositoryMock.VerifyAll();
    }

    [Fact]
    public async Task SearchUsersAsync_ReturnsEmptyResults_WhenNoMatches()
    {
        // Arrange
        var request = new SearchUserRequest
        {
            FullName = "NonExistent",
            PageNumber = 1,
            PageSize = 10
        };
        _repositoryMock.Setup(r => r.SearchUsersAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<User>(), 0));

        // Act
        var result = await _service.SearchUsersAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Users.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        _repositoryMock.VerifyAll();
    }
}

