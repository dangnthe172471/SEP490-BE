using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
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

        // ========================= SUCCESS CASES =========================

        [Fact(DisplayName = "Create - hợp lệ đầy đủ trường → trả 201 Created")]
        // Case #1: Hợp lệ đầy đủ (record ok, doctor phụ trách, thuốc hợp lệ, có items, có issuedDate)
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
            // The controller returns an anonymous object with message and data
            var json = JsonSerializer.Serialize(created.Value);
            var doc = JsonDocument.Parse(json);
            var dataElement = doc.RootElement.GetProperty("data");
            var payload = JsonSerializer.Deserialize<PrescriptionSummaryDto>(dataElement.GetRawText());
            Assert.NotNull(payload);
            Assert.Equal(1001, payload.PrescriptionId);
        }

        [Fact(DisplayName = "Create - không truyền IssuedDate → vẫn 201 Created (Service dùng UtcNow)")]
        // Case #2: Không có issuedDate → Service tự dùng DateTime.UtcNow
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
            // The controller returns an anonymous object with message and data
            var json = JsonSerializer.Serialize(created.Value);
            var doc = JsonDocument.Parse(json);
            var dataElement = doc.RootElement.GetProperty("data");
            var payload = JsonSerializer.Deserialize<PrescriptionSummaryDto>(dataElement.GetRawText());
            Assert.NotNull(payload);
            Assert.Equal(1002, payload.PrescriptionId);
        }

        // ========================= BUSINESS / DATA ERRORS =========================

        [Fact(DisplayName = "Create - recordId không tồn tại → Service ném InvalidOperationException('Hồ sơ bệnh án không tồn tại.')")]
        // Case #3: recordId không tồn tại
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

            // Assert - Controller catches exception and returns BadRequest
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);
            var json = JsonSerializer.Serialize(badRequest.Value);
            var doc = JsonDocument.Parse(json);
            var message = doc.RootElement.GetProperty("message").GetString();
            Assert.Equal("Hồ sơ bệnh án không tồn tại.", message);
        }

        [Fact(DisplayName = "Create - hồ sơ thuộc bác sĩ khác → Service ném UnauthorizedAccessException('Bạn không phụ trách hồ sơ này.')")]
        // Case #4: Không phải bác sĩ phụ trách
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

            // Assert - Controller catches exception and returns 403 Forbidden
            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
            var json = JsonSerializer.Serialize(forbidden.Value);
            var doc = JsonDocument.Parse(json);
            var message = doc.RootElement.GetProperty("message").GetString();
            Assert.Equal("Bạn không phụ trách hồ sơ này.", message);
        }

        [Fact(DisplayName = "Create - Items rỗng → Service ném InvalidOperationException('Đơn thuốc phải có ít nhất 1 dòng.')")]
        // Case #5: Items rỗng
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
                Items = new List<CreatePrescriptionItem>() // rỗng
            };

            // Act
            var result = await controller.Create(req, CancellationToken.None);

            // Assert - Controller catches exception and returns BadRequest
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);
            var json = JsonSerializer.Serialize(badRequest.Value);
            var doc = JsonDocument.Parse(json);
            var message = doc.RootElement.GetProperty("message").GetString();
            Assert.Equal("Đơn thuốc phải có ít nhất 1 dòng.", message);
        }

        [Fact(DisplayName = "Create - Thuốc không tồn tại → Service ném InvalidOperationException('Một hoặc nhiều thuốc không hợp lệ.')")]
        // Case #6: Thuốc không tồn tại (hoặc list id không khớp)
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

            // Assert - Controller catches exception and returns BadRequest
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);
            var json = JsonSerializer.Serialize(badRequest.Value);
            var doc = JsonDocument.Parse(json);
            var message = doc.RootElement.GetProperty("message").GetString();
            Assert.Equal("Một hoặc nhiều thuốc không hợp lệ.", message);
        }

        // ========================= SYSTEM ERROR =========================

        [Fact(DisplayName = "Create - Lỗi hệ thống khi lưu DB → Service ném Exception('DB error')")]
        // Case #7: DB/EF exception
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
    }
}
