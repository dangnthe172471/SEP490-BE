using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.Repositories;

namespace SEP490_BE.Tests.Repositories
{
    public class RoomRepositoryTests
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

        // SEED dữ liệu phòng khám mẫu
        private async Task SeedRoomDataAsync(DiamondHealthContext ctx)
        {
            ctx.Rooms.AddRange(
                new Room { RoomId = 1, RoomName = "Phòng khám tổng quát 101" },
                new Room { RoomId = 2, RoomName = "Phòng tim mạch 202" },
                new Room { RoomId = 3, RoomName = "Phòng khám tổng quát 102" },
                new Room { RoomId = 4, RoomName = "Phòng tim mạch 203" },
                new Room { RoomId = 5, RoomName = "Phòng da liễu 301" },
                new Room { RoomId = 6, RoomName = "Phòng nội soi 401" },
                new Room { RoomId = 7, RoomName = "Phòng xét nghiệm 501" },
                new Room { RoomId = 8, RoomName = "Phòng chụp X-quang 601" }
            );

            await ctx.SaveChangesAsync();
        }

        // 🧪 Test 1: GetAllAsync trả về tất cả phòng khám
        [Fact]
        public async Task GetAllAsync_Returns_All_8_Rooms()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedRoomDataAsync(ctx);
            var repo = new RoomRepository(ctx);

            var all = await repo.GetAllAsync();

            all.Should().HaveCount(8);
            all.Should().OnlyContain(r => r.RoomId > 0 && !string.IsNullOrEmpty(r.RoomName));
        }

        // 🧪 Test 2: GetByIdAsync trả null khi không tồn tại; khi tồn tại thì có đầy đủ thông tin
        [Fact]
        public async Task GetByIdAsync_Returns_Null_If_NotFound_Else_Room_With_Doctors()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedRoomDataAsync(ctx);

            // Seed doctors để test Include
            ctx.Roles.Add(new Role { RoleId = 1, RoleName = "Doctor" });
            ctx.Users.Add(new User { UserId = 1, Phone = "0901", PasswordHash = "x", FullName = "Dr. A", RoleId = 1 });
            ctx.Doctors.Add(new Doctor { DoctorId = 1, UserId = 1, Specialty = "Cardiology", ExperienceYears = 5, RoomId = 1 });

            await ctx.SaveChangesAsync();

            var repo = new RoomRepository(ctx);

            (await repo.GetByIdAsync(999)).Should().BeNull();

            var room1 = await repo.GetByIdAsync(1);
            room1.Should().NotBeNull();
            room1!.RoomId.Should().Be(1);
            room1.RoomName.Should().Be("Phòng khám tổng quát 101");
            room1.Doctors.Should().NotBeNull();
        }

        // 🧪 Test 3: GetPagedAsync — phân trang cơ bản
        [Fact]
        public async Task GetPagedAsync_Returns_Paginated_Results()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedRoomDataAsync(ctx);
            var repo = new RoomRepository(ctx);

            var (items, total) = await repo.GetPagedAsync(pageNumber: 1, pageSize: 3);

            total.Should().Be(8);
            items.Should().HaveCount(3);
            items.Should().OnlyContain(r => r.RoomId >= 1 && r.RoomId <= 3);
        }

        // 🧪 Test 4: GetPagedAsync — search term hoạt động
        [Fact]
        public async Task GetPagedAsync_Filters_By_SearchTerm()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedRoomDataAsync(ctx);
            var repo = new RoomRepository(ctx);

            var (items, total) = await repo.GetPagedAsync(pageNumber: 1, pageSize: 10, searchTerm: "tim mạch");

            total.Should().Be(2); // 2 phòng tim mạch
            items.Should().OnlyContain(r => r.RoomName.Contains("tim mạch"));
        }

        // 🧪 Test 5: GetPagedAsync — không có kết quả khi search không tồn tại
        [Fact]
        public async Task GetPagedAsync_Returns_Empty_When_SearchTerm_NotFound()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedRoomDataAsync(ctx);
            var repo = new RoomRepository(ctx);

            var (items, total) = await repo.GetPagedAsync(pageNumber: 1, pageSize: 10, searchTerm: "RoomXYZ");

            total.Should().Be(0);
            items.Should().BeEmpty();
        }

        // 🧪 Test 6: AddAsync — thêm phòng mới
        [Fact]
        public async Task AddAsync_Creates_New_Room()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedRoomDataAsync(ctx);
            var repo = new RoomRepository(ctx);

            var newRoom = new Room { RoomName = "Phòng mới 999" };
            await repo.AddAsync(newRoom);

            var saved = await ctx.Rooms.FirstOrDefaultAsync(r => r.RoomName == "Phòng mới 999");
            saved.Should().NotBeNull();
            saved!.RoomName.Should().Be("Phòng mới 999");
            (await ctx.Rooms.CountAsync()).Should().Be(9);
        }

        // 🧪 Test 7: UpdateAsync — cập nhật tên phòng
        [Fact]
        public async Task UpdateAsync_Updates_RoomName()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedRoomDataAsync(ctx);
            var repo = new RoomRepository(ctx);

            var room = await ctx.Rooms.FirstAsync(r => r.RoomId == 1);
            room.RoomName = "Phòng đã cập nhật";
            await repo.UpdateAsync(room);

            var updated = await ctx.Rooms.FindAsync(1);
            updated!.RoomName.Should().Be("Phòng đã cập nhật");
        }

        // 🧪 Test 8: DeleteAsync — xóa phòng
        [Fact]
        public async Task DeleteAsync_Removes_Room()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedRoomDataAsync(ctx);
            var repo = new RoomRepository(ctx);

            await repo.DeleteAsync(1);

            var deleted = await ctx.Rooms.FindAsync(1);
            deleted.Should().BeNull();
            (await ctx.Rooms.CountAsync()).Should().Be(7);
        }

        // 🧪 Test 9: DeleteAsync — không throw khi xóa phòng không tồn tại
        [Fact]
        public async Task DeleteAsync_Does_Not_Throw_When_Room_Not_Exists()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedRoomDataAsync(ctx);
            var repo = new RoomRepository(ctx);

            var act = async () => await repo.DeleteAsync(9999);
            await act.Should().NotThrowAsync();

            (await ctx.Rooms.CountAsync()).Should().Be(8);
        }

        // 🧪 Test 10: GetPagedAsync — xử lý pageNumber/pageSize không hợp lệ
        [Theory]
        [InlineData(0, 10)]
        [InlineData(-1, 5)]
        [InlineData(1, 0)]
        [InlineData(1, -5)]
        public async Task GetPagedAsync_Handles_Invalid_Paging(int pageNumber, int pageSize)
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedRoomDataAsync(ctx);
            var repo = new RoomRepository(ctx);

            // Repository không validate paging, chỉ dùng trực tiếp trong LINQ
            // Với invalid paging có thể trả về kết quả không mong muốn nhưng không crash
            var (items, totalCount) = await repo.GetPagedAsync(pageNumber, pageSize);

            // Chỉ verify rằng method không throw exception
            items.Should().NotBeNull();
            totalCount.Should().BeGreaterThanOrEqualTo(0);
        }

        // 🧪 Test 11: GetAllAsync — trả về empty list khi chưa có dữ liệu
        [Fact]
        public async Task GetAllAsync_Returns_Empty_List_When_No_Data()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            var repo = new RoomRepository(ctx);

            var all = await repo.GetAllAsync();

            all.Should().BeEmpty();
        }

        // 🧪 Test 12: GetByIdAsync — Include doctors relationship
        [Fact]
        public async Task GetByIdAsync_Includes_Doctors_Relationship()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedRoomDataAsync(ctx);

            // Tạo Role trước
            var role = new Role { RoleId = 1, RoleName = "Doctor" };
            ctx.Roles.Add(role);
            await ctx.SaveChangesAsync();

            // Tạo Users với ID khác nhau để tránh conflict
            var user1 = new User { UserId = 1, Phone = "0901", PasswordHash = "x", FullName = "Dr. Test 1", RoleId = 1 };
            var user2 = new User { UserId = 2, Phone = "0902", PasswordHash = "x", FullName = "Dr. Test 2", RoleId = 1 };
            ctx.Users.AddRange(user1, user2);
            await ctx.SaveChangesAsync();

            // Tạo Doctors với Users đã tồn tại
            var doctor1 = new Doctor { DoctorId = 1, UserId = 1, Specialty = "Cardiology", ExperienceYears = 5, RoomId = 1 };
            var doctor2 = new Doctor { DoctorId = 2, UserId = 2, Specialty = "Neurology", ExperienceYears = 3, RoomId = 1 };
            ctx.Doctors.AddRange(doctor1, doctor2);

            await ctx.SaveChangesAsync();
            var repo = new RoomRepository(ctx);

            var room = await repo.GetByIdAsync(1);

            room.Should().NotBeNull();
            room!.Doctors.Should().HaveCount(2);
        }

        // 🧪 Test 13: GetPagedAsync — sort by RoomId ascending
        [Fact]
        public async Task GetPagedAsync_Sorts_By_RoomId_Ascending()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedRoomDataAsync(ctx);
            var repo = new RoomRepository(ctx);

            var (items, total) = await repo.GetPagedAsync(pageNumber: 1, pageSize: 5);

            items.Should().BeInAscendingOrder(r => r.RoomId);
            items.First().RoomId.Should().Be(1);
        }

        // 🧪 Test 14: GetPagedAsync với searchTerm có ký tự đặc biệt
        [Fact]
        public async Task GetPagedAsync_Handles_Special_Characters_In_SearchTerm()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedRoomDataAsync(ctx);
            var repo = new RoomRepository(ctx);

            var (items, totalCount) = await repo.GetPagedAsync(1, 10, "!@#$%^&*()");

            items.Should().NotBeNull();
            totalCount.Should().Be(0);
            items.Should().BeEmpty();
        }

        // 🧪 Test 15: GetPagedAsync với searchTerm rất dài
        [Fact]
        public async Task GetPagedAsync_Handles_Very_Long_SearchTerm()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedRoomDataAsync(ctx);
            var repo = new RoomRepository(ctx);

            var longSearchTerm = new string('A', 1000);
            var (items, totalCount) = await repo.GetPagedAsync(1, 10, longSearchTerm);

            items.Should().NotBeNull();
            totalCount.Should().Be(0);
            items.Should().BeEmpty();
        }

        // 🧪 Test 16: UpdateAsync với room có Doctors
        [Fact]
        public async Task UpdateAsync_Updates_Room_With_Existing_Doctors()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);

            // Tạo Users với ID khác nhau để tránh conflict
            var user1 = new User { UserId = 1, Phone = "0901", PasswordHash = "x", FullName = "Dr. Test 1", RoleId = 1 };
            var user2 = new User { UserId = 2, Phone = "0902", PasswordHash = "x", FullName = "Dr. Test 2", RoleId = 1 };
            ctx.Users.AddRange(user1, user2);
            await ctx.SaveChangesAsync();

            // Tạo Room với Doctors
            var room = new Room { RoomId = 1, RoomName = "Original Room" };
            ctx.Rooms.Add(room);

            var doctor1 = new Doctor { DoctorId = 1, UserId = 1, Specialty = "Cardiology", ExperienceYears = 5, RoomId = 1 };
            var doctor2 = new Doctor { DoctorId = 2, UserId = 2, Specialty = "Neurology", ExperienceYears = 3, RoomId = 1 };
            ctx.Doctors.AddRange(doctor1, doctor2);
            await ctx.SaveChangesAsync();

            var repo = new RoomRepository(ctx);

            // Update room name
            room.RoomName = "Updated Room Name";
            await repo.UpdateAsync(room);

            // Verify update
            var updatedRoom = await ctx.Rooms
                .Include(r => r.Doctors)
                .FirstOrDefaultAsync(r => r.RoomId == 1);

            updatedRoom.Should().NotBeNull();
            updatedRoom!.RoomName.Should().Be("Updated Room Name");
            updatedRoom.Doctors.Should().HaveCount(2);
        }

        // 🧪 Test 17: AddAsync throws exception when room name already exists
        [Fact]
        public async Task AddAsync_Throws_When_Room_Name_Already_Exists()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedRoomDataAsync(ctx);
            var repo = new RoomRepository(ctx);

            var duplicateRoom = new Room { RoomName = "Phòng khám tổng quát 101" };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.AddAsync(duplicateRoom));

            exception.Message.Should().Be("Room name 'Phòng khám tổng quát 101' already exists.");
        }

        // 🧪 Test 18: AddAsync throws exception when room name already exists (case insensitive)
        [Fact]
        public async Task AddAsync_Throws_When_Room_Name_Already_Exists_Case_Insensitive()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedRoomDataAsync(ctx);
            var repo = new RoomRepository(ctx);

            var duplicateRoom = new Room { RoomName = "PHÒNG KHÁM TỔNG QUÁT 101" };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.AddAsync(duplicateRoom));

            exception.Message.Should().Be("Room name 'PHÒNG KHÁM TỔNG QUÁT 101' already exists.");
        }

        // 🧪 Test 19: UpdateAsync throws exception when room name already exists
        [Fact]
        public async Task UpdateAsync_Throws_When_Room_Name_Already_Exists()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedRoomDataAsync(ctx);
            var repo = new RoomRepository(ctx);

            var roomToUpdate = await ctx.Rooms.FirstAsync(r => r.RoomId == 1);
            roomToUpdate.RoomName = "Phòng tim mạch 202"; // This name already exists

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.UpdateAsync(roomToUpdate));

            exception.Message.Should().Be("Room name 'Phòng tim mạch 202' already exists.");
        }

        // 🧪 Test 20: UpdateAsync allows same room to keep its name
        [Fact]
        public async Task UpdateAsync_Allows_Same_Room_To_Keep_Its_Name()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedRoomDataAsync(ctx);
            var repo = new RoomRepository(ctx);

            var roomToUpdate = await ctx.Rooms.FirstAsync(r => r.RoomId == 1);
            var originalName = roomToUpdate.RoomName;
            roomToUpdate.RoomName = originalName; // Same name

            await repo.UpdateAsync(roomToUpdate);

            var updatedRoom = await ctx.Rooms.FirstAsync(r => r.RoomId == 1);
            updatedRoom.RoomName.Should().Be(originalName);
        }

        // 🧪 Test 21: DeleteAsync throws exception when room has doctors
        [Fact]
        public async Task DeleteAsync_Throws_When_Room_Has_Doctors()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);

            // Tạo Users với ID khác nhau để tránh conflict
            var user1 = new User { UserId = 1, Phone = "0901", PasswordHash = "x", FullName = "Dr. Test 1", RoleId = 1 };
            var user2 = new User { UserId = 2, Phone = "0902", PasswordHash = "x", FullName = "Dr. Test 2", RoleId = 1 };
            ctx.Users.AddRange(user1, user2);
            await ctx.SaveChangesAsync();

            // Tạo Room với Doctors
            var room = new Room { RoomId = 1, RoomName = "Room with Doctors" };
            ctx.Rooms.Add(room);

            var doctor1 = new Doctor { DoctorId = 1, UserId = 1, Specialty = "Cardiology", ExperienceYears = 5, RoomId = 1 };
            var doctor2 = new Doctor { DoctorId = 2, UserId = 2, Specialty = "Neurology", ExperienceYears = 3, RoomId = 1 };
            ctx.Doctors.AddRange(doctor1, doctor2);
            await ctx.SaveChangesAsync();

            var repo = new RoomRepository(ctx);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.DeleteAsync(1));

            exception.Message.Should().Be("Cannot delete room 'Room with Doctors' because it has 2 doctor(s) assigned.");
        }

        // 🧪 Test 22: DeleteAsync succeeds when room has no doctors
        [Fact]
        public async Task DeleteAsync_Succeeds_When_Room_Has_No_Doctors()
        {
            var db = Guid.NewGuid().ToString();
            await using var ctx = NewCtx(db);
            await SeedRoomDataAsync(ctx);
            var repo = new RoomRepository(ctx);

            await repo.DeleteAsync(1);

            var deletedRoom = await ctx.Rooms.FirstOrDefaultAsync(r => r.RoomId == 1);
            deletedRoom.Should().BeNull();
        }
    }
}
