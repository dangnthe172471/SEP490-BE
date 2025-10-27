using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UsersController _controller;

        public UserControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _controller = new UsersController(_userServiceMock.Object);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnCreatedResult_WhenSuccessful()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Phone = "0909123456",
                Password = "password123",
                FullName = "Test User",
                RoleId = 1
            };
            var userDto = new UserDto { UserId = 1, FullName = "Test User" };
            _userServiceMock
                .Setup(s => s.CreateUserAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userDto);

            // Act
            var result = await _controller.CreateUser(request, CancellationToken.None);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<UserDto>(createdResult.Value);
            Assert.Equal(1, returnValue.UserId);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenPhoneAlreadyExists()
        {
            // Arrange
            var request = new CreateUserRequest { Phone = "0909123456" };
            _userServiceMock
                .Setup(s => s.CreateUserAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Số điện thoại đã được sử dụng."));

            // Act
            var result = await _controller.CreateUser(request, CancellationToken.None);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Số điện thoại đã được sử dụng", badRequest.Value.ToString());
        }

        [Fact]
        public async Task CreateUser_ShouldReturn500_WhenUnhandledExceptionOccurs()
        {
            // Arrange
            var request = new CreateUserRequest();
            _userServiceMock
                .Setup(s => s.CreateUserAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.CreateUser(request, CancellationToken.None);

            // Assert
            var objResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objResult.StatusCode);
        }
    }
}
