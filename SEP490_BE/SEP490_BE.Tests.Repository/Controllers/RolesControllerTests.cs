using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers
{
    public class RolesControllerTests
    {
        private readonly Mock<IRoleService> _svc = new();

        private RolesController MakeControllerWithUser(int? userId = 1, string? role = "Admin")
        {
            var controller = new RolesController(_svc.Object);

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
        public async Task GetAll_ReturnsOk_WithRoles()
        {
            _svc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RoleDto>
                {
                    new() { RoleId = 1, RoleName = "Admin" },
                    new() { RoleId = 2, RoleName = "Doctor" }
                });

            var ctrl = MakeControllerWithUser();
            var result = await ctrl.GetAll(CancellationToken.None);
            var ok = result as OkObjectResult;

            ok.Should().NotBeNull();
            (ok!.Value as IEnumerable<RoleDto>).Should().HaveCount(2);
        }

        [Fact]
        public void GetAll_HasAuthorizeAttribute()
        {
            var method = typeof(RolesController).GetMethod(nameof(RolesController.GetAll));
            var attributes = method?.GetCustomAttributes(typeof(AuthorizeAttribute), true);

            attributes.Should().NotBeNull();
            attributes!.Length.Should().BeGreaterThan(0);
        }
    }
}

