using FluentAssertions;
using Moq;
using SEP490_BE.BLL.Services;
using SEP490_BE.DAL.DTOs.MedicineDTO;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.Tests.Services;

public class MedicineServiceTests
{
    private readonly Mock<IMedicineRepository> _repositoryMock = new(MockBehavior.Strict);
    private readonly MedicineService _service;

    public MedicineServiceTests()
    {
        _service = new MedicineService(_repositoryMock.Object);
    }

    // Helper method to create a Medicine entity
    private static Medicine MakeMedicine(int id, int providerId, string name, string status = "Providing")
    {
        return new Medicine
        {
            MedicineId = id,
            ProviderId = providerId,
            MedicineName = name,
            Status = status,
            ActiveIngredient = "Test Ingredient",
            Strength = "100mg",
            DosageForm = "Tablet",
            Route = "Oral",
            PrescriptionUnit = "Tablet",
            TherapeuticClass = "Test Class",
            PackSize = "Box of 10",
            Provider = new PharmacyProvider
            {
                ProviderId = providerId,
                UserId = 10,
                User = new User
                {
                    UserId = 10,
                    FullName = "Provider Name"
                }
            }
        };
    }

    // ===== GetAllMedicineAsync Tests =====

    [Fact]
    public async Task GetAllMedicineAsync_ReturnsAllMedicines()
    {
        var medicines = new List<Medicine>
        {
            MakeMedicine(1, 1, "Medicine 1"),
            MakeMedicine(2, 1, "Medicine 2")
        };
        _repositoryMock.Setup(r => r.GetAllMedicineAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(medicines);

        var result = await _service.GetAllMedicineAsync(CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().MedicineId.Should().Be(1);
        _repositoryMock.VerifyAll();
    }

    [Fact]
    public async Task GetAllMedicineAsync_ReturnsEmptyList_WhenNoMedicines()
    {
        _repositoryMock.Setup(r => r.GetAllMedicineAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Medicine>());

        var result = await _service.GetAllMedicineAsync(CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _repositoryMock.VerifyAll();
    }

    // ===== GetMedicineByIdAsync Tests =====

    [Fact]
    public async Task GetMedicineByIdAsync_ReturnsMedicine_WhenExists()
    {
        var medicine = MakeMedicine(1, 1, "Test Medicine");
        _repositoryMock.Setup(r => r.GetMedicineByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(medicine);

        var result = await _service.GetMedicineByIdAsync(1, CancellationToken.None);

        result.Should().NotBeNull();
        result!.MedicineId.Should().Be(1);
        result.MedicineName.Should().Be("Test Medicine");
        _repositoryMock.VerifyAll();
    }

    [Fact]
    public async Task GetMedicineByIdAsync_ReturnsNull_WhenNotExists()
    {
        _repositoryMock.Setup(r => r.GetMedicineByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Medicine?)null);

        var result = await _service.GetMedicineByIdAsync(999, CancellationToken.None);

        result.Should().BeNull();
        _repositoryMock.VerifyAll();
    }

    // ===== CreateMedicineAsync Tests =====

    [Fact]
    public async Task CreateMedicineAsync_CreatesMedicine_WhenValid()
    {
        var dto = new CreateMedicineDto
        {
            MedicineName = "New Medicine",
            ActiveIngredient = "Test Ingredient",
            Strength = "100mg",
            DosageForm = "Tablet",
            Route = "Oral",
            PrescriptionUnit = "Tablet",
            TherapeuticClass = "Test Class",
            PackSize = "Box of 10",
            Status = "Providing"
        };

        Medicine? capturedMedicine = null;
        _repositoryMock.Setup(r => r.CreateMedicineAsync(It.IsAny<Medicine>(), It.IsAny<CancellationToken>()))
            .Callback<Medicine, CancellationToken>((m, ct) => capturedMedicine = m)
            .Returns(Task.CompletedTask);

        await _service.CreateMedicineAsync(dto, 1, CancellationToken.None);

        capturedMedicine.Should().NotBeNull();
        capturedMedicine!.MedicineName.Should().Be("New Medicine");
        capturedMedicine.ProviderId.Should().Be(1);
        capturedMedicine.Status.Should().Be("Providing");
        _repositoryMock.VerifyAll();
    }

    [Fact]
    public async Task CreateMedicineAsync_ThrowsException_WhenMedicineNameEmpty()
    {
        var dto = new CreateMedicineDto
        {
            MedicineName = "",
            ActiveIngredient = "Test Ingredient",
            Strength = "100mg",
            DosageForm = "Tablet",
            Route = "Oral",
            PrescriptionUnit = "Tablet",
            TherapeuticClass = "Test Class",
            PackSize = "Box of 10"
        };

        await _service.Invoking(s => s.CreateMedicineAsync(dto, 1, CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*MedicineName is required*");
        _repositoryMock.Verify(r => r.CreateMedicineAsync(It.IsAny<Medicine>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateMedicineAsync_ThrowsException_WhenMedicineNameTooLong()
    {
        var dto = new CreateMedicineDto
        {
            MedicineName = new string('A', 201), // Exceeds 200 characters
            ActiveIngredient = "Test Ingredient",
            Strength = "100mg",
            DosageForm = "Tablet",
            Route = "Oral",
            PrescriptionUnit = "Tablet",
            TherapeuticClass = "Test Class",
            PackSize = "Box of 10"
        };

        await _service.Invoking(s => s.CreateMedicineAsync(dto, 1, CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*cannot exceed 200 characters*");
        _repositoryMock.Verify(r => r.CreateMedicineAsync(It.IsAny<Medicine>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateMedicineAsync_DefaultsStatusToProviding_WhenStatusEmpty()
    {
        var dto = new CreateMedicineDto
        {
            MedicineName = "New Medicine",
            ActiveIngredient = "Test Ingredient",
            Strength = "100mg",
            DosageForm = "Tablet",
            Route = "Oral",
            PrescriptionUnit = "Tablet",
            TherapeuticClass = "Test Class",
            PackSize = "Box of 10",
            Status = "" // Empty status
        };

        Medicine? capturedMedicine = null;
        _repositoryMock.Setup(r => r.CreateMedicineAsync(It.IsAny<Medicine>(), It.IsAny<CancellationToken>()))
            .Callback<Medicine, CancellationToken>((m, ct) => capturedMedicine = m)
            .Returns(Task.CompletedTask);

        await _service.CreateMedicineAsync(dto, 1, CancellationToken.None);

        capturedMedicine.Should().NotBeNull();
        capturedMedicine!.Status.Should().Be("Providing");
        _repositoryMock.VerifyAll();
    }

    // ===== UpdateMineAsync Tests =====

    [Fact]
    public async Task UpdateMineAsync_UpdatesMedicine_WhenValid()
    {
        var existingMedicine = MakeMedicine(1, 1, "Original Name");
        var dto = new UpdateMedicineDto
        {
            MedicineName = "Updated Name",
            Status = "Stopped"
        };

        _repositoryMock.Setup(r => r.GetProviderIdByUserIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)1);
        _repositoryMock.Setup(r => r.GetMedicineByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMedicine);

        Medicine? capturedMedicine = null;
        _repositoryMock.Setup(r => r.UpdateMedicineAsync(It.IsAny<Medicine>(), It.IsAny<CancellationToken>()))
            .Callback<Medicine, CancellationToken>((m, ct) => capturedMedicine = m)
            .Returns(Task.CompletedTask);

        await _service.UpdateMineAsync(10, 1, dto, CancellationToken.None);

        capturedMedicine.Should().NotBeNull();
        capturedMedicine!.MedicineName.Should().Be("Updated Name");
        capturedMedicine.Status.Should().Be("Stopped");
        _repositoryMock.VerifyAll();
    }

    [Fact]
    public async Task UpdateMineAsync_ThrowsException_WhenUserNotProvider()
    {
        var dto = new UpdateMedicineDto { MedicineName = "Updated Name" };

        _repositoryMock.Setup(r => r.GetProviderIdByUserIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)null);

        await _service.Invoking(s => s.UpdateMineAsync(999, 1, dto, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not a provider*");
        _repositoryMock.Verify(r => r.UpdateMedicineAsync(It.IsAny<Medicine>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateMineAsync_ThrowsException_WhenMedicineNotFound()
    {
        var dto = new UpdateMedicineDto { MedicineName = "Updated Name" };

        _repositoryMock.Setup(r => r.GetProviderIdByUserIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)1);
        _repositoryMock.Setup(r => r.GetMedicineByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Medicine?)null);

        await _service.Invoking(s => s.UpdateMineAsync(10, 999, dto, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
        _repositoryMock.Verify(r => r.UpdateMedicineAsync(It.IsAny<Medicine>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateMineAsync_ThrowsException_WhenNotOwner()
    {
        var existingMedicine = MakeMedicine(1, 2, "Medicine"); // Belongs to provider 2
        var dto = new UpdateMedicineDto { MedicineName = "Updated Name" };

        _repositoryMock.Setup(r => r.GetProviderIdByUserIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)1); // User 10 is provider 1
        _repositoryMock.Setup(r => r.GetMedicineByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMedicine); // Medicine belongs to provider 2

        await _service.Invoking(s => s.UpdateMineAsync(10, 1, dto, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not allowed to update*");
        _repositoryMock.Verify(r => r.UpdateMedicineAsync(It.IsAny<Medicine>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateMineAsync_ThrowsException_WhenMedicineNameEmpty()
    {
        var existingMedicine = MakeMedicine(1, 1, "Original Name");
        var dto = new UpdateMedicineDto { MedicineName = "   " }; // Whitespace only

        _repositoryMock.Setup(r => r.GetProviderIdByUserIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)1);
        _repositoryMock.Setup(r => r.GetMedicineByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMedicine);

        await _service.Invoking(s => s.UpdateMineAsync(10, 1, dto, CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*cannot be empty or whitespace*");
        _repositoryMock.Verify(r => r.UpdateMedicineAsync(It.IsAny<Medicine>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ===== GetProviderIdByUserIdAsync Tests =====

    [Fact]
    public async Task GetProviderIdByUserIdAsync_ReturnsProviderId_WhenExists()
    {
        _repositoryMock.Setup(r => r.GetProviderIdByUserIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)1);

        var result = await _service.GetProviderIdByUserIdAsync(10, CancellationToken.None);

        result.Should().Be(1);
        _repositoryMock.VerifyAll();
    }

    [Fact]
    public async Task GetProviderIdByUserIdAsync_ReturnsNull_WhenNotExists()
    {
        _repositoryMock.Setup(r => r.GetProviderIdByUserIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)null);

        var result = await _service.GetProviderIdByUserIdAsync(999, CancellationToken.None);

        result.Should().BeNull();
        _repositoryMock.VerifyAll();
    }

    // ===== GetMinePagedAsync Tests =====

    [Fact]
    public async Task GetMinePagedAsync_ReturnsPagedResults()
    {
        var medicines = new List<Medicine>
        {
            MakeMedicine(1, 1, "Medicine 1"),
            MakeMedicine(2, 1, "Medicine 2")
        };

        _repositoryMock.Setup(r => r.GetProviderIdByUserIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)1);
        _repositoryMock.Setup(r => r.GetByProviderIdPagedAsync(1, 1, 10, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((medicines, 2));

        var result = await _service.GetMinePagedAsync(10, 1, 10, null, null, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        _repositoryMock.VerifyAll();
    }

    [Fact]
    public async Task GetMinePagedAsync_ThrowsException_WhenUserNotProvider()
    {
        _repositoryMock.Setup(r => r.GetProviderIdByUserIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)null);

        await _service.Invoking(s => s.GetMinePagedAsync(999, 1, 10, null, null, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not a provider*");
        _repositoryMock.Verify(r => r.GetByProviderIdPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetMinePagedAsync_LimitsPageSizeTo100()
    {
        var medicines = new List<Medicine>();

        _repositoryMock.Setup(r => r.GetProviderIdByUserIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)1);
        _repositoryMock.Setup(r => r.GetByProviderIdPagedAsync(1, 1, 100, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((medicines, 0));

        await _service.GetMinePagedAsync(10, 1, 200, null, null, CancellationToken.None); // Request 200, should be limited to 100

        _repositoryMock.Verify(r => r.GetByProviderIdPagedAsync(1, 1, 100, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ===== GenerateExcelTemplateAsync Tests =====

    [Fact]
    public async Task GenerateExcelTemplateAsync_ReturnsByteArray()
    {
        var result = await _service.GenerateExcelTemplateAsync(CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Length.Should().BeGreaterThan(0);
    }

    // ===== ImportFromExcelAsync Tests =====

    [Fact]
    public async Task ImportFromExcelAsync_ThrowsException_WhenUserNotProvider()
    {
        var stream = new MemoryStream();

        _repositoryMock.Setup(r => r.GetProviderIdByUserIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)null);

        await _service.Invoking(s => s.ImportFromExcelAsync(999, stream, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not a provider*");
    }
}
