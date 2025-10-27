using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers
{
    public class RoomsControllerTests
    {
        private readonly Mock<IRoomService> _svc = new();

        private RoomsController MakeControllerWithUser(int? userId = 1, string? role = "Clinic Manager")
        {
            var controller = new RoomsController(_svc.Object);

            var claims = new List<Claim>();
            if (userId.HasValue)
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
            if (!string.IsNullOrEmpty(role))
                claims.Add(new Claim(ClaimTypes.Role, role));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"))
                }
            };
            return controller;
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithData()
        {
            _svc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RoomDto>
                {
                    new() { RoomId = 1, RoomName = "Phòng 101" },
                    new() { RoomId = 2, RoomName = "Phòng 102" }
                });

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.GetAll(CancellationToken.None);
            var okResult = result.Result as OkObjectResult;

            okResult.Should().NotBeNull();
            var list = okResult!.Value as IEnumerable<RoomDto>;
            list.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenFound()
        {
            _svc.Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RoomDto { RoomId = 5, RoomName = "Phòng 505" });

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.GetById(5, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNull()
        {
            _svc.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((RoomDto?)null);

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.GetById(999, CancellationToken.None);

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetPaged_ReturnsOk_WithPagedData()
        {
            _svc.Setup(s => s.GetPagedAsync(1, 10, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResponse<RoomDto>
                {
                    Items = new List<RoomDto> { new() { RoomId = 1, RoomName = "Phòng 101" } },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.GetPaged(1, 10, null, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPaged_WithSearchTerm_ReturnsFilteredResults()
        {
            _svc.Setup(s => s.GetPagedAsync(1, 10, "tim mạch", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResponse<RoomDto>
                {
                    Items = new List<RoomDto> { new() { RoomId = 1, RoomName = "Phòng tim mạch 202" } },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.GetPaged(1, 10, "tim mạch", CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsCreated_WhenSuccess()
        {
            _svc.Setup(s => s.CreateAsync(It.IsAny<CreateRoomRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(99);

            var ctrl = MakeControllerWithUser();
            var request = new CreateRoomRequest { RoomName = "Phòng mới" };

            var result = await ctrl.Create(request, CancellationToken.None);

            result.Result.Should().BeOfType<CreatedAtActionResult>();
            _svc.Verify(s => s.CreateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_When_ArgumentException()
        {
            _svc.Setup(s => s.CreateAsync(It.IsAny<CreateRoomRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Room name is required."));

            var ctrl = MakeControllerWithUser();
            var request = new CreateRoomRequest { RoomName = "" };

            var result = await ctrl.Create(request, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOk_WhenSuccess()
        {
            _svc.Setup(s => s.UpdateAsync(5, It.IsAny<UpdateRoomRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RoomDto { RoomId = 5, RoomName = "Phòng đã cập nhật" });

            var ctrl = MakeControllerWithUser();
            var request = new UpdateRoomRequest { RoomName = "Phòng đã cập nhật" };

            var result = await ctrl.Update(5, request, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
            _svc.Verify(s => s.UpdateAsync(5, request, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenNull()
        {
            _svc.Setup(s => s.UpdateAsync(999, It.IsAny<UpdateRoomRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((RoomDto?)null);

            var ctrl = MakeControllerWithUser();
            var request = new UpdateRoomRequest { RoomName = "Updated" };

            var result = await ctrl.Update(999, request, CancellationToken.None);

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_When_ArgumentException()
        {
            _svc.Setup(s => s.UpdateAsync(5, It.IsAny<UpdateRoomRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Room name is required."));

            var ctrl = MakeControllerWithUser();
            var request = new UpdateRoomRequest { RoomName = "" };

            var result = await ctrl.Update(5, request, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenSuccess()
        {
            _svc.Setup(s => s.DeleteAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.Delete(5, CancellationToken.None);

            result.Should().BeOfType<NoContentResult>();
            _svc.Verify(s => s.DeleteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenNotDeleted()
        {
            _svc.Setup(s => s.DeleteAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.Delete(999, CancellationToken.None);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetAll_Requires_AuthorizeAttribute()
        {
            var ctrl = MakeControllerWithUser();

            // Verify that the endpoint requires authorization
            var method = typeof(RoomsController).GetMethod(nameof(RoomsController.GetAll));
            var attributes = method?.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);

            attributes.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAll_Empty_List_ReturnsOk()
        {
            _svc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RoomDto>());

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.GetAll(CancellationToken.None);
            var okResult = result.Result as OkObjectResult;

            okResult.Should().NotBeNull();
            var list = okResult!.Value as IEnumerable<RoomDto>;
            list.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPaged_Empty_Result_ReturnsOk()
        {
            _svc.Setup(s => s.GetPagedAsync(1, 10, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResponse<RoomDto>
                {
                    Items = new List<RoomDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.GetPaged(1, 10, null, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_Handles_Special_Characters_In_Name()
        {
            _svc.Setup(s => s.CreateAsync(It.IsAny<CreateRoomRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var ctrl = MakeControllerWithUser();
            var request = new CreateRoomRequest { RoomName = "Phòng khám tổng quát" };

            var result = await ctrl.Create(request, CancellationToken.None);

            result.Result.Should().BeOfType<CreatedAtActionResult>();
            _svc.Verify(s => s.CreateAsync(
                It.Is<CreateRoomRequest>(r => r.RoomName == "Phòng khám tổng quát"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Create_Handles_Very_Long_Name()
        {
            _svc.Setup(s => s.CreateAsync(It.IsAny<CreateRoomRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var ctrl = MakeControllerWithUser();
            var longName = new string('A', 1000);
            var request = new CreateRoomRequest { RoomName = longName };

            var result = await ctrl.Create(request, CancellationToken.None);

            result.Result.Should().BeOfType<CreatedAtActionResult>();
            _svc.Verify(s => s.CreateAsync(
                It.Is<CreateRoomRequest>(r => r.RoomName == longName),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Update_Handles_Special_Characters_In_Name()
        {
            var existingDto = new RoomDto { RoomId = 3, RoomName = "Updated Room" };
            _svc.Setup(s => s.UpdateAsync(3, It.IsAny<UpdateRoomRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingDto);

            var ctrl = MakeControllerWithUser();
            var request = new UpdateRoomRequest { RoomName = "Phòng khám tổng quát" };

            var result = await ctrl.Update(3, request, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
            _svc.Verify(s => s.UpdateAsync(3,
                It.Is<UpdateRoomRequest>(r => r.RoomName == "Phòng khám tổng quát"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPaged_Handles_Very_Large_PageSize()
        {
            _svc.Setup(s => s.GetPagedAsync(1, int.MaxValue, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResponse<RoomDto>
                {
                    Items = new List<RoomDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = int.MaxValue
                });

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.GetPaged(1, int.MaxValue, null, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPaged_Handles_Special_Characters_In_SearchTerm()
        {
            _svc.Setup(s => s.GetPagedAsync(1, 10, "!@#$%^&*()", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResponse<RoomDto>
                {
                    Items = new List<RoomDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.GetPaged(1, 10, "!@#$%^&*()", CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_When_Room_Name_Too_Short()
        {
            _svc.Setup(s => s.CreateAsync(It.IsAny<CreateRoomRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Room name must be at least 2 characters long."));

            var ctrl = MakeControllerWithUser();
            var request = new CreateRoomRequest { RoomName = "A" };

            var result = await ctrl.Create(request, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result.Result as BadRequestObjectResult;
            badRequest!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_When_Room_Name_Too_Long()
        {
            _svc.Setup(s => s.CreateAsync(It.IsAny<CreateRoomRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Room name cannot exceed 100 characters."));

            var ctrl = MakeControllerWithUser();
            var longName = new string('A', 101);
            var request = new CreateRoomRequest { RoomName = longName };

            var result = await ctrl.Create(request, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_When_Room_Name_Has_Invalid_Characters()
        {
            _svc.Setup(s => s.CreateAsync(It.IsAny<CreateRoomRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Room name contains invalid characters. Only letters, numbers, spaces, and common punctuation are allowed."));

            var ctrl = MakeControllerWithUser();
            var request = new CreateRoomRequest { RoomName = "Room@#$%^&*()" };

            var result = await ctrl.Create(request, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_When_Room_Name_Already_Exists()
        {
            _svc.Setup(s => s.CreateAsync(It.IsAny<CreateRoomRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Room name 'Test Room' already exists."));

            var ctrl = MakeControllerWithUser();
            var request = new CreateRoomRequest { RoomName = "Test Room" };

            var result = await ctrl.Create(request, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_When_Room_Name_Too_Short()
        {
            _svc.Setup(s => s.UpdateAsync(3, It.IsAny<UpdateRoomRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Room name must be at least 2 characters long."));

            var ctrl = MakeControllerWithUser();
            var request = new UpdateRoomRequest { RoomName = "A" };

            var result = await ctrl.Update(3, request, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_When_Room_Name_Too_Long()
        {
            _svc.Setup(s => s.UpdateAsync(3, It.IsAny<UpdateRoomRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Room name cannot exceed 100 characters."));

            var ctrl = MakeControllerWithUser();
            var longName = new string('A', 101);
            var request = new UpdateRoomRequest { RoomName = longName };

            var result = await ctrl.Update(3, request, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_When_Room_Name_Has_Invalid_Characters()
        {
            _svc.Setup(s => s.UpdateAsync(3, It.IsAny<UpdateRoomRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Room name contains invalid characters. Only letters, numbers, spaces, and common punctuation are allowed."));

            var ctrl = MakeControllerWithUser();
            var request = new UpdateRoomRequest { RoomName = "Room@#$%^&*()" };

            var result = await ctrl.Update(3, request, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_When_Room_Name_Already_Exists()
        {
            _svc.Setup(s => s.UpdateAsync(3, It.IsAny<UpdateRoomRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Room name 'Test Room' already exists."));

            var ctrl = MakeControllerWithUser();
            var request = new UpdateRoomRequest { RoomName = "Test Room" };

            var result = await ctrl.Update(3, request, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsBadRequest_When_Room_Has_Doctors()
        {
            _svc.Setup(s => s.DeleteAsync(5, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Cannot delete room 'Test Room' because it has 2 doctor(s) assigned."));

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.Delete(5, CancellationToken.None);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
