using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using SEP490_BE.API.Controllers.PaymentControllers;
using SEP490_BE.BLL.IServices.IPaymentServices;
using SEP490_BE.DAL.DTOs.PaymentDTO;

namespace SEP490_BE.Tests.Controllers
{
    public class ReceptionPaymentControllerTests
    {
        private readonly Mock<IPaymentService> _paymentServiceMock = new(MockBehavior.Strict);
        private readonly Mock<IConfiguration> _configMock = new();

        private ReceptionPaymentController CreateController()
        {
            return new ReceptionPaymentController(_paymentServiceMock.Object, _configMock.Object);
        }

        #region GenerateQr Tests

        [Fact]
        public void GenerateQr_WithValidDto_ReturnsOk()
        {
            // Arrange
            var dto = new GenerateQrDto
            {
                Amount = 100000,
                AddInfo = "Test payment"
            };

            var expectedResult = new QrResultDto
            {
                QrUrl = "https://example.com/qr"
            };

            _paymentServiceMock.Setup(s => s.GenerateQrLink(dto, _configMock.Object))
                .Returns(expectedResult);

            var controller = CreateController();

            // Act
            var result = controller.GenerateQr(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<QrResultDto>(okResult.Value);
            value.QrUrl.Should().Be("https://example.com/qr");
            _paymentServiceMock.VerifyAll();
        }

        [Fact]
        public void GenerateQr_WithException_ReturnsInternalServerError()
        {
            // Arrange
            var dto = new GenerateQrDto
            {
                Amount = 100000,
                AddInfo = "Test payment"
            };

            _paymentServiceMock.Setup(s => s.GenerateQrLink(dto, _configMock.Object))
                .Throws(new Exception("Payment service error"));

            var controller = CreateController();

            // Act
            var result = controller.GenerateQr(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().NotBeNull();
            _paymentServiceMock.VerifyAll();
        }

        #endregion
    }
}

