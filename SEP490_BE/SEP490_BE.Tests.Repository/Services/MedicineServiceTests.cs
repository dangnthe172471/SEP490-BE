using FluentAssertions;
using Moq;
using SEP490_BE.BLL.Services;
using SEP490_BE.DAL.DTOs.MedicineDTO;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.Tests.Services
{
    public class MedicineServiceTests
    {
        private readonly Mock<IMedicineRepository> _repo = new();

        // 🔧 Helper: Tạo entity Medicine kèm Provider.User để test mapping
        private static Medicine MakeMed(int id, int providerId, string name, string? status = "Providing", string? sideEffects = null, string providerName = "Prov A")
            => new Medicine
            {
                MedicineId = id,
                ProviderId = providerId,
                MedicineName = name,
                Status = status,
                SideEffects = sideEffects,
                Provider = new PharmacyProvider
                {
                    ProviderId = providerId,
                    User = new User { FullName = providerName, Phone = "0900", PasswordHash = "x", RoleId = 1, Role = new Role { RoleId = 1, RoleName = "Pharmacy Provider" } }
                }
            };

        // ✅ Test: GetProviderIdByUserIdAsync forward thẳng xuống repository
        [Fact]
        public async Task GetProviderIdByUserIdAsync_Forwards_To_Repository()
        {
            _repo.Setup(r => r.GetProviderIdByUserIdAsync(123, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(10);

            var svc = new MedicineService(_repo.Object);

            var result = await svc.GetProviderIdByUserIdAsync(123);

            result.Should().Be(10);
            _repo.Verify(r => r.GetProviderIdByUserIdAsync(123, It.IsAny<CancellationToken>()), Times.Once);
        }

        // ✅ Test: GetAllAsync map entity → ReadMedicineDto, gồm ProviderName
        [Fact]
        public async Task GetAllAsync_Maps_Entities_To_Dtos_With_ProviderName()
        {
            var data = new List<Medicine>
            {
                MakeMed(1, 10, "A", "Providing", null, "Prov X"),
                MakeMed(2, 10, "B", "Stopped",   null, "Prov X")
            };

            _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(data);

            var svc = new MedicineService(_repo.Object);

            var list = await svc.GetAllAsync();

            list.Should().HaveCount(2);
            list[0].MedicineId.Should().Be(1);
            list[0].MedicineName.Should().Be("A");
            list[0].ProviderId.Should().Be(10);
            list[0].ProviderName.Should().Be("Prov X");
            list[1].Status.Should().Be("Stopped");
        }

        // ✅ Test: GetByIdAsync trả về null khi không tìm thấy
        [Fact]
        public async Task GetByIdAsync_Returns_Null_When_NotFound()
        {
            _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Medicine?)null);

            var svc = new MedicineService(_repo.Object);

            var dto = await svc.GetByIdAsync(999);

            dto.Should().BeNull();
        }

        // ✅ Test: GetByIdAsync map đúng khi tìm thấy
        [Fact]
        public async Task GetByIdAsync_Maps_When_Found()
        {
            var med = MakeMed(5, 10, "Metformin", "Providing", "Nausea", "Prov Z");
            _repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(med);

            var svc = new MedicineService(_repo.Object);

            var dto = await svc.GetByIdAsync(5);

            dto.Should().NotBeNull();
            dto!.MedicineId.Should().Be(5);
            dto.MedicineName.Should().Be("Metformin");
            dto.SideEffects.Should().Be("Nausea");
            dto.Status.Should().Be("Providing");
            dto.ProviderName.Should().Be("Prov Z");
        }

        // ✅ Test: CreateAsync ném lỗi nếu MedicineName null/whitespace
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateAsync_Throws_When_Name_Is_Null_Or_Whitespace(string? badName)
        {
            var svc = new MedicineService(_repo.Object);
            var dto = new CreateMedicineDto { MedicineName = badName, SideEffects = null, Status = null };

            var act = async () => await svc.CreateAsync(dto, providerId: 10);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Medicine name is required*");
            _repo.Verify(r => r.CreateAsync(It.IsAny<Medicine>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ✅ Test: CreateAsync dùng Status = "Available" khi dto.Status null/whitespace, và Trim tên
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateAsync_Defaults_Status_To_Available_And_Trims_Name_When_Status_Empty(string? dtoStatus)
        {
            var svc = new MedicineService(_repo.Object);
            var dto = new CreateMedicineDto { MedicineName = "  New Med  ", SideEffects = "n/a", Status = dtoStatus };

            await svc.CreateAsync(dto, providerId: 7);

            _repo.Verify(r => r.CreateAsync(
                It.Is<Medicine>(m =>
                    m.ProviderId == 7 &&
                    m.MedicineName == "New Med" &&
                    m.SideEffects == "n/a" &&
                    m.Status == "Available"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // ✅ Test: CreateAsync dùng Status truyền vào (đã Trim)
        [Fact]
        public async Task CreateAsync_Uses_Provided_Status_Trimmed()
        {
            var svc = new MedicineService(_repo.Object);
            var dto = new CreateMedicineDto { MedicineName = "  Paracetamol  ", SideEffects = null, Status = "  Providing " };

            await svc.CreateAsync(dto, providerId: 8);

            _repo.Verify(r => r.CreateAsync(
                It.Is<Medicine>(m =>
                    m.ProviderId == 8 &&
                    m.MedicineName == "Paracetamol" &&
                    m.Status == "Providing"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // ✅ Test: UpdateAsync ném KeyNotFound khi không tìm thấy entity theo id
        [Fact]
        public async Task UpdateAsync_Throws_KeyNotFound_When_Entity_NotFound()
        {
            _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Medicine?)null);

            var svc = new MedicineService(_repo.Object);
            var dto = new UpdateMedicineDto { MedicineName = "X" };

            var act = async () => await svc.UpdateAsync(999, dto);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                     .WithMessage("*999*");
            _repo.Verify(r => r.UpdateAsync(It.IsAny<Medicine>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ✅ Test: UpdateAsync cập nhật field (trim tên, giữ nguyên nếu null) và gọi repo.UpdateAsync
        [Fact]
        public async Task UpdateAsync_Updates_Fields_And_Calls_Repo_Update()
        {
            var existing = MakeMed(3, 10, "Old", "Providing", "SE", "Prov X");
            _repo.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

            var svc = new MedicineService(_repo.Object);
            var dto = new UpdateMedicineDto
            {
                MedicineName = "  New Name  ",
                SideEffects = null,         // -> giữ nguyên "SE"
                Status = "Stopped"
            };

            await svc.UpdateAsync(3, dto);

            _repo.Verify(r => r.UpdateAsync(
                It.Is<Medicine>(m =>
                    m.MedicineId == 3 &&
                    m.MedicineName == "New Name" &&
                    m.SideEffects == "SE" &&
                    m.Status == "Stopped" &&
                    m.ProviderId == 10
                ),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // ✅ Test: GetByProviderIdAsync map list sang DTO kèm ProviderName
        [Fact]
        public async Task GetByProviderIdAsync_Maps_List_To_Dtos()
        {
            var list = new List<Medicine>
            {
                MakeMed(1, 10, "A", "Providing", null, "Prov X"),
                MakeMed(2, 10, "B", "Stopped",   null, "Prov X"),
            };

            _repo.Setup(r => r.GetByProviderIdAsync(10, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(list);

            var svc = new MedicineService(_repo.Object);

            var dtos = await svc.GetByProviderIdAsync(10);

            dtos.Should().HaveCount(2);
            dtos[0].ProviderName.Should().Be("Prov X");
            dtos[1].Status.Should().Be("Stopped");
        }

        // ✅ Test: SoftDeleteAsync chỉ cần forward xuống repo
        [Fact]
        public async Task SoftDeleteAsync_Forwards_To_Repository()
        {
            var svc = new MedicineService(_repo.Object);

            await svc.SoftDeleteAsync(77);

            _repo.Verify(r => r.SoftDeleteAsync(77, It.IsAny<CancellationToken>()), Times.Once);
        }

        // ✅ Test: GetMineAsync ném InvalidOperation khi user không phải provider
        [Fact]
        public async Task GetMineAsync_Throws_When_User_Not_Provider()
        {
            _repo.Setup(r => r.GetProviderIdByUserIdAsync(555, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((int?)null);

            var svc = new MedicineService(_repo.Object);

            var act = async () => await svc.GetMineAsync(555);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*không phải là nhà cung cấp*");
        }

        // ✅ Test: GetMineAsync trả list đã map khi user là provider
        [Fact]
        public async Task GetMineAsync_Returns_Mapped_List_When_User_Is_Provider()
        {
            _repo.Setup(r => r.GetProviderIdByUserIdAsync(101, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(10);

            var meds = new List<Medicine>
            {
                MakeMed(1, 10, "M1", "Providing", null, "Prov X"),
                MakeMed(2, 10, "M2", "Stopped",   null, "Prov X"),
            };
            _repo.Setup(r => r.GetByProviderIdAsync(10, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(meds);

            var svc = new MedicineService(_repo.Object);

            var list = await svc.GetMineAsync(101);

            list.Should().HaveCount(2);
            list.Select(i => i.MedicineName).Should().Contain(new[] { "M1", "M2" });
            list.All(i => i.ProviderId == 10).Should().BeTrue();
        }

        // ✅ Test: GetMinePagedAsync normalize status/sort, pageNumber/pageSize, và map DTOs
        [Fact]
        public async Task GetMinePagedAsync_Normalizes_Inputs_And_Maps()
        {
            // user → provider
            _repo.Setup(r => r.GetProviderIdByUserIdAsync(123, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(10);

            // repo → trả danh sách + total
            var items = new List<Medicine>
            {
                MakeMed(1, 10, "Alpha", "Providing", null, "Prov X"),
                MakeMed(2, 10, "Beta",  "Providing", null, "Prov X"),
            };
            _repo.Setup(r => r.GetByProviderIdPagedAsync(10, 1, 2, "Providing", "az", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((items, 2));

            var svc = new MedicineService(_repo.Object);

            // status "providing" (lower) → "Providing"; sort "AZ"/mixed-case → "az"
            var result = await svc.GetMinePagedAsync(
                userId: 123, pageNumber: 1, pageSize: 2,
                status: "providing", sort: "AZ"
            );

            result.TotalCount.Should().Be(2);
            result.Items.Should().HaveCount(2);
            result.Items.First().ProviderName.Should().Be("Prov X");

            _repo.Verify(r => r.GetByProviderIdPagedAsync(10, 1, 2, "Providing", "az", It.IsAny<CancellationToken>()), Times.Once);
        }

        // ✅ Test: GetMinePagedAsync cap pageSize<=100 và pageNumber>=1
        [Fact]
        public async Task GetMinePagedAsync_Clamps_PageSize_And_PageNumber()
        {
            _repo.Setup(r => r.GetProviderIdByUserIdAsync(123, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(10);

            // Khi service clamp: pageNumber<1 → 1; pageSize>100 → 100
            _repo.Setup(r => r.GetByProviderIdPagedAsync(10, 1, 100, null, null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((new List<Medicine>(), 0));

            var svc = new MedicineService(_repo.Object);

            var result = await svc.GetMinePagedAsync(
                userId: 123, pageNumber: -5, pageSize: 9999, status: "unknown", sort: "unknown"
            );

            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(100);

            _repo.Verify(r => r.GetByProviderIdPagedAsync(10, 1, 100, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        // ✅ Test: GetMinePagedAsync ném lỗi nếu user không phải provider
        [Fact]
        public async Task GetMinePagedAsync_Throws_When_User_Not_Provider()
        {
            _repo.Setup(r => r.GetProviderIdByUserIdAsync(999, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((int?)null);

            var svc = new MedicineService(_repo.Object);

            var act = async () => await svc.GetMinePagedAsync(999, 1, 10);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*không phải là nhà cung cấp*");
        }
    }
}
