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
    public class ServicesControllerTests
    {
        private readonly Mock<IServiceService> _svc = new();

        private ServicesController MakeControllerWithUser(int? userId = 1, string? role = "Clinic Manager")
        {
            var controller = new ServicesController(_svc.Object);

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
                .ReturnsAsync(new List<ServiceDto>
                {
                    new() { ServiceId = 1, ServiceName = "Dermatology A" },
                    new() { ServiceId = 2, ServiceName = "InternalMed B" }
                });

            var ctrl = MakeControllerWithUser();

            var result = await ctrl.GetAll(CancellationToken.None);
            var ok = result.Result as OkObjectResult;

            ok.Should().NotBeNull();
            (ok!.Value as IEnumerable<ServiceDto>).Should().HaveCount(2);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenFound()
        {
            _svc.Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ServiceDto { ServiceId = 5, ServiceName = "Svc" });

            var ctrl = MakeControllerWithUser();
            var result = await ctrl.GetById(5, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenMissing()
        {
            _svc.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ServiceDto?)null);

            var ctrl = MakeControllerWithUser();
            var result = await ctrl.GetById(999, CancellationToken.None);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetPaged_ReturnsOk()
        {
            _svc.Setup(s => s.GetPagedAsync(1, 10, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResponse<ServiceDto>
                {
                    Items = new List<ServiceDto> { new() { ServiceId = 1, ServiceName = "Svc" } },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var ctrl = MakeControllerWithUser();
            var result = await ctrl.GetPaged(1, 10, null, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsCreated_WhenSuccess()
        {
            _svc.Setup(s => s.CreateAsync(It.IsAny<CreateServiceRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(77);

            var ctrl = MakeControllerWithUser();
            var req = new CreateServiceRequest { ServiceName = "New", IsActive = true };

            var result = await ctrl.Create(req, CancellationToken.None);

            result.Result.Should().BeOfType<CreatedAtActionResult>();
            _svc.Verify(s => s.CreateAsync(req, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_OnArgumentException()
        {
            _svc.Setup(s => s.CreateAsync(It.IsAny<CreateServiceRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Invalid"));

            var ctrl = MakeControllerWithUser();
            var req = new CreateServiceRequest { ServiceName = "" };

            var result = await ctrl.Create(req, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOk_WhenSuccess()
        {
            _svc.Setup(s => s.UpdateAsync(5, It.IsAny<UpdateServiceRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ServiceDto { ServiceId = 5, ServiceName = "Updated" });

            var ctrl = MakeControllerWithUser();
            var req = new UpdateServiceRequest { ServiceName = "Updated", IsActive = true };

            var result = await ctrl.Update(5, req, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
            _svc.Verify(s => s.UpdateAsync(5, req, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenMissing()
        {
            _svc.Setup(s => s.UpdateAsync(999, It.IsAny<UpdateServiceRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ServiceDto?)null);

            var ctrl = MakeControllerWithUser();
            var req = new UpdateServiceRequest { ServiceName = "Updated", IsActive = true };

            var result = await ctrl.Update(999, req, CancellationToken.None);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_OnArgumentException()
        {
            _svc.Setup(s => s.UpdateAsync(5, It.IsAny<UpdateServiceRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Invalid"));

            var ctrl = MakeControllerWithUser();
            var req = new UpdateServiceRequest { ServiceName = "" };

            var result = await ctrl.Update(5, req, CancellationToken.None);

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
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            _svc.Setup(s => s.DeleteAsync(404, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var ctrl = MakeControllerWithUser();
            var result = await ctrl.Delete(404, CancellationToken.None);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsBadRequest_OnInvalidOperation()
        {
            _svc.Setup(s => s.DeleteAsync(5, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Has dependencies"));

            var ctrl = MakeControllerWithUser();
            var result = await ctrl.Delete(5, CancellationToken.None);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}

