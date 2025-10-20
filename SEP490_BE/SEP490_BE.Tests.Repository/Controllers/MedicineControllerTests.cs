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
        private readonly Mock<IMedicineService> _svc = new();

        private MedicineController MakeControllerWithUser(int? userId = 1, string? role = "Pharmacy Provider")
        {
            var controller = new MedicineController(_svc.Object);

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
                .ReturnsAsync(new List<ReadMedicineDto> { new() { MedicineId = 1, MedicineName = "A" } });

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.GetAll(CancellationToken.None) as OkObjectResult;

            result.Should().NotBeNull();
            var list = result!.Value as List<ReadMedicineDto>;
            list.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenFound()
        {
            _svc.Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReadMedicineDto { MedicineId = 5 });

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.GetById(5, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNull()
        {
            _svc.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReadMedicineDto?)null);

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.GetById(999, CancellationToken.None) as NotFoundObjectResult;

            result.Should().NotBeNull();
            result!.Value.Should().BeAssignableTo<object>();
        }

        [Fact]
        public async Task GetByProviderId_ReturnsOk_WithList()
        {
            _svc.Setup(s => s.GetByProviderIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ReadMedicineDto> { new() { MedicineId = 1 } });

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.GetByProviderId(10, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsUnauthorized_When_No_UserId()
        {
            var ctrl = MakeControllerWithUser(userId: null);
            var dto = new CreateMedicineDto { MedicineName = "A" };

            var result = await ctrl.Create(dto, CancellationToken.None);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsConflict_When_No_ProviderId()
        {
            _svc.Setup(s => s.GetProviderIdByUserIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((int?)null);

            var ctrl = MakeControllerWithUser();
            var dto = new CreateMedicineDto { MedicineName = "A" };

            var result = await ctrl.Create(dto, CancellationToken.None);

            result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsConflict_When_Service_Throws_InvalidOperation()
        {
            _svc.Setup(s => s.GetProviderIdByUserIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(5);
            _svc.Setup(s => s.CreateAsync(It.IsAny<CreateMedicineDto>(), 5, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Duplicated name"));

            var ctrl = MakeControllerWithUser();
            var dto = new CreateMedicineDto { MedicineName = "Dup" };

            var result = await ctrl.Create(dto, CancellationToken.None);

            result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOk_When_Success()
        {
            _svc.Setup(s => s.GetProviderIdByUserIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(10);

            var ctrl = MakeControllerWithUser();
            var dto = new CreateMedicineDto { MedicineName = "New" };

            var result = await ctrl.Create(dto, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
            _svc.Verify(s => s.CreateAsync(dto, 10, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_When_KeyNotFound()
        {
            _svc.Setup(s => s.UpdateAsync(99, It.IsAny<UpdateMedicineDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("not found"));

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.Update(99, new UpdateMedicineDto(), CancellationToken.None);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_When_InvalidOperation()
        {
            _svc.Setup(s => s.UpdateAsync(5, It.IsAny<UpdateMedicineDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("invalid"));

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.Update(5, new UpdateMedicineDto(), CancellationToken.None);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOk_When_Success()
        {
            var ctrl = MakeControllerWithUser();

            var result = await ctrl.Update(5, new UpdateMedicineDto(), CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
            _svc.Verify(s => s.UpdateAsync(5, It.IsAny<UpdateMedicineDto>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SoftDelete_ReturnsNotFound_When_KeyNotFound()
        {
            _svc.Setup(s => s.SoftDeleteAsync(9, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("not found"));

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.SoftDelete(9, CancellationToken.None);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task SoftDelete_ReturnsOk_When_Success()
        {
            var ctrl = MakeControllerWithUser();

            var result = await ctrl.SoftDelete(9, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
            _svc.Verify(s => s.SoftDeleteAsync(9, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetMine_ReturnsUnauthorized_When_No_UserId()
        {
            var ctrl = MakeControllerWithUser(userId: null);

            var result = await ctrl.GetMine(1, 10, null, null, CancellationToken.None);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task GetMine_ReturnsConflict_When_Service_Throws_InvalidOperation()
        {
            _svc.Setup(s => s.GetMinePagedAsync(1, 1, 10, null, null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("not provider"));

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.GetMine(1, 10, null, null, CancellationToken.None);

            result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task GetMine_ReturnsOk_When_Success()
        {
            _svc.Setup(s => s.GetMinePagedAsync(1, 1, 10, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<ReadMedicineDto> { Items = new List<ReadMedicineDto>() });

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.GetMine(1, 10, null, null, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
