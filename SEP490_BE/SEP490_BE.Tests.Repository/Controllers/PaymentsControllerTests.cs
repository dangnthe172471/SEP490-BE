using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Net.payOS.Types;
using SEP490_BE.API.Controllers.PaymentControllers;
using SEP490_BE.BLL.IServices.IPaymentServices;
using SEP490_BE.DAL.DTOs.PaymentDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.Tests.Controllers
{

    public class PaymentsControllerTests
    {
        private readonly Mock<IPaymentService> _paymentServiceMock;
        private readonly Mock<IPayOSService> _payOSServiceMock;
        private readonly PaymentsController _controller;

        public PaymentsControllerTests()
        {
            _paymentServiceMock = new Mock<IPaymentService>();
            _payOSServiceMock = new Mock<IPayOSService>();

            _controller = new PaymentsController(_payOSServiceMock.Object, _paymentServiceMock.Object);
        }


        #region GetPaymentDetails

        [Fact]
        public async Task GetPaymentDetails_ShouldReturnOk_WithCorrectTotal()
        {
            // Arrange
            int recordId = 1;

            var items = new List<MedicalRecordServiceItemDTO>
    {
        new MedicalRecordServiceItemDTO { Name = "A", Quantity = 2, UnitPrice = 10 }, // 20
        new MedicalRecordServiceItemDTO { Name = "B", Quantity = 1, UnitPrice = 40 }  // 40
    };

            _paymentServiceMock
                .Setup(s => s.GetServicesForRecordAsync(recordId))
                .ReturnsAsync(items);

            // Act
            var result = await _controller.GetPaymentDetails(recordId) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();

            var value = result!.Value!;
            var type = value.GetType();

            var recordIdProp = type.GetProperty("recordId")!;
            var totalAmountProp = type.GetProperty("totalAmount")!;
            var itemsProp = type.GetProperty("items")!;

            var recordIdReturned = (int)recordIdProp.GetValue(value)!;
            var totalAmountReturned = (decimal)totalAmountProp.GetValue(value)!;
            var itemsReturned =
                (IEnumerable<MedicalRecordServiceItemDTO>)itemsProp.GetValue(value)!;

            recordIdReturned.Should().Be(1);
            totalAmountReturned.Should().Be(60);
            itemsReturned.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetPaymentDetails_ShouldReturnBadRequest_WhenRecordIdInvalid()
        {
            var result = await _controller.GetPaymentDetails(0) as BadRequestObjectResult;

            result.Should().NotBeNull();
            result!.Value.Should().Be("recordId must be greater than 0");
        }
        [Fact]
        public async Task GetPaymentDetails_ShouldReturnNotFound_WhenNoItems()
        {
            int recordId = 99;

            _paymentServiceMock
                .Setup(s => s.GetServicesForRecordAsync(recordId))
                .ReturnsAsync(new List<MedicalRecordServiceItemDTO>()); // hoặc null

            var result = await _controller.GetPaymentDetails(recordId) as NotFoundObjectResult;

            result.Should().NotBeNull();
            result!.Value.Should().Be($"No services found for record {recordId}");
        }
        #endregion


        #region CreatePayment
        [Fact]
        public async Task CreatePayment_NullDto_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.CreatePayment(null!);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            bad.Value!.ToString().Should().Contain("Dữ liệu thanh toán là bắt buộc");

            _paymentServiceMock.Verify(
                s => s.CreatePaymentAsync(It.IsAny<CreatePaymentRequestDTO>(),true),
                Times.Never);
        }
        [Fact]
        public async Task CreatePayment_InvalidMedicalRecordId_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreatePaymentRequestDTO
            {
                MedicalRecordId = 0,
                Amount = 500_000,
                Description = "Test invalid MRN",
                Items = new List<ItemDTO>()
            };

            // Act
            var result = await _controller.CreatePayment(dto);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            bad.Value!.ToString().Should().Contain("Mã hồ sơ y tế không hợp lệ");

            _paymentServiceMock.Verify(
                s => s.CreatePaymentAsync(It.IsAny<CreatePaymentRequestDTO>(), true),
                Times.Never);
        }
        [Fact]
        public async Task CreatePayment_InvalidAmount_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreatePaymentRequestDTO
            {
                MedicalRecordId = 1,
                Amount = 0,
                Description = "Test invalid amount",
                Items = new List<ItemDTO>()
            };

            // Act
            var result = await _controller.CreatePayment(dto);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            bad.Value!.ToString().Should().Contain("Số tiền thanh toán phải lớn hơn 0");

            _paymentServiceMock.Verify(
                s => s.CreatePaymentAsync(It.IsAny<CreatePaymentRequestDTO>(), true),
                Times.Never);
        }
        [Fact]
        public async Task CreatePayment_Success_ReturnsOkWithPaymentInfo()
        {
            // Arrange
            var dto = new CreatePaymentRequestDTO
            {
                MedicalRecordId = 1,
                Amount = 500_000,
                Description = "Thanh toán dịch vụ khám",
                Items = new List<ItemDTO>
        {
            new ItemDTO { Name = "Khám tổng quát", Quantity = 1, Price = 500_000 }
        }
            };

            var serviceResult = new CreatePaymentResponseDTO
            {
                PaymentId = 123,
                CheckoutUrl = "https://payos.example.com/checkout/123"
            };

            _paymentServiceMock
                .Setup(s => s.CreatePaymentAsync(dto, true))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.CreatePayment(dto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);

            ok.Value.Should().BeEquivalentTo(new
            {
                paymentId = 123,
                checkoutUrl = "https://payos.example.com/checkout/123"
            });

            _paymentServiceMock.Verify(
                s => s.CreatePaymentAsync(dto, true),
                Times.Once);
        }
        [Fact]
        public async Task CreatePayment_ServiceReturnsNull_Returns500()
        {
            // Arrange
            var dto = new CreatePaymentRequestDTO
            {
                MedicalRecordId = 1,
                Amount = 100_000,
                Description = "Test null result",
                Items = new List<ItemDTO>()
            };

            _paymentServiceMock
                .Setup(s => s.CreatePaymentAsync(dto, true))
                .ReturnsAsync((CreatePaymentResponseDTO?)null);

            // Act
            var result = await _controller.CreatePayment(dto);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
            obj.Value!.ToString().Should().Contain("Không tạo được giao dịch thanh toán");

            _paymentServiceMock.Verify(
                s => s.CreatePaymentAsync(dto, true),
                Times.Once);
        }
        [Fact]
        public async Task CreatePayment_ServiceThrows_ReturnsBadRequestWithMessage()
        {
            // Arrange
            var dto = new CreatePaymentRequestDTO
            {
                MedicalRecordId = 2,
                Amount = 200_000,
                Description = "Thanh toán test lỗi",
                Items = new List<ItemDTO>()
            };

            _paymentServiceMock
                .Setup(s => s.CreatePaymentAsync(dto, true))
                .ThrowsAsync(new Exception("Payment creation failed"));

            // Act
            var result = await _controller.CreatePayment(dto);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            bad.Value!.ToString().Should().Contain("Payment creation failed");

            _paymentServiceMock.Verify(
                s => s.CreatePaymentAsync(dto, true),
                Times.Once);
        }

        #endregion

        #region callback

        [Fact]
        public async Task Callback_ShouldReturnBadRequest_WhenSignatureInvalid()
        {
            _payOSServiceMock
                .Setup(s => s.VerifyWebhook(It.IsAny<WebhookType>()))
                .Throws(new Exception("Invalid signature"));

            var webhook = new WebhookType(
     code: "dummy",
     desc: "dummy",
     success: true,
     data: null,          // có thể để null vì bạn mock VerifyWebhook
     signature: "dummy"
 );

            var result = await _controller.Callback(webhook) as BadRequestObjectResult;


            result.Should().NotBeNull();
            ((string)result!.Value!).Should().Be("Invalid signature");
        }

        [Theory]
        [InlineData("00", "PAID")]
        [InlineData("01", "PENDING")]
        [InlineData("09", "FAILED")]
        [InlineData("XX", "UNKNOWN")]
        public async Task Callback_ShouldUpdateStatus_CoverAllBranches(string code, string expectedStatus)
        {
            var webhook = new WebhookType(
                code: "dummy",
                desc: "dummy",
                success: true,
                data: null,
                signature: "dummy"
            );

            var webhookResult = new WebhookData(
                orderCode: 999,
                amount: 100,
                description: "",
                accountNumber: "",
                reference: "",
                transactionDateTime: "",
                currency: "",
                paymentLinkId: "",
                code: code,        // <── bạn chỉ cần set đúng field này
                desc: "",
                counterAccountBankId: null,
                counterAccountBankName: null,
                counterAccountName: null,
                counterAccountNumber: null,
                virtualAccountName: null,
                virtualAccountNumber: null
            );

            _payOSServiceMock
                .Setup(s => s.VerifyWebhook(webhook))
                .Returns(webhookResult);

            _paymentServiceMock
                .Setup(s => s.UpdatePaymentStatusAsync(999, expectedStatus))
                .Returns(Task.CompletedTask);

            var result = await _controller.Callback(webhook);

            result.Should().BeOfType<OkResult>();
        }

        #endregion

        #region GetPaymentStatus
        [Fact]
        public async Task GetPaymentStatus_ValidId_ReturnsOkWithDto()
        {
            // Arrange
            int recordId = 123;
            var dto = new PaymentStatusDTO
            {
                RecordId = recordId,
                Status = "PAID",
                CheckoutUrl = "https://payos.vn/checkout/123"
            };

            _paymentServiceMock
                .Setup(s => s.GetPaymentStatusAsync(recordId))
                .ReturnsAsync(dto);

            // Act
            var result = await _controller.GetPaymentStatus(recordId) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            var value = result.Value as PaymentStatusDTO;
            value.Should().NotBeNull();
            value!.RecordId.Should().Be(recordId);
            value.Status.Should().Be("PAID");
            value.CheckoutUrl.Should().Be("https://payos.vn/checkout/123");

            _paymentServiceMock.Verify(
                s => s.GetPaymentStatusAsync(recordId),
                Times.Once);
        }
        [Fact]
        public async Task GetPaymentStatus_InvalidRecordId_ReturnsBadRequest_AndDoesNotCallService()
        {
            // Arrange
            int recordId = 0;

            // Act
            var result = await _controller.GetPaymentStatus(recordId) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);
            result.Value!.ToString().Should().Contain("RecordId must be greater than 0");

            _paymentServiceMock.Verify(
                s => s.GetPaymentStatusAsync(It.IsAny<int>()),
                Times.Never);
        }
        [Fact]
        public async Task GetPaymentStatus_NotFound_Returns404()
        {
            // Arrange
            int recordId = 999;

            _paymentServiceMock
                .Setup(s => s.GetPaymentStatusAsync(recordId))
                .ReturnsAsync((PaymentStatusDTO?)null);

            // Act
            var result = await _controller.GetPaymentStatus(recordId) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(404);
            result.Value!.ToString().Should().Contain("Không tìm thấy thông tin thanh toán");

            _paymentServiceMock.Verify(
                s => s.GetPaymentStatusAsync(recordId),
                Times.Once);
        }
        [Fact]
        public async Task GetPaymentStatus_ServiceThrows_Returns500()
        {
            // Arrange
            int recordId = 123;

            _paymentServiceMock
                .Setup(s => s.GetPaymentStatusAsync(recordId))
                .ThrowsAsync(new Exception("Database failure"));

            // Act
            var result = await _controller.GetPaymentStatus(recordId) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

            var json = result.Value!.ToString()!;
            json.Should().Contain("Có lỗi xảy ra khi lấy trạng thái thanh toán");
            json.Should().Contain("Database failure");

            _paymentServiceMock.Verify(
                s => s.GetPaymentStatusAsync(recordId),
                Times.Once);
        }

        #endregion

        #region GetPaymentsChart
        [Fact]
        public async Task GetPayments_ValidRange_ReturnsOkWithChartData()
        {
            // Arrange
            var start = new DateTime(2025, 1, 1);
            var end = new DateTime(2025, 1, 31);

            var chartData = new List<PaymentChartDto>
    {
        new PaymentChartDto { PaymentDate = new DateTime(2025, 1, 5), Amount = 100_000m },
        new PaymentChartDto { PaymentDate = new DateTime(2025, 1, 10), Amount = 200_000m }
    };

            _paymentServiceMock
                .Setup(s => s.GetPaymentsForChartAsync(start, end))
                .ReturnsAsync(chartData);

            // Act
            var result = await _controller.GetPayments(start, end) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var value = result.Value as List<PaymentChartDto>;
            value.Should().NotBeNull();
            value!.Count.Should().Be(2);
            value[0].PaymentDate.Should().Be(new DateTime(2025, 1, 5));
            value[0].Amount.Should().Be(100_000m);

            _paymentServiceMock.Verify(
                s => s.GetPaymentsForChartAsync(start, end),
                Times.Once);
        }
        [Fact]
        public async Task GetPayments_InvalidDateRange_ReturnsBadRequest_AndDoesNotCallService()
        {
            // Arrange
            var start = new DateTime(2025, 2, 1);
            var end = new DateTime(2025, 1, 1); // start > end

            // Act
            var result = await _controller.GetPayments(start, end) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            result.Value!.ToString().Should().Contain("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc");

            _paymentServiceMock.Verify(
                s => s.GetPaymentsForChartAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()),
                Times.Never);
        }
        [Fact]
        public async Task GetPayments_ServiceThrows_Returns500()
        {
            // Arrange
            var start = new DateTime(2025, 1, 1);
            var end = new DateTime(2025, 1, 31);

            _paymentServiceMock
                .Setup(s => s.GetPaymentsForChartAsync(start, end))
                .ThrowsAsync(new Exception("Database failure"));

            // Act
            var result = await _controller.GetPayments(start, end) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

            var json = result.Value!.ToString()!;
            json.Should().Contain("Có lỗi xảy ra khi lấy dữ liệu thanh toán cho biểu đồ");
            json.Should().Contain("Database failure");

            _paymentServiceMock.Verify(
                s => s.GetPaymentsForChartAsync(start, end),
                Times.Once);
        }

        #endregion
    }
}
