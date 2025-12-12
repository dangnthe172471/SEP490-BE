using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.PrescriptionDoctorDTO;
using System.Security.Claims;
using System.Text.Json;

namespace SEP490_BE.Tests.Controllers
{
    public class PrescriptionsDoctorControllerTests
    {
        private static PrescriptionsDoctorController BuildController(
            Mock<IPrescriptionDoctorService> mockService,
            int userId = 123)
        {
            var controller = new PrescriptionsDoctorController(mockService.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "Doctor")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            return controller;
        }

        private static CreatePrescriptionRequest MakeReq(
            int recordId,
            DateTime? issued = null,
            IEnumerable<(int medicineId, string dosage, string duration)>? items = null)
        {
            return new CreatePrescriptionRequest
            {
                RecordId = recordId,
                IssuedDate = issued,
                Items = (items ?? new[] { (1, "2 viên/ngày", "5 ngày") })
                    .Select(t => new CreatePrescriptionItem
                    {
                        MedicineId = t.medicineId,
                        Dosage = t.dosage,
                        Duration = t.duration
                    }).ToList()
            };
        }

        [Fact(DisplayName = "Create - hợp lệ đầy đủ trường → trả 201 Created")]
        public async Task Create_Should_Return201_When_Valid_AllFields()
        {
            // Arrange
            var mock = new Mock<IPrescriptionDoctorService>();
            var expected = new PrescriptionSummaryDto
            {
                PrescriptionId = 1001,
                IssuedDate = DateTime.UtcNow
            };

            mock.Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CreatePrescriptionRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var controller = BuildController(mock);
            var req = MakeReq(
                recordId: 10,
                issued: DateTime.Parse("2025-11-03T17:35:23.848Z"),
                items: new[] { (1, "2 viên/ngày", "5 ngày") });

            // Act
            var result = await controller.Create(req, CancellationToken.None);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, created.StatusCode);
            var json = JsonSerializer.Serialize(created.Value);
            var doc = JsonDocument.Parse(json);
            var dataElement = doc.RootElement.GetProperty("data");
            var payload = JsonSerializer.Deserialize<PrescriptionSummaryDto>(dataElement.GetRawText());
            Assert.NotNull(payload);
            Assert.Equal(1001, payload.PrescriptionId);
        }

        [Fact(DisplayName = "Create - không truyền IssuedDate → vẫn 201 Created (Service dùng UtcNow)")]
        public async Task Create_Should_Return201_When_IssuedDate_Null()
        {
            // Arrange
            var mock = new Mock<IPrescriptionDoctorService>();
            var expected = new PrescriptionSummaryDto
            {
                PrescriptionId = 1002,
                IssuedDate = DateTime.UtcNow
            };

            mock.Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CreatePrescriptionRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var controller = BuildController(mock);
            var req = MakeReq(
                recordId: 11,
                issued: null,
                items: new[] { (2, "1 viên/ngày", "7 ngày") });

            // Act
            var result = await controller.Create(req, CancellationToken.None);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, created.StatusCode);
            var json = JsonSerializer.Serialize(created.Value);
            var doc = JsonDocument.Parse(json);
            var dataElement = doc.RootElement.GetProperty("data");
            var payload = JsonSerializer.Deserialize<PrescriptionSummaryDto>(dataElement.GetRawText());
            Assert.NotNull(payload);
            Assert.Equal(1002, payload.PrescriptionId);
        }

        [Fact(DisplayName = "Create - recordId không tồn tại → Service ném InvalidOperationException('Hồ sơ bệnh án không tồn tại.')")]
        public async Task Create_Should_Throw_InvalidOperation_When_Record_NotFound()
        {
            // Arrange
            var mock = new Mock<IPrescriptionDoctorService>();
            mock.Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CreatePrescriptionRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Hồ sơ bệnh án không tồn tại."));

            var controller = BuildController(mock);
            var req = MakeReq(
                recordId: 999,
                issued: DateTime.Parse("2025-11-03T17:35:23.848Z"),
                items: new[] { (1, "2 viên/ngày", "5 ngày") });

            // Act
            var result = await controller.Create(req, CancellationToken.None);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);
            var json = JsonSerializer.Serialize(badRequest.Value);
            var doc = JsonDocument.Parse(json);
            var message = doc.RootElement.GetProperty("message").GetString();
            Assert.Equal("Hồ sơ bệnh án không tồn tại.", message);
        }

        [Fact(DisplayName = "Create - hồ sơ thuộc bác sĩ khác → Service ném UnauthorizedAccessException('Bạn không phụ trách hồ sơ này.')")]
        public async Task Create_Should_Throw_UnauthorizedAccess_When_Not_Record_Owner()
        {
            // Arrange
            var mock = new Mock<IPrescriptionDoctorService>();
            mock.Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CreatePrescriptionRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnauthorizedAccessException("Bạn không phụ trách hồ sơ này."));

            var controller = BuildController(mock);
            var req = MakeReq(
                recordId: 12,
                issued: DateTime.Parse("2025-11-03T17:35:23.848Z"),
                items: new[] { (1, "2 viên/ngày", "5 ngày") });

            // Act
            var result = await controller.Create(req, CancellationToken.None);

            // Assert
            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
            var json = JsonSerializer.Serialize(forbidden.Value);
            var doc = JsonDocument.Parse(json);
            var message = doc.RootElement.GetProperty("message").GetString();
            Assert.Equal("Bạn không phụ trách hồ sơ này.", message);
        }

        [Fact(DisplayName = "Create - Items rỗng → Service ném InvalidOperationException('Đơn thuốc phải có ít nhất 1 dòng.')")]
        public async Task Create_Should_Throw_InvalidOperation_When_Items_Empty()
        {
            // Arrange
            var mock = new Mock<IPrescriptionDoctorService>();
            mock.Setup(s => s.CreateAsync(It.IsAny<int>(), It.Is<CreatePrescriptionRequest>(r => r.Items.Count == 0), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Đơn thuốc phải có ít nhất 1 dòng."));

            var controller = BuildController(mock);
            var req = new CreatePrescriptionRequest
            {
                RecordId = 10,
                IssuedDate = DateTime.Parse("2025-11-03T17:35:23.848Z"),
                Items = new List<CreatePrescriptionItem>()
            };

            // Act
            var result = await controller.Create(req, CancellationToken.None);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);
            var json = JsonSerializer.Serialize(badRequest.Value);
            var doc = JsonDocument.Parse(json);
            var message = doc.RootElement.GetProperty("message").GetString();
            Assert.Equal("Đơn thuốc phải có ít nhất 1 dòng.", message);
        }

        [Fact(DisplayName = "Create - Thuốc không tồn tại → Service ném InvalidOperationException('Một hoặc nhiều thuốc không hợp lệ.')")]
        public async Task Create_Should_Throw_InvalidOperation_When_Medicine_NotFound()
        {
            // Arrange
            var mock = new Mock<IPrescriptionDoctorService>();
            mock.Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CreatePrescriptionRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Một hoặc nhiều thuốc không hợp lệ."));

            var controller = BuildController(mock);
            var req = MakeReq(
                recordId: 10,
                issued: DateTime.Parse("2025-11-03T17:35:23.848Z"),
                items: new[] { (99, "1 viên/ngày", "5 ngày") });

            // Act
            var result = await controller.Create(req, CancellationToken.None);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);
            var json = JsonSerializer.Serialize(badRequest.Value);
            var doc = JsonDocument.Parse(json);
            var message = doc.RootElement.GetProperty("message").GetString();
            Assert.Equal("Một hoặc nhiều thuốc không hợp lệ.", message);
        }

        [Fact(DisplayName = "Create - Lỗi hệ thống khi lưu DB → Service ném Exception('DB error')")]
        public async Task Create_Should_Throw_Exception_When_DbFails()
        {
            // Arrange
            var mock = new Mock<IPrescriptionDoctorService>();
            mock.Setup(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<CreatePrescriptionRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB error"));

            var controller = BuildController(mock);
            var req = MakeReq(
                recordId: 10,
                issued: DateTime.Parse("2025-11-03T17:35:23.848Z"),
                items: new[] { (1, "2 viên/ngày", "5 ngày") });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => controller.Create(req, CancellationToken.None));
            Assert.Equal("DB error", ex.Message);
        }

        [Fact(DisplayName = "GetById - hợp lệ, user là Doctor → trả 200 Ok với PrescriptionSummaryDto")]
        public async Task GetById_Should_Return200_When_Valid_Doctor()
        {
            // Arrange
            var mock = new Mock<IPrescriptionDoctorService>();
            var expected = new PrescriptionSummaryDto
            {
                PrescriptionId = 1001,
                IssuedDate = DateTime.UtcNow
            };

            mock.Setup(s => s.GetByIdAsync(It.IsAny<int>(), 1001, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var controller = BuildController(mock, userId: 123);

            // Act
            var result = await controller.GetById(1001, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, ok.StatusCode);
            var payload = Assert.IsType<PrescriptionSummaryDto>(ok.Value);
            Assert.Equal(1001, payload.PrescriptionId);
        }

        [Fact(DisplayName = "GetById - prescription không tồn tại → trả 404 NotFound")]
        public async Task GetById_Should_Return404_When_NotFound()
        {
            // Arrange
            var mock = new Mock<IPrescriptionDoctorService>();
            mock.Setup(s => s.GetByIdAsync(It.IsAny<int>(), 999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PrescriptionSummaryDto?)null);

            var controller = BuildController(mock, userId: 123);

            // Act
            var result = await controller.GetById(999, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact(DisplayName = "GetById - user không có quyền truy cập → Service ném UnauthorizedAccessException → trả 403 Forbidden")]
        public async Task GetById_Should_Return403_When_Unauthorized()
        {
            // Arrange
            var mock = new Mock<IPrescriptionDoctorService>();
            mock.Setup(s => s.GetByIdAsync(It.IsAny<int>(), 1001, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnauthorizedAccessException("Bạn không có quyền xem đơn thuốc này."));

            var controller = BuildController(mock, userId: 123);

            // Act
            var result = await controller.GetById(1001, CancellationToken.None);

            // Assert
            var forbidden = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(403, forbidden.StatusCode);
            var json = JsonSerializer.Serialize(forbidden.Value);
            var doc = JsonDocument.Parse(json);
            var message = doc.RootElement.GetProperty("message").GetString();
            Assert.Equal("Bạn không có quyền xem đơn thuốc này.", message);
        }

        [Fact(DisplayName = "GetById - userId không hợp lệ (không parse được) → trả 401 Unauthorized")]
        public async Task GetById_Should_Return401_When_UserId_Invalid()
        {
            // Arrange
            var mock = new Mock<IPrescriptionDoctorService>();
            var controller = new PrescriptionsDoctorController(mock.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "invalid"),
                new Claim(ClaimTypes.Role, "Doctor")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await controller.GetById(1001, CancellationToken.None);

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
            mock.Verify(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact(DisplayName = "GetRecordsForDoctor - hợp lệ với tất cả params → trả 200 Ok với PagedResult")]
        public async Task GetRecordsForDoctor_Should_Return200_When_Valid()
        {
            // Arrange
            var mock = new Mock<IPrescriptionDoctorService>();
            var from = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
            var to = DateOnly.FromDateTime(DateTime.Today);
            var expected = new PagedResult<RecordListItemDto>
            {
                Items = new List<RecordListItemDto>
                {
                    new RecordListItemDto { RecordId = 1, PatientName = "John Doe" }
                },
                PageNumber = 1,
                PageSize = 20,
                TotalCount = 1
            };

            mock.Setup(s => s.GetRecordsForDoctorAsync(It.IsAny<int>(), from, to, "test", 1, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var controller = BuildController(mock, userId: 123);

            // Act
            var result = await controller.GetRecordsForDoctor(from, to, "test", 1, 20, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, ok.StatusCode);
            var payload = Assert.IsType<PagedResult<RecordListItemDto>>(ok.Value);
            Assert.Equal(1, payload.TotalCount);
            Assert.Single(payload.Items);
        }

        [Fact(DisplayName = "GetRecordsForDoctor - pageNumber <= 0 → tự động clamp về 1")]
        public async Task GetRecordsForDoctor_Should_ClampPageNumber_When_Invalid()
        {
            // Arrange
            var mock = new Mock<IPrescriptionDoctorService>();
            var expected = new PagedResult<RecordListItemDto>
            {
                Items = new List<RecordListItemDto>(),
                PageNumber = 1,
                PageSize = 20,
                TotalCount = 0
            };

            mock.Setup(s => s.GetRecordsForDoctorAsync(It.IsAny<int>(), null, null, null, 1, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var controller = BuildController(mock, userId: 123);

            // Act
            var result = await controller.GetRecordsForDoctor(null, null, null, 0, 20, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            mock.Verify(s => s.GetRecordsForDoctorAsync(It.IsAny<int>(), null, null, null, 1, 20, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "GetRecordsForDoctor - pageSize > 100 → tự động clamp về 100")]
        public async Task GetRecordsForDoctor_Should_ClampPageSize_When_TooLarge()
        {
            // Arrange
            var mock = new Mock<IPrescriptionDoctorService>();
            var expected = new PagedResult<RecordListItemDto>
            {
                Items = new List<RecordListItemDto>(),
                PageNumber = 1,
                PageSize = 100,
                TotalCount = 0
            };

            mock.Setup(s => s.GetRecordsForDoctorAsync(It.IsAny<int>(), null, null, null, 1, 100, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var controller = BuildController(mock, userId: 123);

            // Act
            var result = await controller.GetRecordsForDoctor(null, null, null, 1, 200, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            mock.Verify(s => s.GetRecordsForDoctorAsync(It.IsAny<int>(), null, null, null, 1, 100, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "GetRecordsForDoctor - Service ném InvalidOperationException → trả 400 BadRequest")]
        public async Task GetRecordsForDoctor_Should_Return400_When_InvalidOperation()
        {
            // Arrange
            var mock = new Mock<IPrescriptionDoctorService>();
            mock.Setup(s => s.GetRecordsForDoctorAsync(It.IsAny<int>(), It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Invalid date range."));

            var controller = BuildController(mock, userId: 123);

            // Act
            var result = await controller.GetRecordsForDoctor(null, null, null, 1, 20, CancellationToken.None);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequest.StatusCode);
            var json = JsonSerializer.Serialize(badRequest.Value);
            var doc = JsonDocument.Parse(json);
            var message = doc.RootElement.GetProperty("message").GetString();
            Assert.Equal("Invalid date range.", message);
        }

        [Fact(DisplayName = "GetRecordsForDoctor - userId không hợp lệ → trả 401 Unauthorized")]
        public async Task GetRecordsForDoctor_Should_Return401_When_UserId_Invalid()
        {
            // Arrange
            var mock = new Mock<IPrescriptionDoctorService>();
            var controller = new PrescriptionsDoctorController(mock.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "invalid"),
                new Claim(ClaimTypes.Role, "Doctor")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await controller.GetRecordsForDoctor(null, null, null, 1, 20, CancellationToken.None);

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
            mock.Verify(s => s.GetRecordsForDoctorAsync(It.IsAny<int>(), It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
