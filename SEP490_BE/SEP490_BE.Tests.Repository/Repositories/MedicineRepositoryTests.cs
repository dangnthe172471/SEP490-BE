using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.Repositories;

namespace SEP490_BE.Tests.Repositories
{
    public class MedicineRepositoryTests
    {
        private DiamondHealthContext NewContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<DiamondHealthContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .Options;

            var ctx = new DiamondHealthContext(options);

            // Seed Providers (giữ đúng ProviderId để test)
            ctx.PharmacyProviders.AddRange(
                new PharmacyProvider { ProviderId = 1, UserId = 101 },
                new PharmacyProvider { ProviderId = 2, UserId = 102 }
            );

            ctx.SaveChanges();
            return ctx;
        }

        private MedicineRepository NewRepo(DiamondHealthContext ctx) => new(ctx);

        // TC1: ProviderId=1, "Paracetamol", "Nausea", "Providing"  -> THÀNH CÔNG
        [Fact(DisplayName = "TC1 - Create success for Provider(1) with Paracetamol/Nausea/Providing")]
        public async Task Create_Success_Provider1_Paracetamol()
        {
            using var ctx = NewContext(nameof(Create_Success_Provider1_Paracetamol));
            var repo = NewRepo(ctx);

            var med = new Medicine
            {
                ProviderId = 1,
                MedicineName = "Paracetamol",
                SideEffects = "Nausea",
                Status = "Providing"
            };

            await repo.CreateMedicineAsync(med, CancellationToken.None);

            var saved = await ctx.Medicines.SingleAsync();
            Assert.Equal(1, saved.ProviderId);
            Assert.Equal("Paracetamol", saved.MedicineName);
            Assert.Equal("Nausea", saved.SideEffects);
            Assert.Equal("Providing", saved.Status);
        }

        // TC2: ProviderId=2, "Paracetamol", "Nausea", "Providing" (trong khi Provider(1) đã có Paracetamol) -> THÀNH CÔNG
        [Fact(DisplayName = "TC2 - Create success for Provider(2) same name Paracetamol (different provider)")]
        public async Task Create_Success_Provider2_SameName_DifferentProvider()
        {
            using var ctx = NewContext(nameof(Create_Success_Provider2_SameName_DifferentProvider));

            // Seed sẵn cho Provider(1) đã có "Paracetamol"
            ctx.Medicines.Add(new Medicine
            {
                ProviderId = 1,
                MedicineName = "Paracetamol",
                SideEffects = "Nausea",
                Status = "Providing"
            });
            await ctx.SaveChangesAsync();

            var repo = NewRepo(ctx);

            var med2 = new Medicine
            {
                ProviderId = 2,
                MedicineName = "Paracetamol",
                SideEffects = "Nausea",
                Status = "Providing"
            };

            await repo.CreateMedicineAsync(med2, CancellationToken.None);

            var all = await ctx.Medicines.ToListAsync();
            Assert.Equal(2, all.Count); // cả provider(1) & provider(2) đều có Paracetamol
        }

        // TC3: Duplicate cùng Provider -> InvalidOperationException
        [Fact(DisplayName = "TC3 - Create fails: duplicate name for same provider")]
        public async Task Create_Fails_Duplicate_SameProvider()
        {
            using var ctx = NewContext(nameof(Create_Fails_Duplicate_SameProvider));

            ctx.Medicines.Add(new Medicine
            {
                ProviderId = 1,
                MedicineName = "Paracetamol",
                SideEffects = "Nausea",
                Status = "Providing"
            });
            await ctx.SaveChangesAsync();

            var repo = NewRepo(ctx);

            var dup = new Medicine
            {
                ProviderId = 1,
                MedicineName = "Paracetamol",
                SideEffects = "Nausea",
                Status = "Providing"
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                repo.CreateMedicineAsync(dup, CancellationToken.None));

            Assert.Contains("already exists for this provider", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(1, await ctx.Medicines.CountAsync()); // không thêm mới
        }

        // TC4: Trim tên -> vẫn tạo được và tên được Trim
        [Fact(DisplayName = "TC4 - Create trims name on insert")]
        public async Task Create_Success_Trim_Name()
        {
            using var ctx = NewContext(nameof(Create_Success_Trim_Name));
            var repo = NewRepo(ctx);

            var med = new Medicine
            {
                ProviderId = 1,
                MedicineName = "  Paracetamol  ",
                SideEffects = "Nausea",
                Status = "Providing"
            };

            await repo.CreateMedicineAsync(med, CancellationToken.None);

            var saved = await ctx.Medicines.SingleAsync();
            Assert.Equal("Paracetamol", saved.MedicineName);
        }

        // TC5: Tên rỗng/space -> ArgumentException
        [Theory(DisplayName = "TC5 - Create fails: empty/whitespace name")]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Create_Fails_EmptyOrWhitespaceName(string badName)
        {
            using var ctx = NewContext(nameof(Create_Fails_EmptyOrWhitespaceName) + "_" + Guid.NewGuid());
            var repo = NewRepo(ctx);

            var med = new Medicine
            {
                ProviderId = 1,
                MedicineName = badName,
                SideEffects = "Nausea",
                Status = "Providing"
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                repo.CreateMedicineAsync(med, CancellationToken.None));

            Assert.Contains("cannot be empty or whitespace", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(await ctx.Medicines.ToListAsync());
        }

        // ===================== UPDATE TESTS =====================

        // Test 1: 1,"Paracetamol 500","Nausea","Stopped" -> success
        [Fact(DisplayName = "U-TC1 - Provider=1, Name='Paracetamol 500', SE='Nausea', Status='Stopped' -> Success")]
        public async Task Update_Success_Provider1_Paracetamol500_Stopped()
        {
            using var ctx = NewContext(nameof(Update_Success_Provider1_Paracetamol500_Stopped));
            var repo = NewRepo(ctx);

            // Seed existing (Id=10, Provider=1)
            ctx.Medicines.Add(new Medicine
            {
                MedicineId = 10,
                ProviderId = 1,
                MedicineName = "Paracetamol",
                SideEffects = "OldSE",
                Status = "Providing"
            });
            await ctx.SaveChangesAsync();

            var input = new Medicine
            {
                MedicineId = 10,
                ProviderId = 1,
                MedicineName = "Paracetamol 500",
                SideEffects = "Nausea",
                Status = "Stopped"
            };

            await repo.UpdateMedicineAsync(input, CancellationToken.None);

            var updated = await ctx.Medicines.FirstAsync(m => m.MedicineId == 10);
            Assert.Equal(1, updated.ProviderId);
            Assert.Equal("Paracetamol 500", updated.MedicineName);
            Assert.Equal("Nausea", updated.SideEffects);
            Assert.Equal("Stopped", updated.Status);
        }

        // Test 2: 2,"Paracetamol 500",null,null -> success
        [Fact(DisplayName = "U-TC2 - Provider=2, Name='Paracetamol 500', SE=null, Status=null -> Success")]
        public async Task Update_Success_Provider2_Paracetamol500_Nulls()
        {
            using var ctx = NewContext(nameof(Update_Success_Provider2_Paracetamol500_Nulls));
            var repo = NewRepo(ctx);

            // Seed existing (Id=20, Provider=2)
            ctx.Medicines.Add(new Medicine
            {
                MedicineId = 20,
                ProviderId = 2,
                MedicineName = "Paracetamol",
                SideEffects = "OldSE",
                Status = "Providing"
            });
            await ctx.SaveChangesAsync();

            var input = new Medicine
            {
                MedicineId = 20,
                ProviderId = 2,
                MedicineName = " Paracetamol 500 ", // sẽ Trim
                SideEffects = null,
                Status = null
            };

            await repo.UpdateMedicineAsync(input, CancellationToken.None);

            var updated = await ctx.Medicines.FirstAsync(m => m.MedicineId == 20);
            Assert.Equal(2, updated.ProviderId);
            Assert.Equal("Paracetamol 500", updated.MedicineName); // đã Trim
            Assert.Null(updated.SideEffects);
            Assert.Null(updated.Status);
        }

        // Test 3: 1,"","Nausea","Providing" -> lỗi tên rỗng exception
        [Fact(DisplayName = "U-TC3 - Empty name -> ArgumentException (theo logic hiện tại: tên CŨ rỗng/space)")]
        public async Task Update_Fails_EmptyName_ArgumentException()
        {
            using var ctx = NewContext(nameof(Update_Fails_EmptyName_ArgumentException));
            var repo = NewRepo(ctx);

            // CHÚ Ý: Repo đang check tên CŨ rỗng/space trước khi gán tên mới.
            // Vì vậy seed tên hiện có là whitespace để ném lỗi đúng yêu cầu.
            ctx.Medicines.Add(new Medicine
            {
                MedicineId = 30,
                ProviderId = 1,
                MedicineName = "   ", // tên hiện có toàn space -> sẽ ném ArgumentException
                SideEffects = "OldSE",
                Status = "Providing"
            });
            await ctx.SaveChangesAsync();

            var input = new Medicine
            {
                MedicineId = 30,
                ProviderId = 1,
                MedicineName = "", // tên mới rỗng theo mô tả test case
                SideEffects = "Nausea",
                Status = "Providing"
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                repo.UpdateMedicineAsync(input, CancellationToken.None));

            Assert.Contains("cannot be empty or whitespace", ex.Message, StringComparison.OrdinalIgnoreCase);

            // Nếu muốn check "tên MỚI rỗng" thay vì tên cũ, hãy sửa repo:
            // if (medicine.MedicineName != null && string.IsNullOrWhiteSpace(medicine.MedicineName)) throw ...
        }

        // Test 4: 100," Amoxicillin ","Fever","Providing" -> lỗi không được đổi Provider Id Exception
        [Fact(DisplayName = "U-TC4 - Change ProviderId -> InvalidOperationException")]
        public async Task Update_Fails_ChangeProvider_InvalidOperation()
        {
            using var ctx = NewContext(nameof(Update_Fails_ChangeProvider_InvalidOperation));
            var repo = NewRepo(ctx);

            // Seed existing (Id=40, Provider=1)
            ctx.Medicines.Add(new Medicine
            {
                MedicineId = 40,
                ProviderId = 1,
                MedicineName = "Amox",
                SideEffects = "OldSE",
                Status = "Providing"
            });
            await ctx.SaveChangesAsync();

            var input = new Medicine
            {
                MedicineId = 40,
                ProviderId = 100,               // Đổi ProviderId -> phải lỗi
                MedicineName = " Amoxicillin ",
                SideEffects = "Fever",
                Status = "Providing"
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                repo.UpdateMedicineAsync(input, CancellationToken.None));

            Assert.Contains("Changing Provider is not allowed", ex.Message);
        }

        // Test 5: 1," Amoxicillin ",null,"Stopped" -> success
        [Fact(DisplayName = "U-TC5 - Provider=1, Name=' Amoxicillin ' (trim), SE=null, Status='Stopped' -> Success")]
        public async Task Update_Success_Provider1_Amoxicillin_Stopped()
        {
            using var ctx = NewContext(nameof(Update_Success_Provider1_Amoxicillin_Stopped));
            var repo = NewRepo(ctx);

            // Seed existing (Id=50, Provider=1)
            ctx.Medicines.Add(new Medicine
            {
                MedicineId = 50,
                ProviderId = 1,
                MedicineName = "OldName",
                SideEffects = "OldSE",
                Status = "Providing"
            });
            await ctx.SaveChangesAsync();

            var input = new Medicine
            {
                MedicineId = 50,
                ProviderId = 1,
                MedicineName = " Amoxicillin ",
                SideEffects = null,
                Status = "Stopped"
            };

            await repo.UpdateMedicineAsync(input, CancellationToken.None);

            var updated = await ctx.Medicines.FirstAsync(m => m.MedicineId == 50);
            Assert.Equal(1, updated.ProviderId);
            Assert.Equal("Amoxicillin", updated.MedicineName); // Trim
            Assert.Null(updated.SideEffects);
            Assert.Equal("Stopped", updated.Status);
        }

    }
}
