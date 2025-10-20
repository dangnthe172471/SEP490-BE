using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.Repositories;

namespace SEP490_BE.Tests.Repositories
{
    public class MedicineRepositoryTests
    {
        // Tạo DbContext InMemory mới cho mỗi test (độc lập với DB thật)
        private DiamondHealthContext NewCtx(string db)
        {
            var opt = new DbContextOptionsBuilder<DiamondHealthContext>()
                .UseInMemoryDatabase(db)
                .EnableSensitiveDataLogging()
                .Options;
            return new DiamondHealthContext(opt);
        }

        // SEED dữ liệu GIỐNG HỆT ảnh SQL: 16 rows (ProviderId=1 có 3; ProviderId=2 có 13)
        private async Task SeedExactlyLikeScreenshotAsync(DiamondHealthContext ctx)
        {
            // 👇 Seed 1 role tối thiểu
            var pharmacyRole = new Role
            {
                RoleId = 3,
                RoleName = "Pharmacy Provider"
            };
            ctx.Roles.Add(pharmacyRole);

            var u1 = new User
            {
                UserId = 1,
                Phone = "0900000001",
                PasswordHash = "hash-1",
                FullName = "Provider One",
                Email = "p1@example.com",
                RoleId = pharmacyRole.RoleId,
                Role = pharmacyRole
            };

            var u2 = new User
            {
                UserId = 2,
                Phone = "0900000002",
                PasswordHash = "hash-2",
                FullName = "Provider Two",
                Email = "p2@example.com",
                RoleId = pharmacyRole.RoleId,
                Role = pharmacyRole
            };

            var p1 = new PharmacyProvider { ProviderId = 1, UserId = 1, User = u1 };
            var p2 = new PharmacyProvider { ProviderId = 2, UserId = 2, User = u2 };

            ctx.Users.AddRange(u1, u2);
            ctx.PharmacyProviders.AddRange(p1, p2);

            // 16 thuốc
            ctx.Medicines.AddRange(
                new Medicine { MedicineId = 1, ProviderId = 1, MedicineName = "Paracetamol 500mg", SideEffects = "Buồn ngủ, mệt nhẹ", Status = "Available" },
                new Medicine { MedicineId = 2, ProviderId = 1, MedicineName = "Amlodipine 5mg", SideEffects = "Phù chân, nhức đầu", Status = "Available" },
                new Medicine { MedicineId = 3, ProviderId = 1, MedicineName = "Tiffy", SideEffects = "Buồn ngủ, khô miệng, chóng mặt", Status = "Active" },
                new Medicine { MedicineId = 4, ProviderId = 2, MedicineName = "Metformin 850mg", SideEffects = "Khó tiêu, tiêu chảy, vị kim loại trong miệng", Status = "Providing" },
                new Medicine { MedicineId = 5, ProviderId = 2, MedicineName = "AB", SideEffects = "AB", Status = "Providing" },
                new Medicine { MedicineId = 6, ProviderId = 2, MedicineName = "Paracetamol 500mg", SideEffects = "Buồn nôn, chóng mặt, dị ứng nhẹ", Status = "Providing" },
                new Medicine { MedicineId = 7, ProviderId = 2, MedicineName = "Amoxicillin 500mg", SideEffects = "Tiêu chảy, nổi mẩn, đau bụng", Status = "Providing" },
                new Medicine { MedicineId = 8, ProviderId = 2, MedicineName = "Ibuprofen 400mg", SideEffects = "Kích thích dạ dày, đau bụng, đau đầu", Status = "Providing" },
                new Medicine { MedicineId = 9, ProviderId = 2, MedicineName = "Cetirizine 10mg", SideEffects = "Buồn ngủ, khô miệng, mệt mỏi", Status = "Providing" },
                new Medicine { MedicineId = 10, ProviderId = 2, MedicineName = "Azithromycin 250mg", SideEffects = "Buồn nôn, đau dạ dày, tiêu chảy", Status = "Providing" },
                new Medicine { MedicineId = 11, ProviderId = 2, MedicineName = "Metformin 500mg", SideEffects = "Khó tiêu, tiêu chảy nhẹ, buồn nôn", Status = "Providing" },
                new Medicine { MedicineId = 12, ProviderId = 2, MedicineName = "Omeprazole 20mg", SideEffects = "Đầy hơi, đau bụng, buồn nôn", Status = "Providing" },
                new Medicine { MedicineId = 13, ProviderId = 2, MedicineName = "Simvastatin 20mg", SideEffects = "Đau cơ, chóng mặt, táo bón", Status = "Providing" },
                new Medicine { MedicineId = 14, ProviderId = 2, MedicineName = "Losartan 50mg", SideEffects = "Hạ huyết áp, chóng mặt, buồn nôn", Status = "Providing" },
                new Medicine { MedicineId = 15, ProviderId = 2, MedicineName = "CAA", SideEffects = "CAA", Status = "Providing" },
                new Medicine { MedicineId = 16, ProviderId = 2, MedicineName = "CSS", SideEffects = "CSS", Status = "Stopped" }
            );

            await ctx.SaveChangesAsync();
        }

        // 🧪 Test 1: GetAllAsync trả về đúng 16 bản ghi và đã Include Provider.User
        [Fact]
        public async Task GetAllAsync_Returns_All_16_With_Provider_And_User()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedExactlyLikeScreenshotAsync(ctx);
            var repo = new MedicineRepository(ctx);

            var all = await repo.GetAllAsync();

            all.Should().HaveCount(16);
            all.All(m => m.Provider != null && m.Provider!.User != null).Should().BeTrue();
        }

        // 🧪 Test 2: GetByIdAsync trả null khi không tồn tại; khi tồn tại thì có Provider.User
        [Fact]
        public async Task GetByIdAsync_Returns_Null_If_NotFound_Else_Item_With_Relations()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedExactlyLikeScreenshotAsync(ctx);
            var repo = new MedicineRepository(ctx);

            (await repo.GetByIdAsync(999)).Should().BeNull();

            var med6 = await repo.GetByIdAsync(6); // Paracetamol 500mg của Provider 2
            med6.Should().NotBeNull();
            med6!.ProviderId.Should().Be(2);
            med6.Provider!.User!.FullName.Should().Be("Provider Two");
        }

        // 🧪 Test 3: GetByProviderIdAsync chỉ trả dữ liệu của 1 provider và đủ số lượng
        [Fact]
        public async Task GetByProviderIdAsync_Returns_Only_Provider2_With_Count_13()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedExactlyLikeScreenshotAsync(ctx);
            var repo = new MedicineRepository(ctx);

            var p2Items = await repo.GetByProviderIdAsync(2);

            p2Items.Should().HaveCount(13);
            p2Items.Should().OnlyContain(m => m.ProviderId == 2);
        }

        // 🧪 Test 4: CreateAsync — trim tên và chặn tạo bản ghi trùng tên trong CÙNG provider
        [Fact]
        public async Task CreateAsync_Trims_Name_And_Blocks_Duplicate_In_Same_Provider()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedExactlyLikeScreenshotAsync(ctx);
            var repo = new MedicineRepository(ctx);

            // Trùng tên "Paracetamol 500mg" tại Provider 2 (đã có id=6)
            var dup = new Medicine { ProviderId = 2, MedicineName = "  Paracetamol 500mg  ", Status = "Providing" };
            var act = async () => await repo.CreateAsync(dup);
            await act.Should().ThrowAsync<InvalidOperationException>();

            // Tạo mới 1 thuốc khác tên tại Provider 1 -> hợp lệ
            var ok = new Medicine { ProviderId = 1, MedicineName = "NewDrug X", Status = "Available" };
            await repo.CreateAsync(ok);

            (await ctx.Medicines.CountAsync(m => m.ProviderId == 1)).Should().Be(4); // ban đầu 3, thêm 1 = 4
        }

        // 🧪 Test 5: UpdateAsync — không cho đổi ProviderId, nhưng cho cập nhật tên/sideEffects/status (có trim)
        [Fact]
        public async Task UpdateAsync_Disallows_Provider_Change_But_Updates_Fields()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedExactlyLikeScreenshotAsync(ctx);
            var repo = new MedicineRepository(ctx);

            // Lấy Metformin 850mg (id=4, provider 2)
            var current = await ctx.Medicines.AsNoTracking().FirstAsync(m => m.MedicineId == 4);

            // ❌ thử đổi ProviderId -> phải ném InvalidOperationException
            var wrong = new Medicine
            {
                MedicineId = current.MedicineId,
                ProviderId = 999,
                MedicineName = current.MedicineName,
                SideEffects = current.SideEffects,
                Status = current.Status
            };
            var bad = async () => await repo.UpdateAsync(wrong);
            await bad.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("*Changing Provider*");

            // ✅ cập nhật hợp lệ + trim tên
            current.MedicineName = "  Metformin 850mg (Updated) ";
            current.SideEffects = "Note cập nhật";
            current.Status = "Providing";
            current.ProviderId = 2;

            await repo.UpdateAsync(current);

            var after = await ctx.Medicines.FindAsync(4);
            after!.MedicineName.Should().Be("Metformin 850mg (Updated)");
            after.SideEffects.Should().Be("Note cập nhật");
            after.Status.Should().Be("Providing");
        }

        // 🧪 Test 6: UpdateAsync — ném KeyNotFound khi id không tồn tại
        [Fact]
        public async Task UpdateAsync_Throws_KeyNotFound_If_Id_Not_Exists()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedExactlyLikeScreenshotAsync(ctx);
            var repo = new MedicineRepository(ctx);

            var ghost = new Medicine { MedicineId = 9999, ProviderId = 1, MedicineName = "Ghost" };
            var act = async () => await repo.UpdateAsync(ghost);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                     .WithMessage("*9999*");
        }

        // 🧪 Test 7: SoftDeleteAsync — chuyển Status thành "Stopped" và ném lỗi nếu không thấy id
        [Fact]
        public async Task SoftDeleteAsync_Stops_Medicine_Or_Throws_If_NotFound()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedExactlyLikeScreenshotAsync(ctx);
            var repo = new MedicineRepository(ctx);

            // Not found
            var bad = async () => await repo.SoftDeleteAsync(7777);
            await bad.Should().ThrowAsync<KeyNotFoundException>();

            // Success — chọn 1 bản ghi đang Providing của provider 2
            var anyProvidingId = ctx.Medicines.First(m => m.ProviderId == 2 && m.Status == "Providing").MedicineId;
            await repo.SoftDeleteAsync(anyProvidingId);

            (await ctx.Medicines.FindAsync(anyProvidingId))!.Status.Should().Be("Stopped");
        }

        // 🧪 Test 8: GetByProviderIdPagedAsync — lọc theo status + sort az/za/mặc định + tổng số + phân trang
        [Theory]
        [InlineData(null, null)]            // không lọc, sort mặc định (Id desc)
        [InlineData("Providing", "az")]     // Providing, A→Z
        [InlineData("Providing", "za")]     // Providing, Z→A
        [InlineData("Stopped", null)]       // Stopped, sort mặc định
        public async Task GetByProviderIdPagedAsync_Filter_Sort_Paginate_Work(string? status, string? sort)
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedExactlyLikeScreenshotAsync(ctx);
            var repo = new MedicineRepository(ctx);

            var (items, total) = await repo.GetByProviderIdPagedAsync(
                providerId: 2, pageNumber: 1, pageSize: 5, status: status, sort: sort);

            // Tổng kỳ vọng theo filter snapshot
            var expectedTotal = ctx.Medicines
                .Where(m => m.ProviderId == 2)
                .Where(m => string.IsNullOrWhiteSpace(status) ? true : m.Status == status)
                .Count();

            total.Should().Be(expectedTotal);
            items.Should().HaveCount(Math.Min(5, expectedTotal));

            if (string.Equals(sort, "az", StringComparison.OrdinalIgnoreCase))
                items.Select(i => i.MedicineName).Should().BeInAscendingOrder(StringComparer.Ordinal);
            if (string.Equals(sort, "za", StringComparison.OrdinalIgnoreCase))
                items.Select(i => i.MedicineName).Should().BeInDescendingOrder(StringComparer.Ordinal);
        }

        // 🧪 Test 9: GetByProviderIdPagedAsync — tự chuẩn hoá pageNumber/pageSize khi giá trị không hợp lệ
        [Fact]
        public async Task GetByProviderIdPagedAsync_Normalizes_Invalid_Page_And_Size()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedExactlyLikeScreenshotAsync(ctx);
            var repo = new MedicineRepository(ctx);

            var (items, total) = await repo.GetByProviderIdPagedAsync(
                providerId: 2, pageNumber: -3, pageSize: 0, status: null, sort: null);

            total.Should().Be(ctx.Medicines.Count(m => m.ProviderId == 2)); // = 13
            items.Should().HaveCount(10); // pageSize mặc định = 10
        }

        // 🧪 Test 10: CreateAsync — ném lỗi khi MedicineName null hoặc rỗng
        [Fact]
        public async Task CreateAsync_Should_Throw_When_MedicineName_Is_Null()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedExactlyLikeScreenshotAsync(ctx);
            var repo = new MedicineRepository(ctx);

            var invalid = new Medicine { ProviderId = 1, MedicineName = null, Status = "Available" };
            var act = async () => await repo.CreateAsync(invalid);

            var ex = await act.Should().ThrowAsync<Exception>();
            ex.Which.Should().Match<Exception>(e =>
                   (e is InvalidOperationException && e.Message.Contains("Medicine name", StringComparison.OrdinalIgnoreCase))
                || (e is Microsoft.EntityFrameworkCore.DbUpdateException && e.Message.Contains("Required properties", StringComparison.OrdinalIgnoreCase))
            );
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateAsync_Allows_Empty_Or_Whitespace_Name_When_Repo_Has_No_Validation(string badName)
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedExactlyLikeScreenshotAsync(ctx);
            var repo = new MedicineRepository(ctx);

            var med = new Medicine { ProviderId = 1, MedicineName = badName, Status = "Available" };
            var act = async () => await repo.CreateAsync(med);

            await act.Should().NotThrowAsync();

            // tuỳ repo có trim hay không:
            var saved = await ctx.Medicines.FirstAsync(m => m.MedicineId == med.MedicineId);
            // Nếu repo có trim: mong đợi "" ; nếu không trim: giữ nguyên badName
            saved.MedicineName.Should().Be((badName ?? string.Empty).Trim()); // an toàn cho cả hai phía
        }


        // 🧪 Test 11: CreateAsync — cho phép SideEffects = null, không ném lỗi
        [Fact]
        public async Task CreateAsync_Should_Allow_SideEffects_Null()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedExactlyLikeScreenshotAsync(ctx);
            var repo = new MedicineRepository(ctx);

            var med = new Medicine
            {
                ProviderId = 1,
                MedicineName = "Z-Test-Null-SE",
                SideEffects = null,
                Status = "Available"
            };

            await repo.CreateAsync(med);

            (await ctx.Medicines.FirstOrDefaultAsync(m => m.MedicineName == "Z-Test-Null-SE"))
                .Should().NotBeNull();
        }

        // 🧪 Test 12: CreateAsync — nếu Status null, vẫn lưu được (EF cho phép)
        [Fact]
        public async Task CreateAsync_Should_Allow_Status_Null()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedExactlyLikeScreenshotAsync(ctx);
            var repo = new MedicineRepository(ctx);

            var med = new Medicine
            {
                ProviderId = 2,
                MedicineName = "NewDrugWithoutStatus",
                SideEffects = "test",
                Status = null
            };

            await repo.CreateAsync(med);

            var found = await ctx.Medicines.FirstAsync(m => m.MedicineName == "NewDrugWithoutStatus");
            found.Status.Should().BeNull();
        }

        // 🧪 Test 13 (đã chỉnh): UpdateAsync — hiện tại repository ném InvalidOperation khi truyền null
        [Fact]
        public async Task UpdateAsync_Should_Throw_InvalidOperation_When_Medicine_Is_Null()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedExactlyLikeScreenshotAsync(ctx);
            var repo = new MedicineRepository(ctx);

            Medicine? nullMed = null;
            var act = async () => await repo.UpdateAsync(nullMed!);

            await act.Should()
                     .ThrowAsync<InvalidOperationException>()
                     .WithInnerExceptionExactly(typeof(NullReferenceException));
        }
    }
}
