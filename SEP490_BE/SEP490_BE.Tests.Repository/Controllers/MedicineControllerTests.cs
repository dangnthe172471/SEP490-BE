using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.MedicineDTO;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers
{
    public class MedicineControllerTests
    {
        private static MedicineController CreateController(Mock<IMedicineService> svcMock, int userId = 1)
        {
            var controller = new MedicineController(svcMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "Pharmacy Provider")
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            return controller;
        }

        // ===============================================================
        //                    CREATE TEST CASES (5)
        // ===============================================================

        [Fact]
        //Tạo thuốc hợp lệ (đầy đủ các trường, status = Providing)
        public async Task Create_Should_Return200_When_Valid()
        {
            var svc = new Mock<IMedicineService>();
            svc.Setup(s => s.GetProviderIdByUserIdAsync(1, It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);
            svc.Setup(s => s.CreateMedicineAsync(It.IsAny<CreateMedicineDto>(), 1, It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

            var controller = CreateController(svc);
            var dto = new CreateMedicineDto
            {
                MedicineName = "Vitamin C 500mg",
                CommonSideEffects = "buồn nôn nhẹ",
                Status = "Providing"
            };

            var result = await controller.Create(dto, CancellationToken.None);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        [Fact]
        //Tạo thuốc hợp lệ (status = null tự mặc định “Providing”)
        public async Task Create_Should_Return200_When_StatusNull_DefaultProviding()
        {
            var svc = new Mock<IMedicineService>();
            svc.Setup(s => s.GetProviderIdByUserIdAsync(1, It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);
            svc.Setup(s => s.CreateMedicineAsync(It.IsAny<CreateMedicineDto>(), 1, It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

            var controller = CreateController(svc);
            var dto = new CreateMedicineDto
            {
                MedicineName = "Zinc 50mg",
                CommonSideEffects = "đau bụng thoáng qua",
                Status = null
            };

            var result = await controller.Create(dto, CancellationToken.None);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        [Fact]
        //Thiếu tên thuốc 400 Bad Request
        public async Task Create_Should_Return400_When_NameMissing()
        {
            var svc = new Mock<IMedicineService>();
            svc.Setup(s => s.GetProviderIdByUserIdAsync(1, It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);
            svc.Setup(s => s.CreateMedicineAsync(It.IsAny<CreateMedicineDto>(), 1, It.IsAny<CancellationToken>()))
               .ThrowsAsync(new ArgumentException("Medicine name is required."));

            var controller = CreateController(svc);
            var dto = new CreateMedicineDto
            {
                MedicineName = "",
                CommonSideEffects = "chóng mặt",
                Status = "Providing"
            };

            var result = await controller.Create(dto, CancellationToken.None);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, bad.StatusCode);
        }

        [Fact]
        //Status không hợp lệ 400 Bad Request
        public async Task Create_Should_Return400_When_StatusInvalid()
        {
            var svc = new Mock<IMedicineService>();
            svc.Setup(s => s.GetProviderIdByUserIdAsync(1, It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);
            svc.Setup(s => s.CreateMedicineAsync(It.IsAny<CreateMedicineDto>(), 1, It.IsAny<CancellationToken>()))
               .ThrowsAsync(new ArgumentException("Invalid status. Allowed: Providing | Stopped."));

            var controller = CreateController(svc);
            var dto = new CreateMedicineDto
            {
                MedicineName = "Cefuroxime",
                CommonSideEffects = "tiêu chảy",
                Status = "Active"
            };

            var result = await controller.Create(dto, CancellationToken.None);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, bad.StatusCode);
        }

        [Fact]
        //Tên thuốc trùng với thuốc khác cùng Provider 409 Conflict
        public async Task Create_Should_Return409_When_DuplicatedName()
        {
            var svc = new Mock<IMedicineService>();
            svc.Setup(s => s.GetProviderIdByUserIdAsync(1, It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);
            svc.Setup(s => s.CreateMedicineAsync(It.IsAny<CreateMedicineDto>(), 1, It.IsAny<CancellationToken>()))
               .ThrowsAsync(new InvalidOperationException("Medicine 'Amlodipine 5mg' already exists for this provider."));

            var controller = CreateController(svc);
            var dto = new CreateMedicineDto
            {
                MedicineName = "Amlodipine 5mg",
                CommonSideEffects = "đau đầu",
                Status = "Providing"
            };

            var result = await controller.Create(dto, CancellationToken.None);
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflict.StatusCode);
        }

        // ===============================================================
        //                    UPDATE TEST CASES (7)
        // ===============================================================

        [Fact]
        // Cập nhật hợp lệ (đổi trạng thái Providing Stopped)
        public async Task Update_Should_Return200_When_Valid_ChangeStatus()
        {
            var svc = new Mock<IMedicineService>();
            svc.Setup(s => s.UpdateMineAsync(1, 1, It.IsAny<UpdateMedicineDto>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

            var controller = CreateController(svc);
            var dto = new UpdateMedicineDto
            {
                MedicineName = "Paracetamol 500mg",
                CommonSideEffects = "buồn ngủ, mệt nhẹ",
                Status = "Stopped"
            };

            var result = await controller.Update(1, dto, CancellationToken.None);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        [Fact]
        //Cập nhật hợp lệ (đổi tên thuốc)
        public async Task Update_Should_Return200_When_Valid_Rename()
        {
            var svc = new Mock<IMedicineService>();
            svc.Setup(s => s.UpdateMineAsync(1, 3, It.IsAny<UpdateMedicineDto>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

            var controller = CreateController(svc);
            var dto = new UpdateMedicineDto
            {
                MedicineName = "Amoxicillin 500mg",
                CommonSideEffects = "khó tiêu",
                Status = "Providing"
            };

            var result = await controller.Update(3, dto, CancellationToken.None);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        [Fact]
        //Đổi tên trùng thuốc khác cùng Provider 409 Conflict
        public async Task Update_Should_Return409_When_DuplicateName()
        {
            var svc = new Mock<IMedicineService>();
            svc.Setup(s => s.UpdateMineAsync(1, 4, It.IsAny<UpdateMedicineDto>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new InvalidOperationException("Medicine 'Amlodipine 5mg' already exists for this provider."));

            var controller = CreateController(svc);
            var dto = new UpdateMedicineDto
            {
                MedicineName = "Amlodipine 5mg",
                CommonSideEffects = "đầy bụng",
                Status = "Providing"
            };

            var result = await controller.Update(4, dto, CancellationToken.None);
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflict.StatusCode);
        }

        [Fact]
        //Thuốc không tồn tại 404 Not Found
        public async Task Update_Should_Return404_When_NotFound()
        {
            var svc = new Mock<IMedicineService>();
            svc.Setup(s => s.UpdateMineAsync(1, 999, It.IsAny<UpdateMedicineDto>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new KeyNotFoundException("Medicine with ID 999 not found."));

            var controller = CreateController(svc);
            var dto = new UpdateMedicineDto
            {
                MedicineName = "NonExistingDrug",
                CommonSideEffects = "khó thở",
                Status = "Providing"
            };

            var result = await controller.Update(999, dto, CancellationToken.None);
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFound.StatusCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        //Tên thuốc null hoặc chỉ toàn khoảng trắng 400 Bad Request
        public async Task Update_Should_Return400_When_NameNullOrWhitespace(string? name)
        {
            var svc = new Mock<IMedicineService>();
            svc.Setup(s => s.UpdateMineAsync(1, 1, It.IsAny<UpdateMedicineDto>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new ArgumentException("Medicine name cannot be empty or whitespace."));

            var controller = CreateController(svc);
            var dto = new UpdateMedicineDto
            {
                MedicineName = name,
                CommonSideEffects = "đau bụng",
                Status = "Providing"
            };

            var result = await controller.Update(1, dto, CancellationToken.None);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, bad.StatusCode);
        }

        [Fact]
        //Status sai (Available) 400 Bad Request
        public async Task Update_Should_Return400_When_StatusInvalid()
        {
            var svc = new Mock<IMedicineService>();
            svc.Setup(s => s.UpdateMineAsync(1, 1, It.IsAny<UpdateMedicineDto>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new ArgumentException("Invalid status. Allowed: Providing | Stopped."));

            var controller = CreateController(svc);
            var dto = new UpdateMedicineDto
            {
                MedicineName = "NewDrug",
                CommonSideEffects = "buồn nôn",
                Status = "Available"
            };

            var result = await controller.Update(1, dto, CancellationToken.None);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, bad.StatusCode);
        }

        [Fact]
        //Tên trùng nhưng khác hoa/thường (AMO vs Amo) 409 Conflict
        public async Task Update_Should_Return409_When_CaseInsensitiveDuplicate()
        {
            var svc = new Mock<IMedicineService>();
            svc.Setup(s => s.UpdateMineAsync(1, 5, It.IsAny<UpdateMedicineDto>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new InvalidOperationException("Medicine 'AMO' already exists for this provider."));

            var controller = CreateController(svc);
            var dto = new UpdateMedicineDto
            {
                MedicineName = "AMO",
                CommonSideEffects = "đau đầu",
                Status = "Providing"
            };

            var result = await controller.Update(5, dto, CancellationToken.None);
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflict.StatusCode);
        }
    }
}
