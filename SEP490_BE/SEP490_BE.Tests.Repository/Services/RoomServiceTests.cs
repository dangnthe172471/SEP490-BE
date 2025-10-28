using FluentAssertions;
using Moq;
using SEP490_BE.BLL.Services;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.Tests.Services
{
    public class RoomServiceTests
    {
        private readonly Mock<IRoomRepository> _repo = new();

        // 🔧 Helper: Tạo entity Room để test mapping
        private static Room MakeRoom(int id, string name)
            => new Room
            {
                RoomId = id,
                RoomName = name
            };

        // ✅ Test: GetAllAsync map entity → RoomDto
        [Fact]
        public async Task GetAllAsync_Maps_Entities_To_Dtos()
        {
            var data = new List<Room>
            {
                MakeRoom(1, "Phòng 101"),
                MakeRoom(2, "Phòng 102"),
                MakeRoom(3, "Phòng 103")
            };

            _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(data);

            var svc = new RoomService(_repo.Object);

            var list = await svc.GetAllAsync();

            list.Should().HaveCount(3);
            list.ElementAt(0).RoomId.Should().Be(1);
            list.ElementAt(0).RoomName.Should().Be("Phòng 101");
            list.ElementAt(1).RoomId.Should().Be(2);
        }

        // ✅ Test: GetByIdAsync trả về null khi không tìm thấy
        [Fact]
        public async Task GetByIdAsync_Returns_Null_When_NotFound()
        {
            _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Room?)null);

            var svc = new RoomService(_repo.Object);

            var dto = await svc.GetByIdAsync(999);

            dto.Should().BeNull();
        }

        // ✅ Test: GetByIdAsync map đúng khi tìm thấy
        [Fact]
        public async Task GetByIdAsync_Maps_When_Found()
        {
            var room = MakeRoom(5, "Phòng 505");
            _repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(room);

            var svc = new RoomService(_repo.Object);

            var dto = await svc.GetByIdAsync(5);

            dto.Should().NotBeNull();
            dto!.RoomId.Should().Be(5);
            dto.RoomName.Should().Be("Phòng 505");
        }

        // ✅ Test: GetPagedAsync map entities → DTOs với pagination
        [Fact]
        public async Task GetPagedAsync_Maps_With_Pagination()
        {
            var rooms = new List<Room>
            {
                MakeRoom(1, "Phòng 101"),
                MakeRoom(2, "Phòng 102"),
                MakeRoom(3, "Phòng 103")
            };

            _repo.Setup(r => r.GetPagedAsync(1, 2, null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((rooms, 3));

            var svc = new RoomService(_repo.Object);

            var result = await svc.GetPagedAsync(1, 2);

            result.TotalCount.Should().Be(3);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(2);
            result.Items.Should().HaveCount(3);
        }

        // ✅ Test: GetPagedAsync normalize pageNumber/pageSize
        [Fact]
        public async Task GetPagedAsync_Normalizes_PageNumber_And_PageSize()
        {
            _repo.Setup(r => r.GetPagedAsync(1, 10, null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((new List<Room>(), 0));

            var svc = new RoomService(_repo.Object);

            var result = await svc.GetPagedAsync(-5, 0);

            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);

            _repo.Verify(r => r.GetPagedAsync(1, 10, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        // ✅ Test: GetPagedAsync truyền searchTerm xuống repository
        [Fact]
        public async Task GetPagedAsync_Forwards_SearchTerm()
        {
            _repo.Setup(r => r.GetPagedAsync(1, 10, "tim mạch", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((new List<Room>(), 0));

            var svc = new RoomService(_repo.Object);

            await svc.GetPagedAsync(1, 10, "tim mạch");

            _repo.Verify(r => r.GetPagedAsync(1, 10, "tim mạch", It.IsAny<CancellationToken>()), Times.Once);
        }

        // ✅ Test: CreateAsync throw khi RoomName null/whitespace
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateAsync_Throws_When_Name_Is_Null_Or_Whitespace(string? badName)
        {
            var svc = new RoomService(_repo.Object);
            var request = new CreateRoomRequest { RoomName = badName! };

            var act = async () => await svc.CreateAsync(request);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Room name is required*");
            _repo.Verify(r => r.AddAsync(It.IsAny<Room>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ✅ Test: CreateAsync trim name và tạo phòng
        [Fact]
        public async Task CreateAsync_Trims_Name_And_Creates_Room()
        {
            var request = new CreateRoomRequest { RoomName = "  Phòng mới  " };

            var svc = new RoomService(_repo.Object);

            await svc.CreateAsync(request);

            _repo.Verify(r => r.AddAsync(
                It.Is<Room>(rm =>
                    rm.RoomName == "Phòng mới"
                ),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // ✅ Test: UpdateAsync trả null khi phòng không tồn tại
        [Fact]
        public async Task UpdateAsync_Returns_Null_When_Room_Not_Exists()
        {
            _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Room?)null);

            var svc = new RoomService(_repo.Object);
            var request = new UpdateRoomRequest { RoomName = "Updated" };

            var result = await svc.UpdateAsync(999, request);

            result.Should().BeNull();
            _repo.Verify(r => r.UpdateAsync(It.IsAny<Room>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ✅ Test: UpdateAsync throw khi tên rỗng
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateAsync_Throws_When_Name_Is_Invalid(string? badName)
        {
            var existing = MakeRoom(1, "Phòng cũ");
            _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

            var svc = new RoomService(_repo.Object);
            var request = new UpdateRoomRequest { RoomName = badName! };

            var act = async () => await svc.UpdateAsync(1, request);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Room name is required*");
        }

        // ✅ Test: UpdateAsync cập nhật tên (trim) và trả DTO
        [Fact]
        public async Task UpdateAsync_Updates_Room_Name_And_Returns_Dto()
        {
            var existing = MakeRoom(3, "Phòng cũ");
            _repo.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

            var svc = new RoomService(_repo.Object);
            var request = new UpdateRoomRequest { RoomName = "  Phòng mới  " };

            var result = await svc.UpdateAsync(3, request);

            result.Should().NotBeNull();
            result!.RoomId.Should().Be(3);
            result.RoomName.Should().Be("Phòng mới");

            _repo.Verify(r => r.UpdateAsync(
                It.Is<Room>(rm =>
                    rm.RoomName == "Phòng mới"
                ),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // ✅ Test: DeleteAsync trả false khi phòng không tồn tại
        [Fact]
        public async Task DeleteAsync_Returns_False_When_Room_Not_Exists()
        {
            _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Room?)null);

            var svc = new RoomService(_repo.Object);

            var result = await svc.DeleteAsync(999);

            result.Should().BeFalse();
            _repo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ✅ Test: DeleteAsync xóa và trả true khi thành công
        [Fact]
        public async Task DeleteAsync_Deletes_And_Returns_True_When_Success()
        {
            var existing = MakeRoom(5, "Phòng 505");
            _repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

            var svc = new RoomService(_repo.Object);

            var result = await svc.DeleteAsync(5);

            result.Should().BeTrue();
            _repo.Verify(r => r.DeleteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
        }

        // ✅ Test: GetPagedAsync return empty list when no data
        [Fact]
        public async Task GetPagedAsync_Returns_Empty_List_When_No_Data()
        {
            _repo.Setup(r => r.GetPagedAsync(1, 10, null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((new List<Room>(), 0));

            var svc = new RoomService(_repo.Object);

            var result = await svc.GetPagedAsync(1, 10);

            result.TotalCount.Should().Be(0);
            result.Items.Should().BeEmpty();
        }

        // ✅ Test: GetAllAsync return empty when no rooms
        [Fact]
        public async Task GetAllAsync_Returns_Empty_When_No_Rooms()
        {
            _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Room>());

            var svc = new RoomService(_repo.Object);

            var list = await svc.GetAllAsync();

            list.Should().BeEmpty();
        }

        // ✅ Test: CreateAsync với tên có ký tự đặc biệt
        [Fact]
        public async Task CreateAsync_Handles_Special_Characters_In_Name()
        {
            // Mock AddAsync để set RoomId
            _repo.Setup(r => r.AddAsync(It.IsAny<Room>(), It.IsAny<CancellationToken>()))
                 .Callback<Room, CancellationToken>((room, ct) => room.RoomId = 1);

            var svc = new RoomService(_repo.Object);
            var request = new CreateRoomRequest { RoomName = "Phòng khám tổng quát" };

            var result = await svc.CreateAsync(request);

            result.Should().BeGreaterThan(0);
            _repo.Verify(r => r.AddAsync(
                It.Is<Room>(rm => rm.RoomName == "Phòng khám tổng quát"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // ✅ Test: CreateAsync với tên rất dài
        [Fact]
        public async Task CreateAsync_Handles_Very_Long_Name()
        {
            // Mock AddAsync để set RoomId
            _repo.Setup(r => r.AddAsync(It.IsAny<Room>(), It.IsAny<CancellationToken>()))
                 .Callback<Room, CancellationToken>((room, ct) => room.RoomId = 1);

            var svc = new RoomService(_repo.Object);
            var longName = new string('A', 100);
            var request = new CreateRoomRequest { RoomName = longName };

            var result = await svc.CreateAsync(request);

            result.Should().BeGreaterThan(0);
            _repo.Verify(r => r.AddAsync(
                It.Is<Room>(rm => rm.RoomName == longName),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // ✅ Test: GetPagedAsync với pageSize rất lớn
        [Fact]
        public async Task GetPagedAsync_Handles_Very_Large_PageSize()
        {
            var rooms = new List<Room>
            {
                MakeRoom(1, "Room 1"),
                MakeRoom(2, "Room 2")
            };
            _repo.Setup(r => r.GetPagedAsync(1, int.MaxValue, null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((rooms, 2));

            var svc = new RoomService(_repo.Object);

            var result = await svc.GetPagedAsync(1, int.MaxValue);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(int.MaxValue);
        }

        // ✅ Test: UpdateAsync với tên có ký tự đặc biệt
        [Fact]
        public async Task UpdateAsync_Handles_Special_Characters_In_Name()
        {
            var existing = MakeRoom(3, "Phòng cũ");
            _repo.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

            var svc = new RoomService(_repo.Object);
            var request = new UpdateRoomRequest { RoomName = "Phòng khám tổng quát" };

            var result = await svc.UpdateAsync(3, request);

            result.Should().NotBeNull();
            result!.RoomId.Should().Be(3);
            result.RoomName.Should().Be("Phòng khám tổng quát");

            _repo.Verify(r => r.UpdateAsync(
                It.Is<Room>(rm => rm.RoomName == "Phòng khám tổng quát"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // ✅ Test: CreateAsync throws when room name is too short
        [Fact]
        public async Task CreateAsync_Throws_When_Room_Name_Too_Short()
        {
            var svc = new RoomService(_repo.Object);
            var request = new CreateRoomRequest { RoomName = "A" };

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => svc.CreateAsync(request));

            exception.Message.Should().Be("Room name must be at least 2 characters long.");
        }

        // ✅ Test: CreateAsync throws when room name is too long
        [Fact]
        public async Task CreateAsync_Throws_When_Room_Name_Too_Long()
        {
            var svc = new RoomService(_repo.Object);
            var longName = new string('A', 101);
            var request = new CreateRoomRequest { RoomName = longName };

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => svc.CreateAsync(request));

            exception.Message.Should().Be("Room name cannot exceed 100 characters.");
        }

        // ✅ Test: CreateAsync throws when room name has invalid characters
        [Fact]
        public async Task CreateAsync_Throws_When_Room_Name_Has_Invalid_Characters()
        {
            var svc = new RoomService(_repo.Object);
            var request = new CreateRoomRequest { RoomName = "Room<>{}[]|\\" };

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => svc.CreateAsync(request));

            exception.Message.Should().Be("Room name contains invalid characters. Only letters, numbers, spaces, and common punctuation are allowed.");
        }

        // ✅ Test: CreateAsync throws when room name already exists
        [Fact]
        public async Task CreateAsync_Throws_When_Room_Name_Already_Exists()
        {
            _repo.Setup(r => r.AddAsync(It.IsAny<Room>(), It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new InvalidOperationException("Room name 'Test Room' already exists."));

            var svc = new RoomService(_repo.Object);
            var request = new CreateRoomRequest { RoomName = "Test Room" };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.CreateAsync(request));

            exception.Message.Should().Be("Room name 'Test Room' already exists.");
        }

        // ✅ Test: UpdateAsync throws when room name is too short
        [Fact]
        public async Task UpdateAsync_Throws_When_Room_Name_Too_Short()
        {
            var existing = MakeRoom(3, "Phòng cũ");
            _repo.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

            var svc = new RoomService(_repo.Object);
            var request = new UpdateRoomRequest { RoomName = "A" };

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => svc.UpdateAsync(3, request));

            exception.Message.Should().Be("Room name must be at least 2 characters long.");
        }

        // ✅ Test: UpdateAsync throws when room name is too long
        [Fact]
        public async Task UpdateAsync_Throws_When_Room_Name_Too_Long()
        {
            var existing = MakeRoom(3, "Phòng cũ");
            _repo.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

            var svc = new RoomService(_repo.Object);
            var longName = new string('A', 101);
            var request = new UpdateRoomRequest { RoomName = longName };

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => svc.UpdateAsync(3, request));

            exception.Message.Should().Be("Room name cannot exceed 100 characters.");
        }

        // ✅ Test: UpdateAsync throws when room name has invalid characters
        [Fact]
        public async Task UpdateAsync_Throws_When_Room_Name_Has_Invalid_Characters()
        {
            var existing = MakeRoom(3, "Phòng cũ");
            _repo.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

            var svc = new RoomService(_repo.Object);
            var request = new UpdateRoomRequest { RoomName = "Room<>{}[]|\\" };

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => svc.UpdateAsync(3, request));

            exception.Message.Should().Be("Room name contains invalid characters. Only letters, numbers, spaces, and common punctuation are allowed.");
        }

        // ✅ Test: UpdateAsync throws when room name already exists
        [Fact]
        public async Task UpdateAsync_Throws_When_Room_Name_Already_Exists()
        {
            var existing = MakeRoom(3, "Phòng cũ");
            _repo.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);
            _repo.Setup(r => r.UpdateAsync(It.IsAny<Room>(), It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new InvalidOperationException("Room name 'Test Room' already exists."));

            var svc = new RoomService(_repo.Object);
            var request = new UpdateRoomRequest { RoomName = "Test Room" };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.UpdateAsync(3, request));

            exception.Message.Should().Be("Room name 'Test Room' already exists.");
        }

        // ✅ Test: DeleteAsync throws when room has doctors
        [Fact]
        public async Task DeleteAsync_Throws_When_Room_Has_Doctors()
        {
            _repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new InvalidOperationException("Cannot delete room 'Test Room' because it has 2 doctor(s) assigned."));

            var svc = new RoomService(_repo.Object);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.DeleteAsync(5));

            exception.Message.Should().Be("Cannot delete room 'Test Room' because it has 2 doctor(s) assigned.");
        }
    }
}
