using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers.ManagerControllers;
using SEP490_BE.BLL.IServices.IDoctorServices;
using SEP490_BE.BLL.IServices.IManagerService;
using SEP490_BE.BLL.IServices.IManagerServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.DTOs.ManagerDTO.ManagerSchedule;
using SEP490_BE.DAL.DTOs.ManagerDTO.Notification;
using SEP490_BE.DAL.Helpers;
using SEP490_BE.DAL.Models;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers
{
	public class ManageScheduleControllerTests {
        private readonly Mock<IScheduleService> _serviceMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<IDoctorScheduleService> _doctorScheduleServiceMock;
        private readonly ManageScheduleController _controller;

        public ManageScheduleControllerTests()
        {
            _serviceMock = new Mock<IScheduleService>();
            _notificationServiceMock = new Mock<INotificationService>();
            _doctorScheduleServiceMock = new Mock<IDoctorScheduleService>();
            _controller = new ManageScheduleController(
                _serviceMock.Object,
                _notificationServiceMock.Object,
                _doctorScheduleServiceMock.Object
            );

            // Set up ControllerContext with "Clinic Manager" role for authorization
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Clinic Manager")
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        #region GetAllShifts
        [Fact]
        public async Task GetAllShifts_ReturnsOk_WithShiftList()
        {
            // Arrange
            var shifts = new List<ShiftResponseDTO>
    {
        new ShiftResponseDTO
        {
            ShiftID = 1,
            ShiftType = "Sang",
            StartTime = new TimeOnly(8, 0),
            EndTime   = new TimeOnly(12, 0)
        },
        new ShiftResponseDTO
        {
            ShiftID = 2,
            ShiftType = "Chieu",
            StartTime = new TimeOnly(13, 0),
            EndTime   = new TimeOnly(17, 0)
        }
    };

            _serviceMock
                .Setup(s => s.GetAllShiftsAsync())
                .ReturnsAsync(shifts);

            // Act
            var result = await _controller.GetAllShifts();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<ShiftResponseDTO>>(ok.Value);
            Assert.Equal(2, data.Count);
            Assert.Equal("Sang", data[0].ShiftType);

            _serviceMock.VerifyAll();
        }
        [Fact]
        public async Task GetAllShifts_WhenServiceThrows_Returns500()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetAllShiftsAsync())
                .ThrowsAsync(new Exception("Database failure"));

            // Act
            var result = await _controller.GetAllShifts();

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
            Assert.Contains("Có lỗi xảy ra khi lấy danh sách ca làm việc",
                            obj.Value!.ToString());

            _serviceMock.VerifyAll();
        }

        #endregion

        #region SearchDoctors
        [Fact]
        public async Task SearchDoctors_WhenKeywordEmpty_ShouldReturnAllDoctors()
        {
            _serviceMock.Setup(s => s.GetAllDoctorsAsync())
                .ReturnsAsync(new List<DoctorDTO> { new DoctorDTO { DoctorID = 1, FullName = "BS A" } });

            var result = await _controller.SearchDoctors("");

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<DoctorDTO>>(ok.Value);
            Assert.Single(data);
        }

        [Fact]
        public async Task SearchDoctors_WithKeyword_ShouldReturnFiltered()
        {
            _serviceMock.Setup(s => s.SearchDoctorsAsync("A"))
                .ReturnsAsync(new List<DoctorDTO> { new DoctorDTO { DoctorID = 1, FullName = "BS A" } });

            var result = await _controller.SearchDoctors("A");

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<DoctorDTO>>(ok.Value);
            Assert.Equal("BS A", data[0].FullName);
        }

        #endregion

        #region GetAllDoctors
        [Fact]
        public async Task GetAllDoctors_ReturnsOk_WithDoctorList()
        {
            // Arrange
            var doctors = new List<DoctorDTO>
    {
        new DoctorDTO { DoctorID = 1, FullName = "Dr. A", Specialty = "Cardiology" },
        new DoctorDTO { DoctorID = 2, FullName = "Dr. B", Specialty = "Dermatology" }
    };

            _serviceMock
                .Setup(s => s.GetAllDoctorsAsync())
                .ReturnsAsync(doctors);

            // Act
            var result = await _controller.GetAllDoctors(null);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<DoctorDTO>>(ok.Value);
            Assert.Equal(2, data.Count);
            Assert.Equal("Dr. A", data[0].FullName);

            _serviceMock.VerifyAll();
        }
        [Fact]
        public async Task GetAllDoctors_ServiceThrows_Returns500()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetAllDoctorsAsync())
                .ThrowsAsync(new Exception("Database failure"));

            var ctrl = _controller;

            // Act
            var result = await ctrl.GetAllDoctors(null);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
            Assert.Contains("Có lỗi xảy ra khi lấy danh sách bác sĩ", obj.Value!.ToString());

            _serviceMock.VerifyAll();
        }

        #endregion

        #region CheckDoctorAvailability   [HttpGet("check-conflict")]
        [Fact]
        public async Task CheckDoctorAvailability_NoConflict_ReturnsAvailable()
        {
            // Arrange
            int doctorId = 1;
            int shiftId = 2;
            var from = new DateOnly(2025, 1, 1);
            var to = new DateOnly(2025, 1, 3);

            _serviceMock
                .Setup(s => s.CheckDoctorConflictAsync(doctorId, shiftId, from, to))
                .ReturnsAsync(false); 

            // Act
            var result = await _controller.CheckDoctorAvailability(doctorId, shiftId, from, to);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = ok.Value!;
            var t = value.GetType();

            var isAvailable = (bool)t.GetProperty("isAvailable")!.GetValue(value)!;
            var message = (string)t.GetProperty("message")!.GetValue(value)!;

            Assert.True(isAvailable);
            Assert.Equal("Bác sĩ rảnh trong thời gian này.", message);
            _serviceMock.VerifyAll();
        }
        [Fact]
        public async Task CheckDoctorAvailability_WithConflict_ReturnsNotAvailable()
        {
            // Arrange
            int doctorId = 1;
            int shiftId = 2;
            var from = new DateOnly(2025, 1, 1);
            var to = new DateOnly(2025, 1, 3);

            _serviceMock
                .Setup(s => s.CheckDoctorConflictAsync(doctorId, shiftId, from, to))
                .ReturnsAsync(true); 

            // Act
            var result = await _controller.CheckDoctorAvailability(doctorId, shiftId, from, to);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = ok.Value!;
            var t = value.GetType();

            var isAvailable = (bool)t.GetProperty("isAvailable")!.GetValue(value)!;
            var message = (string)t.GetProperty("message")!.GetValue(value)!;

            Assert.False(isAvailable);
            Assert.Equal("Bác sĩ đã có lịch trùng.", message);
            _serviceMock.VerifyAll();
        }
        [Fact]
        public async Task CheckDoctorAvailability_InvalidIds_ReturnsBadRequest()
        {
            // Arrange
            int doctorId = 0;
            int shiftId = -1;
            var from = new DateOnly(2025, 1, 1);
            var to = new DateOnly(2025, 1, 3);

            // Act
            var result = await _controller.CheckDoctorAvailability(doctorId, shiftId, from, to);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("doctorId và shiftId phải lớn hơn 0", bad.Value!.ToString());

            _serviceMock.Verify(s =>
                    s.CheckDoctorConflictAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()),
                Times.Never);
        }
        [Fact]
        public async Task CheckDoctorAvailability_InvalidDateRange_ReturnsBadRequest()
        {
            // Arrange
            int doctorId = 1;
            int shiftId = 2;
            var from = new DateOnly(2025, 1, 10);
            var to = new DateOnly(2025, 1, 5); // from > to

            // Act
            var result = await _controller.CheckDoctorAvailability(doctorId, shiftId, from, to);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc", bad.Value!.ToString());

            _serviceMock.Verify(s =>
                    s.CheckDoctorConflictAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()),
                Times.Never);
        }

        [Fact]
        public async Task CheckDoctorAvailability_ServiceThrows_Returns500()
        {
            // Arrange
            int doctorId = 1;
            int shiftId = 2;
            var from = new DateOnly(2025, 1, 1);
            var to = new DateOnly(2025, 1, 3);

            _serviceMock
                .Setup(s => s.CheckDoctorConflictAsync(doctorId, shiftId, from, to))
                .ThrowsAsync(new Exception("Database failure"));

            // Act
            var result = await _controller.CheckDoctorAvailability(doctorId, shiftId, from, to);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
            Assert.Contains("Có lỗi xảy ra khi kiểm tra lịch làm việc của bác sĩ", obj.Value!.ToString());

            _serviceMock.VerifyAll();
        }

        #endregion

        #region  CreateSchedule
        [Fact]
        public async Task CreateSchedule_WithDoctors_SendsNotification_AndReturnsOk()
        {
            // Arrange
            var dto = new CreateScheduleRequestDTO
            {
                EffectiveFrom = new DateOnly(2025, 1, 1),
                EffectiveTo = new DateOnly(2025, 1, 7),
                Shifts = new List<ShiftDoctorMap>
        {
            new ShiftDoctorMap { ShiftId = 1, DoctorIds = new List<int> { 1, 2 } },
            new ShiftDoctorMap { ShiftId = 2, DoctorIds = new List<int> { 2, 3 } }
        }
            };

            var expectedDoctorIds = new List<int> { 1, 2, 3 };   // để check Distinct
            var receiveUserIds = new List<int> { 100, 101 };

            _serviceMock
                .Setup(s => s.CreateScheduleAsync(dto))
                .ReturnsAsync(123);

            _doctorScheduleServiceMock
                .Setup(s => s.GetUserIdsByDoctorIdsAsync(
                    It.Is<List<int>>(ids =>
                        ids.OrderBy(x => x).SequenceEqual(expectedDoctorIds)))
                )
                .ReturnsAsync(receiveUserIds);

            CreateNotificationDTO? captured = null;

            _notificationServiceMock
                .Setup(n => n.SendNotificationAsync(It.IsAny<CreateNotificationDTO>()))
                .Callback<CreateNotificationDTO>(n => captured = n)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateSchedule(dto);

            // Assert HTTP result
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Tạo lịch làm việc thành công", ok.Value!.ToString());

            // Verify service calls
            _serviceMock.VerifyAll();
            _doctorScheduleServiceMock.VerifyAll();
            _notificationServiceMock.Verify(
                n => n.SendNotificationAsync(It.IsAny<CreateNotificationDTO>()),
                Times.Once);

            // Assert nội dung notification gửi đi
            Assert.NotNull(captured);
            Assert.Equal("Lịch làm việc mới", captured!.Title);
            Assert.Equal("Schedule", captured.Type);
            Assert.Null(captured.CreatedBy);
            Assert.True(captured.ReceiverIds.SequenceEqual(receiveUserIds));

            // Không lock format ngày: dùng đúng ToString() của DateOnly
            var fromStr = dto.EffectiveFrom.ToString();
            var toStr = dto.EffectiveTo.ToString();

            Assert.Contains(fromStr, captured.Content);
            Assert.Contains(toStr, captured.Content);
        }

        [Fact]
        public async Task CreateSchedule_NoDoctors_DoesNotSendNotification_ReturnsOk()
        {
            // Arrange: Shifts có DoctorIds null / rỗng
            var dto = new CreateScheduleRequestDTO
            {
                EffectiveFrom = new DateOnly(2025, 1, 1),
                EffectiveTo = new DateOnly(2025, 1, 7),
                Shifts = new List<ShiftDoctorMap>
        {
            new ShiftDoctorMap { ShiftId = 1, DoctorIds = null },
            new ShiftDoctorMap { ShiftId = 2, DoctorIds = new List<int>() }
        }
            };

            _serviceMock
                .Setup(s => s.CreateScheduleAsync(dto))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreateSchedule(dto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Tạo lịch làm việc thành công", ok.Value!.ToString());

            _serviceMock.Verify(s => s.CreateScheduleAsync(dto), Times.Once);
            _doctorScheduleServiceMock.Verify(s =>
                s.GetUserIdsByDoctorIdsAsync(It.IsAny<List<int>>()), Times.Never);

            _notificationServiceMock.Verify(s =>
                s.SendNotificationAsync(It.IsAny<CreateNotificationDTO>()), Times.Never);
        }
        [Fact]
        public async Task CreateSchedule_NullDto_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.CreateSchedule(null!);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Dữ liệu lịch làm việc là bắt buộc", bad.Value!.ToString());

            _serviceMock.Verify(s => s.CreateScheduleAsync(It.IsAny<CreateScheduleRequestDTO>()), Times.Never);
            _doctorScheduleServiceMock.VerifyNoOtherCalls();
            _notificationServiceMock.VerifyNoOtherCalls();
        }
        [Fact]
        public async Task CreateSchedule_InvalidDateRange_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateScheduleRequestDTO
            {
                EffectiveFrom = new DateOnly(2025, 1, 10),
                EffectiveTo = new DateOnly(2025, 1, 5),
                Shifts = new List<ShiftDoctorMap>
        {
            new ShiftDoctorMap { ShiftId = 1, DoctorIds = new List<int> { 1 } }
        }
            };

            // Act
            var result = await _controller.CreateSchedule(dto);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc", bad.Value!.ToString());

            _serviceMock.Verify(s => s.CreateScheduleAsync(It.IsAny<CreateScheduleRequestDTO>()), Times.Never);
            _doctorScheduleServiceMock.VerifyNoOtherCalls();
            _notificationServiceMock.VerifyNoOtherCalls();
        }
        [Fact]
        public async Task CreateSchedule_ServiceThrows_Returns500()
        {
            // Arrange
            var dto = new CreateScheduleRequestDTO
            {
                EffectiveFrom = new DateOnly(2025, 1, 1),
                EffectiveTo = new DateOnly(2025, 1, 7),
                Shifts = new List<ShiftDoctorMap>
        {
            new ShiftDoctorMap { ShiftId = 1, DoctorIds = new List<int> { 1 } }
        }
            };

            _serviceMock
                .Setup(s => s.CreateScheduleAsync(dto))
                .ThrowsAsync(new Exception("Database failure"));

            // Act
            var result = await _controller.CreateSchedule(dto);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);


            Assert.Contains("Có lỗi xảy ra khi tạo lịch làm việc", obj.Value!.ToString());

            _serviceMock.VerifyAll();
            // doctor & notification không nên được gọi
            _doctorScheduleServiceMock.VerifyNoOtherCalls();
            _notificationServiceMock.VerifyNoOtherCalls();
        }

        #endregion

        #region listGroupSchedule
        [Fact]
        public async Task GetGroupedWorkScheduleList_ReturnsOk_WithPagedResult()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;

            var groups = new List<WorkScheduleGroupDto>
    {
        new WorkScheduleGroupDto
        {
            EffectiveFrom = new DateOnly(2025, 1, 1),
            EffectiveTo   = new DateOnly(2025, 1, 7),
            Shifts = new List<ShiftResponseDto>
            {
                new ShiftResponseDto
                {
                    ShiftID   = 1,
                    ShiftType = "Morning",
                    StartTime = "08:00",
                    EndTime   = "12:00",
                    Doctors = new List<DoctorDTO>
                    {
                        new DoctorDTO { DoctorID = 1, FullName = "Dr. A", Specialty = "Cardiology" }
                    }
                }
            }
        },
        new WorkScheduleGroupDto
        {
            EffectiveFrom = new DateOnly(2025, 1, 8),
            EffectiveTo   = null,
            Shifts = new List<ShiftResponseDto>
            {
                new ShiftResponseDto
                {
                    ShiftID   = 2,
                    ShiftType = "Afternoon",
                    StartTime = "13:00",
                    EndTime   = "17:00",
                    Doctors = new List<DoctorDTO>
                    {
                        new DoctorDTO { DoctorID = 2, FullName = "Dr. B", Specialty = "Dermatology" }
                    }
                }
            }
        }
    };

            var paged = new PaginationHelper.PagedResult<WorkScheduleGroupDto>
            {
                Items = groups,
                 TotalCount = groups.Count,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _serviceMock
                .Setup(s => s.GetGroupedWorkScheduleListAsync(pageNumber, pageSize))
                .ReturnsAsync(paged);

            // Act
            var result = await _controller.GetGroupedWorkScheduleList(pageNumber, pageSize);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<PaginationHelper.PagedResult<WorkScheduleGroupDto>>(ok.Value);

            Assert.Equal(2, value.TotalCount);
            Assert.Equal(pageNumber, value.PageNumber);
            Assert.Equal(pageSize, value.PageSize);
            Assert.Equal(2, value.Items.Count);
            Assert.Equal("Morning", value.Items[0].Shifts[0].ShiftType);
            Assert.Equal("Dr. A", value.Items[0].Shifts[0].Doctors[0].FullName);

            _serviceMock.VerifyAll();
        }
        [Fact]
        public async Task GetGroupedWorkScheduleList_InvalidPaging_ReturnsBadRequest()
        {
            // Arrange
            int pageNumber = 0;  
            int pageSize = 10;

            // Act
            var result = await _controller.GetGroupedWorkScheduleList(pageNumber, pageSize);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("pageNumber và pageSize phải lớn hơn 0", bad.Value!.ToString());

            _serviceMock.Verify(s =>
                s.GetGroupedWorkScheduleListAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }
        [Fact]
        public async Task GetGroupedWorkScheduleList_ServiceThrows_Returns500()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;

            _serviceMock
                .Setup(s => s.GetGroupedWorkScheduleListAsync(pageNumber, pageSize))
                .ThrowsAsync(new Exception("Database failure"));

            // Act
            var result = await _controller.GetGroupedWorkScheduleList(pageNumber, pageSize);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
            Assert.Contains("Có lỗi xảy ra khi lấy danh sách lịch làm việc nhóm",
                            obj.Value!.ToString());

            _serviceMock.VerifyAll();
        }

        #endregion

        #region  UpdateDoctorShiftsInRange
        [Fact]
        public async Task UpdateDoctorShiftsInRange_NoAddNoRemove_DoesNotSendNotifications_ReturnsOk()
        {
            // Arrange
            var request = new UpdateDoctorShiftRangeRequest
            {
                ShiftId = 1,
                FromDate = new DateOnly(2025, 1, 1),
                ToDate = new DateOnly(2025, 1, 7),
                AddDoctorIds = new List<int>(),
                RemoveDoctorIds = new List<int>()
            };

            _serviceMock
                .Setup(s => s.UpdateDoctorShiftsInRangeAsync(request))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateDoctorShiftsInRange(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Cập nhật lịch làm việc thành công", ok.Value!.ToString());

            _serviceMock.Verify(s => s.UpdateDoctorShiftsInRangeAsync(request), Times.Once);
            _doctorScheduleServiceMock.Verify(
                s => s.GetUserIdsByDoctorIdsAsync(It.IsAny<List<int>>()),
                Times.Never);
            _notificationServiceMock.Verify(
                n => n.SendNotificationAsync(It.IsAny<CreateNotificationDTO>()),
                Times.Never);
        }
        [Fact]
        public async Task UpdateDoctorShiftsInRange_AddDoctors_SendsAddNotification()
        {
            // Arrange
            var request = new UpdateDoctorShiftRangeRequest
            {
                ShiftId = 1,
                FromDate = new DateOnly(2025, 1, 1),
                ToDate = new DateOnly(2025, 1, 7),
                AddDoctorIds = new List<int> { 1, 2 },
                RemoveDoctorIds = new List<int>()
            };

            var expectedAddIds = new List<int> { 1, 2 };
            var receiverUserIds = new List<int> { 100, 101 };

            _serviceMock
                .Setup(s => s.UpdateDoctorShiftsInRangeAsync(request))
                .Returns(Task.CompletedTask);

            _doctorScheduleServiceMock
                .Setup(s => s.GetUserIdsByDoctorIdsAsync(
                    It.Is<List<int>>(ids => ids.OrderBy(x => x).SequenceEqual(expectedAddIds))))
                .ReturnsAsync(receiverUserIds);

            CreateNotificationDTO? capturedAdd = null;

            _notificationServiceMock
                .Setup(n => n.SendNotificationAsync(It.IsAny<CreateNotificationDTO>()))
                .Callback<CreateNotificationDTO>(n => capturedAdd = n)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateDoctorShiftsInRange(request);

            // Assert HTTP
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Cập nhật lịch làm việc thành công", ok.Value!.ToString());

            // Verify service flow
            _serviceMock.Verify(s => s.UpdateDoctorShiftsInRangeAsync(request), Times.Once);
            _doctorScheduleServiceMock.Verify(
                s => s.GetUserIdsByDoctorIdsAsync(
                    It.Is<List<int>>(ids => ids.OrderBy(x => x).SequenceEqual(expectedAddIds))),
                Times.Once);
            _notificationServiceMock.Verify(
                n => n.SendNotificationAsync(It.IsAny<CreateNotificationDTO>()),
                Times.Once);

            // Assert nội dung notification
            Assert.NotNull(capturedAdd);
            Assert.Equal("Lịch làm việc mới", capturedAdd!.Title);
            Assert.Equal("schedule", capturedAdd.Type);
            Assert.Null(capturedAdd.CreatedBy);
            Assert.True(capturedAdd.ReceiverIds.SequenceEqual(receiverUserIds));

            var fromStr = request.FromDate.ToString("dd/MM/yyyy");
            var toStr = request.ToDate.ToString("dd/MM/yyyy");
            Assert.Contains(fromStr, capturedAdd.Content);
            Assert.Contains(toStr, capturedAdd.Content);
        }
        [Fact]
        public async Task UpdateDoctorShiftsInRange_AddDoctors_NoReceivers_DoesNotSendNotification()
        {
            // Arrange
            var request = new UpdateDoctorShiftRangeRequest
            {
                ShiftId = 1,
                FromDate = new DateOnly(2025, 1, 1),
                ToDate = new DateOnly(2025, 1, 7),
                AddDoctorIds = new List<int> { 1, 2 },
                RemoveDoctorIds = new List<int>()
            };

            var expectedAddIds = new List<int> { 1, 2 };

            _serviceMock
                .Setup(s => s.UpdateDoctorShiftsInRangeAsync(request))
                .Returns(Task.CompletedTask);

            // Trả về list rỗng => receiversAdd.Any() == false
            _doctorScheduleServiceMock
                .Setup(s => s.GetUserIdsByDoctorIdsAsync(
                    It.Is<List<int>>(ids => ids.OrderBy(x => x).SequenceEqual(expectedAddIds))))
                .ReturnsAsync(new List<int>());

            // Act
            var result = await _controller.UpdateDoctorShiftsInRange(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Cập nhật lịch làm việc thành công", ok.Value!.ToString());

            _serviceMock.Verify(s => s.UpdateDoctorShiftsInRangeAsync(request), Times.Once);
            _doctorScheduleServiceMock.Verify(
                s => s.GetUserIdsByDoctorIdsAsync(
                    It.Is<List<int>>(ids => ids.OrderBy(x => x).SequenceEqual(expectedAddIds))),
                Times.Once);
            _notificationServiceMock.Verify(
                n => n.SendNotificationAsync(It.IsAny<CreateNotificationDTO>()),
                Times.Never);
        }
        [Fact]
        public async Task UpdateDoctorShiftsInRange_RemoveDoctors_SendsRemoveNotification()
        {
            // Arrange
            var request = new UpdateDoctorShiftRangeRequest
            {
                ShiftId = 2,
                FromDate = new DateOnly(2025, 2, 1),
                ToDate = new DateOnly(2025, 2, 5),
                AddDoctorIds = new List<int>(),
                RemoveDoctorIds = new List<int> { 3, 4 }
            };

            var expectedRemoveIds = new List<int> { 3, 4 };
            var receiverUserIds = new List<int> { 200, 201 };

            _serviceMock
                .Setup(s => s.UpdateDoctorShiftsInRangeAsync(request))
                .Returns(Task.CompletedTask);

            _doctorScheduleServiceMock
                .Setup(s => s.GetUserIdsByDoctorIdsAsync(
                    It.Is<List<int>>(ids => ids.OrderBy(x => x).SequenceEqual(expectedRemoveIds))))
                .ReturnsAsync(receiverUserIds);

            CreateNotificationDTO? capturedRemove = null;

            _notificationServiceMock
                .Setup(n => n.SendNotificationAsync(It.IsAny<CreateNotificationDTO>()))
                .Callback<CreateNotificationDTO>(n => capturedRemove = n)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateDoctorShiftsInRange(request);

            // Assert HTTP
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Cập nhật lịch làm việc thành công", ok.Value!.ToString());

            // Verify service flow
            _serviceMock.Verify(s => s.UpdateDoctorShiftsInRangeAsync(request), Times.Once);
            _doctorScheduleServiceMock.Verify(
                s => s.GetUserIdsByDoctorIdsAsync(
                    It.Is<List<int>>(ids => ids.OrderBy(x => x).SequenceEqual(expectedRemoveIds))),
                Times.Once);
            _notificationServiceMock.Verify(
                n => n.SendNotificationAsync(It.IsAny<CreateNotificationDTO>()),
                Times.Once);

            // Assert nội dung notification
            Assert.NotNull(capturedRemove);
            Assert.Equal("Lịch làm việc thay đổi", capturedRemove!.Title);
            Assert.Equal("schedule", capturedRemove.Type);
            Assert.Null(capturedRemove.CreatedBy);
            Assert.True(capturedRemove.ReceiverIds.SequenceEqual(receiverUserIds));

            var fromStr = request.FromDate.ToString("dd/MM/yyyy");
            var toStr = request.ToDate.ToString("dd/MM/yyyy");
            Assert.Contains(fromStr, capturedRemove.Content);
            Assert.Contains(toStr, capturedRemove.Content);
        }
        [Fact]
        public async Task UpdateDoctorShiftsInRange_ServiceThrows_ReturnsBadRequest()
        {
            // Arrange
            var inner = new Exception("Inner error");
            var ex = new Exception("Update failed", inner);

            var request = new UpdateDoctorShiftRangeRequest
            {
                ShiftId = 1,
                FromDate = new DateOnly(2025, 1, 1),
                ToDate = new DateOnly(2025, 1, 7),
                AddDoctorIds = new List<int> { 1 },
                RemoveDoctorIds = new List<int> { 2 }
            };

            _serviceMock
                .Setup(s => s.UpdateDoctorShiftsInRangeAsync(request))
                .ThrowsAsync(ex);

            // Act
            var result = await _controller.UpdateDoctorShiftsInRange(request);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var valueStr = bad.Value!.ToString()!;

            Assert.Contains("Update failed", valueStr);
            Assert.Contains("Inner error", valueStr);

            _serviceMock.Verify(s => s.UpdateDoctorShiftsInRangeAsync(request), Times.Once);
            _doctorScheduleServiceMock.VerifyNoOtherCalls();
            _notificationServiceMock.VerifyNoOtherCalls();
        }


        #endregion

        #region CheckDoctorShiftLimit

        [Fact]
        public async Task CheckDoctorShiftLimit_DoctorIdLessOrEqualZero_ReturnsFalse_AndDoesNotCallService()
        {
            // Arrange
            int doctorId = 0;
            var date = new DateOnly(2025, 1, 1);

            // Act
            var result = await _controller.CheckDoctorShiftLimit(doctorId, date);

            // Assert
            Assert.False(result);

            _serviceMock.Verify(
                s => s.CheckDoctorShiftLimitAsync(It.IsAny<int>(), It.IsAny<DateOnly>()),
                Times.Never);
        }
        [Fact]
        public async Task CheckDoctorShiftLimit_WhenServiceReturnsTrue_ReturnsTrue()
        {
            // Arrange
            int doctorId = 5;
            var date = new DateOnly(2025, 1, 1);

            _serviceMock
                .Setup(s => s.CheckDoctorShiftLimitAsync(doctorId, date))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.CheckDoctorShiftLimit(doctorId, date);

            // Assert
            Assert.True(result);

            _serviceMock.Verify(
                s => s.CheckDoctorShiftLimitAsync(doctorId, date),
                Times.Once);
        }
        [Fact]
        public async Task CheckDoctorShiftLimit_WhenServiceReturnsFalse_ReturnsFalse()
        {
            // Arrange
            int doctorId = 5;
            var date = new DateOnly(2025, 1, 1);

            _serviceMock
                .Setup(s => s.CheckDoctorShiftLimitAsync(doctorId, date))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.CheckDoctorShiftLimit(doctorId, date);

            // Assert
            Assert.False(result);

            _serviceMock.Verify(
                s => s.CheckDoctorShiftLimitAsync(doctorId, date),
                Times.Once);
        }

        #endregion

        #region CheckDoctorShiftLimitRange
        [Fact]
        public async Task CheckDoctorShiftLimitRange_ServiceReturnsTrue_ReturnsOkTrue()
        {
            // Arrange
            int doctorId = 5;
            var from = new DateOnly(2025, 1, 1);
            var to = new DateOnly(2025, 1, 7);

            _serviceMock
                .Setup(s => s.CheckDoctorShiftLimitRangeAsync(doctorId, from, to))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.CheckDoctorShiftLimitRange(doctorId, from, to);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<bool>(ok.Value);
            Assert.True(value);

            _serviceMock.Verify(
                s => s.CheckDoctorShiftLimitRangeAsync(doctorId, from, to),
                Times.Once);
        }
        [Fact]
        public async Task CheckDoctorShiftLimitRange_ServiceReturnsFalse_ReturnsOkFalse()
        {
            // Arrange
            int doctorId = 5;
            var from = new DateOnly(2025, 1, 1);
            var to = new DateOnly(2025, 1, 7);

            _serviceMock
                .Setup(s => s.CheckDoctorShiftLimitRangeAsync(doctorId, from, to))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.CheckDoctorShiftLimitRange(doctorId, from, to);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<bool>(ok.Value);
            Assert.False(value);

            _serviceMock.Verify(
                s => s.CheckDoctorShiftLimitRangeAsync(doctorId, from, to),
                Times.Once);
        }
        [Fact]
        public async Task CheckDoctorShiftLimitRange_ServiceThrows_ReturnsBadRequest()
        {
            // Arrange
            int doctorId = 5;
            var from = new DateOnly(2025, 1, 1);
            var to = new DateOnly(2025, 1, 7);

            _serviceMock
                .Setup(s => s.CheckDoctorShiftLimitRangeAsync(doctorId, from, to))
                .ThrowsAsync(new Exception("Something went wrong"));

            // Act
            var result = await _controller.CheckDoctorShiftLimitRange(doctorId, from, to);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Something went wrong", bad.Value!.ToString());

            _serviceMock.Verify(
                s => s.CheckDoctorShiftLimitRangeAsync(doctorId, from, to),
                Times.Once);
        }


        #endregion

        #region GetDoctorsWithoutSchedule
        [Fact]
        public async Task GetDoctorsWithoutSchedule_ReturnsOk_WithDoctorList()
        {
            // Arrange
            var start = new DateOnly(2025, 1, 1);
            var end = new DateOnly(2025, 1, 7);

            var doctors = new List<DoctorDTO>
    {
        new DoctorDTO { DoctorID = 1, FullName = "Dr. A", Specialty = "Cardiology" },
        new DoctorDTO { DoctorID = 2, FullName = "Dr. B", Specialty = "Dermatology" }
    };

            _serviceMock
                .Setup(s => s.GetDoctorsWithoutScheduleAsync(start, end))
                .ReturnsAsync(doctors);

            // Act
            var result = await _controller.GetDoctorsWithoutSchedule(start, end);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<DoctorDTO>>(ok.Value);
            Assert.Equal(2, value.Count);
            Assert.Equal("Dr. A", value[0].FullName);

            _serviceMock.Verify(
                s => s.GetDoctorsWithoutScheduleAsync(start, end),
                Times.Once);
        }
        [Fact]
        public async Task GetDoctorsWithoutSchedule_InvalidDateRange_ReturnsBadRequest()
        {
            // Arrange
            var start = new DateOnly(2025, 1, 10);
            var end = new DateOnly(2025, 1, 5); // start > end

            // Act
            var result = await _controller.GetDoctorsWithoutSchedule(start, end);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc",
                            bad.Value!.ToString());

            _serviceMock.Verify(
                s => s.GetDoctorsWithoutScheduleAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()),
                Times.Never);
        }
        [Fact]
        public async Task GetDoctorsWithoutSchedule_ServiceThrows_Returns500()
        {
            // Arrange
            var start = new DateOnly(2025, 1, 1);
            var end = new DateOnly(2025, 1, 7);

            _serviceMock
                .Setup(s => s.GetDoctorsWithoutScheduleAsync(start, end))
                .ThrowsAsync(new Exception("Database failure"));

            // Act
            var result = await _controller.GetDoctorsWithoutSchedule(start, end);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
            Assert.Contains("Có lỗi xảy ra khi lấy danh sách bác sĩ chưa có lịch làm việc",
                            obj.Value!.ToString());

            _serviceMock.Verify(
                s => s.GetDoctorsWithoutScheduleAsync(start, end),
                Times.Once);
        }

        #endregion

        #region GetAllDoctors2
        [Fact]
        public async Task GetAllDoctors2_ReturnsOk_WithDoctorList()
        {
            // Arrange
            var doctors = new List<DoctorHomeDTO>
            {
                new DoctorHomeDTO { DoctorID = 1, FullName = "Dr. A", Specialty = "Cardiology" },
                new DoctorHomeDTO { DoctorID = 2, FullName = "Dr. B", Specialty = "Dermatology" }
            };

            _serviceMock
                .Setup(s => s.GetAllDoctors2Async())
                .ReturnsAsync(doctors);

            // Act
            var result = await _controller.GetAllDoctors2(null);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<DoctorHomeDTO>>(ok.Value);
            Assert.Equal(2, data.Count);
            Assert.Equal("Dr. A", data[0].FullName);

            _serviceMock.VerifyAll();
        }

        #endregion

        #region GetAll (get-all-doctor-schedule)
        [Fact]
        public async Task GetAll_WithValidDateRange_ReturnsOk()
        {
            // Arrange
            var startDate = new DateOnly(2025, 1, 1);
            var endDate = new DateOnly(2025, 1, 31);
            var schedules = new List<DoctorActiveScheduleRangeDto>
            {
                new DoctorActiveScheduleRangeDto
                {
                    DoctorId = 1,
                    DoctorName = "Dr. A",
                    Specialty = "Cardiology",
                    RoomName = "Room 1",
                    Date = startDate,
                    ShiftType = "Morning",
                    StartTime = new TimeOnly(8, 0),
                    EndTime = new TimeOnly(12, 0)
                }
            };

            _doctorScheduleServiceMock
                .Setup(s => s.GetAllDoctorSchedulesByRangeAsync(startDate, endDate))
                .ReturnsAsync(schedules);

            // Act
            var result = await _controller.GetAll(startDate, endDate);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<DoctorActiveScheduleRangeDto>>(ok.Value);
            Assert.Single(data);
            _doctorScheduleServiceMock.VerifyAll();
        }

        #endregion

        #region GetByRange (getScheduleByRange)
        [Fact]
        public async Task GetByRange_WithValidDateRange_ReturnsOk()
        {
            // Arrange
            var start = new DateOnly(2025, 1, 1);
            var end = new DateOnly(2025, 1, 31);
            var schedules = new List<DailyWorkScheduleViewDto>
            {
                new DailyWorkScheduleViewDto
                {
                    Date = start,
                    Shifts = new List<ShiftResponseDto>()
                }
            };

            _serviceMock
                .Setup(s => s.GetWorkScheduleByDateRangeAsync(start, end))
                .ReturnsAsync(schedules);

            // Act
            var result = await _controller.GetByRange(start, end);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<List<DailyWorkScheduleViewDto>>(ok.Value);
            Assert.Single(data);
            _serviceMock.VerifyAll();
        }

        #endregion

        #region GetByDate (getScheduleByDate)
        [Fact]
        public async Task GetByDate_WithValidDate_ReturnsOk()
        {
            // Arrange
            var date = new DateOnly(2025, 1, 15);
            var pageNumber = 1;
            var pageSize = 10;
            var pagedResult = new PaginationHelper.PagedResult<DailyWorkScheduleDto>
            {
                Items = new List<DailyWorkScheduleDto>
                {
                    new DailyWorkScheduleDto
                    {
                        Date = date,
                        Shifts = new List<WorkScheduleDto>()
                    }
                },
                TotalCount = 1,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _serviceMock
                .Setup(s => s.GetWorkSchedulesByDateAsync(date, pageNumber, pageSize))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetByDate(date, pageNumber, pageSize);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<PaginationHelper.PagedResult<DailyWorkScheduleDto>>(ok.Value);
            Assert.Equal(1, data.TotalCount);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task GetByDate_WithNullDate_ReturnsOk()
        {
            // Arrange
            DateOnly? date = null;
            var pageNumber = 1;
            var pageSize = 10;
            var pagedResult = new PaginationHelper.PagedResult<DailyWorkScheduleDto>
            {
                Items = new List<DailyWorkScheduleDto>(),
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _serviceMock
                .Setup(s => s.GetWorkSchedulesByDateAsync(date, pageNumber, pageSize))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetByDate(date, pageNumber, pageSize);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<PaginationHelper.PagedResult<DailyWorkScheduleDto>>(ok.Value);
            Assert.Equal(0, data.TotalCount);
            _serviceMock.VerifyAll();
        }

        #endregion

        #region GetMonthlyWorkSummary
        [Fact]
        public async Task GetMonthlyWorkSummary_WithValidYearMonth_ReturnsOk()
        {
            // Arrange
            var year = 2025;
            var month = 1;
            var summaries = new List<DailySummaryDto>
            {
                new DailySummaryDto
                {
                    Date = new DateOnly(2025, 1, 1),
                    ShiftCount = 5,
                    DoctorCount = 3
                }
            };

            _serviceMock
                .Setup(s => s.GetMonthlyWorkSummaryAsync(year, month))
                .ReturnsAsync(summaries);

            // Act
            var result = await _controller.GetMonthlyWorkSummary(year, month);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<List<DailySummaryDto>>(ok.Value);
            Assert.Single(data);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task GetMonthlyWorkSummary_WithException_ReturnsBadRequest()
        {
            // Arrange
            var year = 2025;
            var month = 13; // Invalid month

            _serviceMock
                .Setup(s => s.GetMonthlyWorkSummaryAsync(year, month))
                .ThrowsAsync(new ArgumentException("Tháng hoặc năm không hợp lệ"));

            // Act
            var result = await _controller.GetMonthlyWorkSummary(year, month);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Tháng hoặc năm không hợp lệ", badRequest.Value!.ToString());
            _serviceMock.VerifyAll();
        }

        #endregion
    }

}
