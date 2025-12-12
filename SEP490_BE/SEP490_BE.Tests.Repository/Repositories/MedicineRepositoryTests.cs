using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.Repositories;

namespace SEP490_BE.Tests.Repositories;

public class MedicineRepositoryTests
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
            new Role { RoleId = 6, RoleName = "Pharmacy Provider" }
        };
        ctx.Roles.AddRange(roles);

        // Seed Users
        var users = new List<User>
        {
            new User
            {
                UserId = 10,
                Phone = "0909123456",
                FullName = "Pharmacy Provider 1",
                Email = "provider1@example.com",
                RoleId = 6,
                IsActive = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456")
            },
            new User
            {
                UserId = 11,
                Phone = "0909123457",
                FullName = "Pharmacy Provider 2",
                Email = "provider2@example.com",
                RoleId = 6,
                IsActive = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456")
            }
        };
        ctx.Users.AddRange(users);

        // Seed PharmacyProviders
        var providers = new List<PharmacyProvider>
        {
            new PharmacyProvider { ProviderId = 1, UserId = 10, Contact = "Contact 1" },
            new PharmacyProvider { ProviderId = 2, UserId = 11, Contact = "Contact 2" }
        };
        ctx.PharmacyProviders.AddRange(providers);

        // Seed Medicines
        var medicines = new List<Medicine>
        {
            new Medicine
            {
                MedicineId = 1,
                ProviderId = 1,
                MedicineName = "Paracetamol 500mg",
                Status = "Providing",
                ActiveIngredient = "Paracetamol",
                Strength = "500mg",
                DosageForm = "Tablet",
                Route = "Oral",
                PrescriptionUnit = "Tablet",
                TherapeuticClass = "Analgesic",
                PackSize = "Box of 10"
            },
            new Medicine
            {
                MedicineId = 2,
                ProviderId = 1,
                MedicineName = "Amoxicillin 500mg",
                Status = "Providing",
                ActiveIngredient = "Amoxicillin",
                Strength = "500mg",
                DosageForm = "Capsule",
                Route = "Oral",
                PrescriptionUnit = "Capsule",
                TherapeuticClass = "Antibiotic",
                PackSize = "Box of 20"
            },
            new Medicine
            {
                MedicineId = 3,
                ProviderId = 2,
                MedicineName = "Ibuprofen 400mg",
                Status = "Stopped",
                ActiveIngredient = "Ibuprofen",
                Strength = "400mg",
                DosageForm = "Tablet",
                Route = "Oral",
                PrescriptionUnit = "Tablet",
                TherapeuticClass = "NSAID",
                PackSize = "Box of 10"
            }
        };
        ctx.Medicines.AddRange(medicines);

        await ctx.SaveChangesAsync();
    }

    // ===== GetAllMedicineAsync Tests =====

    [Fact]
    public async Task GetAllMedicineAsync_ReturnsAllMedicines()
    {
        using var ctx = NewCtx(nameof(GetAllMedicineAsync_ReturnsAllMedicines));
        await SeedTestDataAsync(ctx);
        var repo = new MedicineRepository(ctx);

        var result = await repo.GetAllMedicineAsync(CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(m => m.MedicineId == 1);
        result.Should().Contain(m => m.MedicineId == 2);
        result.Should().Contain(m => m.MedicineId == 3);
    }

    [Fact]
    public async Task GetAllMedicineAsync_ReturnsEmptyList_WhenNoMedicines()
    {
        using var ctx = NewCtx(nameof(GetAllMedicineAsync_ReturnsEmptyList_WhenNoMedicines));
        var repo = new MedicineRepository(ctx);

        var result = await repo.GetAllMedicineAsync(CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    // ===== GetMedicineByIdAsync Tests =====

    [Fact]
    public async Task GetMedicineByIdAsync_ReturnsMedicine_WhenExists()
    {
        using var ctx = NewCtx(nameof(GetMedicineByIdAsync_ReturnsMedicine_WhenExists));
        await SeedTestDataAsync(ctx);
        var repo = new MedicineRepository(ctx);

        var result = await repo.GetMedicineByIdAsync(1, CancellationToken.None);

        result.Should().NotBeNull();
        result!.MedicineId.Should().Be(1);
        result.MedicineName.Should().Be("Paracetamol 500mg");
        result.ProviderId.Should().Be(1);
    }

    [Fact]
    public async Task GetMedicineByIdAsync_ReturnsNull_WhenNotExists()
    {
        using var ctx = NewCtx(nameof(GetMedicineByIdAsync_ReturnsNull_WhenNotExists));
        await SeedTestDataAsync(ctx);
        var repo = new MedicineRepository(ctx);

        var result = await repo.GetMedicineByIdAsync(999, CancellationToken.None);

        result.Should().BeNull();
    }

    // ===== CreateMedicineAsync Tests =====

    [Fact]
    public async Task CreateMedicineAsync_CreatesMedicine_WhenValid()
    {
        using var ctx = NewCtx(nameof(CreateMedicineAsync_CreatesMedicine_WhenValid));
        await SeedTestDataAsync(ctx);
        var repo = new MedicineRepository(ctx);

        var newMedicine = new Medicine
        {
            ProviderId = 1,
            MedicineName = "New Medicine",
            Status = "Providing",
            ActiveIngredient = "Test Ingredient",
            Strength = "100mg",
            DosageForm = "Tablet",
            Route = "Oral",
            PrescriptionUnit = "Tablet",
            TherapeuticClass = "Test Class",
            PackSize = "Box of 5"
        };

        await repo.CreateMedicineAsync(newMedicine, CancellationToken.None);

        var saved = await ctx.Medicines.FirstOrDefaultAsync(m => m.MedicineName == "New Medicine");
        saved.Should().NotBeNull();
        saved!.MedicineName.Should().Be("New Medicine");
    }

    [Fact]
    public async Task CreateMedicineAsync_ThrowsException_WhenDuplicateNameForSameProvider()
    {
        using var ctx = NewCtx(nameof(CreateMedicineAsync_ThrowsException_WhenDuplicateNameForSameProvider));
        await SeedTestDataAsync(ctx);
        var repo = new MedicineRepository(ctx);

        var duplicateMedicine = new Medicine
        {
            ProviderId = 1,
            MedicineName = "Paracetamol 500mg", // Same name as existing medicine for provider 1
            Status = "Providing",
            ActiveIngredient = "Paracetamol",
            Strength = "500mg",
            DosageForm = "Tablet",
            Route = "Oral",
            PrescriptionUnit = "Tablet",
            TherapeuticClass = "Analgesic",
            PackSize = "Box of 10"
        };

        await repo.Invoking(r => r.CreateMedicineAsync(duplicateMedicine, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists for this provider*");
    }

    [Fact]
    public async Task CreateMedicineAsync_AllowsSameName_ForDifferentProviders()
    {
        using var ctx = NewCtx(nameof(CreateMedicineAsync_AllowsSameName_ForDifferentProviders));
        await SeedTestDataAsync(ctx);
        var repo = new MedicineRepository(ctx);

        var newMedicine = new Medicine
        {
            ProviderId = 2, // Different provider
            MedicineName = "Paracetamol 500mg", // Same name but different provider
            Status = "Providing",
            ActiveIngredient = "Paracetamol",
            Strength = "500mg",
            DosageForm = "Tablet",
            Route = "Oral",
            PrescriptionUnit = "Tablet",
            TherapeuticClass = "Analgesic",
            PackSize = "Box of 10"
        };

        await repo.CreateMedicineAsync(newMedicine, CancellationToken.None);

        var saved = await ctx.Medicines.FirstOrDefaultAsync(m => m.ProviderId == 2 && m.MedicineName == "Paracetamol 500mg");
        saved.Should().NotBeNull();
    }

    // ===== UpdateMedicineAsync Tests =====

    [Fact]
    public async Task UpdateMedicineAsync_UpdatesMedicine_WhenValid()
    {
        using var ctx = NewCtx(nameof(UpdateMedicineAsync_UpdatesMedicine_WhenValid));
        await SeedTestDataAsync(ctx);
        var repo = new MedicineRepository(ctx);

        var updatedMedicine = new Medicine
        {
            MedicineId = 1,
            ProviderId = 1,
            MedicineName = "Updated Paracetamol",
            Status = "Stopped",
            ActiveIngredient = "Paracetamol",
            Strength = "500mg",
            DosageForm = "Tablet",
            Route = "Oral",
            PrescriptionUnit = "Tablet",
            TherapeuticClass = "Analgesic",
            PackSize = "Box of 10"
        };

        await repo.UpdateMedicineAsync(updatedMedicine, CancellationToken.None);

        var updated = await ctx.Medicines.FirstOrDefaultAsync(m => m.MedicineId == 1);
        updated.Should().NotBeNull();
        updated!.MedicineName.Should().Be("Updated Paracetamol");
        updated.Status.Should().Be("Stopped");
    }

    [Fact]
    public async Task UpdateMedicineAsync_ThrowsException_WhenMedicineNotFound()
    {
        using var ctx = NewCtx(nameof(UpdateMedicineAsync_ThrowsException_WhenMedicineNotFound));
        await SeedTestDataAsync(ctx);
        var repo = new MedicineRepository(ctx);

        var nonExistentMedicine = new Medicine
        {
            MedicineId = 999,
            ProviderId = 1,
            MedicineName = "Non Existent",
            Status = "Providing"
        };

        await repo.Invoking(r => r.UpdateMedicineAsync(nonExistentMedicine, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task UpdateMedicineAsync_ThrowsException_WhenChangingProvider()
    {
        using var ctx = NewCtx(nameof(UpdateMedicineAsync_ThrowsException_WhenChangingProvider));
        await SeedTestDataAsync(ctx);
        var repo = new MedicineRepository(ctx);

        var medicineWithChangedProvider = new Medicine
        {
            MedicineId = 1,
            ProviderId = 2, // Changed provider
            MedicineName = "Paracetamol 500mg",
            Status = "Providing"
        };

        await repo.Invoking(r => r.UpdateMedicineAsync(medicineWithChangedProvider, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Changing Provider is not allowed*");
    }

    // ===== GetProviderIdByUserIdAsync Tests =====

    [Fact]
    public async Task GetProviderIdByUserIdAsync_ReturnsProviderId_WhenExists()
    {
        using var ctx = NewCtx(nameof(GetProviderIdByUserIdAsync_ReturnsProviderId_WhenExists));
        await SeedTestDataAsync(ctx);
        var repo = new MedicineRepository(ctx);

        var result = await repo.GetProviderIdByUserIdAsync(10, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().Be(1);
    }

    [Fact]
    public async Task GetProviderIdByUserIdAsync_ReturnsNull_WhenNotExists()
    {
        using var ctx = NewCtx(nameof(GetProviderIdByUserIdAsync_ReturnsNull_WhenNotExists));
        await SeedTestDataAsync(ctx);
        var repo = new MedicineRepository(ctx);

        var result = await repo.GetProviderIdByUserIdAsync(999, CancellationToken.None);

        result.Should().BeNull();
    }

    // ===== GetByProviderIdPagedAsync Tests =====

    [Fact]
    public async Task GetByProviderIdPagedAsync_ReturnsPagedResults()
    {
        using var ctx = NewCtx(nameof(GetByProviderIdPagedAsync_ReturnsPagedResults));
        await SeedTestDataAsync(ctx);
        var repo = new MedicineRepository(ctx);

        var (items, totalCount) = await repo.GetByProviderIdPagedAsync(1, 1, 10, null, null, CancellationToken.None);

        items.Should().NotBeNull();
        items.Should().HaveCount(2); // Provider 1 has 2 medicines
        totalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetByProviderIdPagedAsync_FiltersByStatus()
    {
        using var ctx = NewCtx(nameof(GetByProviderIdPagedAsync_FiltersByStatus));
        await SeedTestDataAsync(ctx);
        var repo = new MedicineRepository(ctx);

        var (items, totalCount) = await repo.GetByProviderIdPagedAsync(2, 1, 10, "Stopped", null, CancellationToken.None);

        items.Should().NotBeNull();
        items.Should().HaveCount(1);
        items.First().Status.Should().Be("Stopped");
        totalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetByProviderIdPagedAsync_SortsByName_AZ()
    {
        using var ctx = NewCtx(nameof(GetByProviderIdPagedAsync_SortsByName_AZ));
        await SeedTestDataAsync(ctx);
        var repo = new MedicineRepository(ctx);

        var (items, totalCount) = await repo.GetByProviderIdPagedAsync(1, 1, 10, null, "az", CancellationToken.None);

        items.Should().NotBeNull();
        items.Should().HaveCount(2);
        items.First().MedicineName.Should().Be("Amoxicillin 500mg"); // A comes before P
    }

    [Fact]
    public async Task GetByProviderIdPagedAsync_SortsByName_ZA()
    {
        using var ctx = NewCtx(nameof(GetByProviderIdPagedAsync_SortsByName_ZA));
        await SeedTestDataAsync(ctx);
        var repo = new MedicineRepository(ctx);

        var (items, totalCount) = await repo.GetByProviderIdPagedAsync(1, 1, 10, null, "za", CancellationToken.None);

        items.Should().NotBeNull();
        items.Should().HaveCount(2);
        items.First().MedicineName.Should().Be("Paracetamol 500mg"); // P comes after A
    }
}
