using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;

namespace SEP490_BE.Tests.Controllers
{
    public class UploadsControllerTests
    {
        private readonly Mock<IWebHostEnvironment> _envMock = new();

        private UploadsController CreateController(string webRootPath = "C:\\wwwroot")
        {
            _envMock.Setup(e => e.WebRootPath).Returns(webRootPath);
            return new UploadsController(_envMock.Object);
        }

        #region UploadAttachment Tests

        [Fact]
        public async Task UploadAttachment_WithValidFile_ReturnsOk()
        {
            // Arrange
            var webRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(webRootPath);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.jpg");
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var controller = CreateController(webRootPath);

            // Act
            var result = await controller.UploadAttachment(fileMock.Object, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().NotBeNull();

            // Cleanup
            try
            {
                Directory.Delete(webRootPath, true);
            }
            catch (Exception)
            {
                // Ignore cleanup errors
            }
        }

        [Fact]
        public async Task UploadAttachment_WithNullFile_ReturnsBadRequest()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.UploadAttachment(null!, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            badRequestResult.Value.Should().Be("File không hợp lệ.");
        }

        [Fact]
        public async Task UploadAttachment_WithEmptyFile_ReturnsBadRequest()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0);

            var controller = CreateController();

            // Act
            var result = await controller.UploadAttachment(fileMock.Object, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            badRequestResult.Value.Should().Be("File không hợp lệ.");
        }

        #endregion
    }
}

