using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers.ManagerControllers;
using SEP490_BE.BLL.IServices.IDoctorServices;
using SEP490_BE.BLL.IServices.IManagerService;
using SEP490_BE.BLL.IServices.IManagerServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.DTOs.ManagerDTO.ManagerSchedule;
using SEP490_BE.DAL.DTOs.MedicineDTO;
using SEP490_BE.DAL.Helpers;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SEP490_BE.Tests.Controllers
{
    public class ManagerControllerTests
    {
        private readonly Mock<IScheduleService> _serviceMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<IDoctorScheduleService> _doctorScheduleServiceMock;
        private readonly ManageScheduleController _controller;

        public ManagerControllerTests()
        {
            _serviceMock = new Mock<IScheduleService>();
            _notificationServiceMock = new Mock<INotificationService>();
            _doctorScheduleServiceMock = new Mock<IDoctorScheduleService>();
            _controller = new ManageScheduleController(
                _serviceMock.Object,
                _notificationServiceMock.Object,
                _doctorScheduleServiceMock.Object
            );
        }

        [Fact]
        public async Task GetAllShifts_ShouldReturnOk()
        {
            _serviceMock.Setup(s => s.GetAllShiftsAsync())
                .ReturnsAsync(new List<ShiftResponseDTO> { new ShiftResponseDTO { ShiftID = 1, ShiftType = "Sáng" } });

            var result = await _controller.GetAllShifts();

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<ShiftResponseDTO>>(ok.Value);
            Assert.Single(data);
        }

        [Fact]
        public async Task GetAllDoctors_ShouldReturnOk()
        {
            _serviceMock.Setup(s => s.GetAllDoctorsAsync())
                .ReturnsAsync(new List<DoctorDTO> { new DoctorDTO { DoctorID = 1, FullName = "BS A" } });

            var result = await _controller.GetAllDoctors(null);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<DoctorDTO>>(ok.Value);
            Assert.Equal("BS A", data[0].FullName);
        }

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

        [Fact]
        public async Task CheckDoctorAvailability_ShouldReturnAvailableMessage()
        {
            _serviceMock.Setup(s => s.CheckDoctorConflictAsync(1, 1, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(false);

            var result = await _controller.CheckDoctorAvailability(1, 1, new DateOnly(2025, 10, 1), new DateOnly(2025, 10, 2));

            var ok = Assert.IsType<OkObjectResult>(result);
            var msg = ok.Value!.ToString();
            Assert.Contains("rảnh", msg);
        }

        [Fact]
        public async Task CreateSchedule_ShouldReturnOkWithMessage()
        {
            var dto = new CreateScheduleRequestDTO();
            _serviceMock.Setup(s => s.CreateScheduleAsync(dto))
                .ReturnsAsync(3);

            var result = await _controller.CreateSchedule(dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Tạo thành công", ok.Value!.ToString());
        }

        [Fact]
        public async Task GetSchedules_ShouldReturnFormattedData()
        {
            _serviceMock.Setup(s => s.GetSchedulesAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<DoctorShift>
                {
                    new DoctorShift
                    {
                        DoctorShiftId = 1,
                        Status = "Active",
                        EffectiveFrom = new DateOnly(2025,10,1),
                        Doctor = new Doctor { User = new User { FullName = "BS A" } },
                        Shift = new Shift { ShiftType = "Sáng", StartTime = new TimeOnly(7,0), EndTime = new TimeOnly(11,0) }
                    }
                });

            var result = await _controller.GetSchedules(new DateOnly(2025, 10, 1), new DateOnly(2025, 10, 2));

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("BS A", ok.Value!.ToString());
        }

        [Fact]
        public async Task GetAllSchedule_ShouldReturnPagedResult()
        {
            var paged = new PaginationHelper.PagedResult<WorkScheduleDto>
            {
                Items = new List<WorkScheduleDto> { new WorkScheduleDto { DoctorId = 1 } }
            };

            _serviceMock.Setup(s => s.GetAllSchedulesAsync(1, 10))
                .ReturnsAsync(paged);

            var result = await _controller.GetAllSchedule(1, 10);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<PaginationHelper.PagedResult<WorkScheduleDto>>(ok.Value);
        }

        [Fact]
        public async Task GetByRange_ShouldReturnOk()
        {
            _serviceMock.Setup(s => s.GetWorkScheduleByDateRangeAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<DailyWorkScheduleViewDto>());

            var result = await _controller.GetByRange(new DateOnly(2025, 10, 1), new DateOnly(2025, 10, 31));

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetByDate_ShouldReturnOk()
        {
            _serviceMock.Setup(s => s.GetWorkSchedulesByDateAsync(It.IsAny<DateOnly?>(), 1, 10))
                .ReturnsAsync(new PaginationHelper.PagedResult<DailyWorkScheduleDto>());

            var result = await _controller.GetByDate(new DateOnly(2025, 10, 1), 1, 10);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetMonthlyWorkSummary_ShouldReturnOk()
        {
            _serviceMock.Setup(s => s.GetMonthlyWorkSummaryAsync(2025, 10))
                .ReturnsAsync(new List<DailySummaryDto>());

            var result = await _controller.GetMonthlyWorkSummary(2025, 10);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateByDate_ShouldReturnOk_WhenSuccess()
        {
            _serviceMock.Setup(s => s.UpdateWorkScheduleByDateAsync(It.IsAny<UpdateWorkScheduleByDateRequest>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.UpdateByDate(new UpdateWorkScheduleByDateRequest());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("thành công", ok.Value!.ToString());
        }

        [Fact]
        public async Task UpdateByDate_ShouldReturnBadRequest_WhenException()
        {
            _serviceMock.Setup(s => s.UpdateWorkScheduleByDateAsync(It.IsAny<UpdateWorkScheduleByDateRequest>()))
                .ThrowsAsync(new Exception("Lỗi test"));

            var result = await _controller.UpdateByDate(new UpdateWorkScheduleByDateRequest());

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Lỗi test", bad.Value!.ToString());
        }

        [Fact]
        public async Task UpdateById_ShouldReturnOk()
        {
            _serviceMock.Setup(s => s.UpdateWorkScheduleByIdAsync(It.IsAny<UpdateWorkScheduleByIdRequest>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.UpdateById(new UpdateWorkScheduleByIdRequest());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Cập nhật lịch thành công", ok.Value!.ToString());
        }

        //[Fact]
        //public async Task GetGroupedWorkScheduleList_ShouldReturnOk()
        //{
        //    _serviceMock.Setup(s => s.GetGroupedWorkScheduleListAsync(1, 10))
        //        .ReturnsAsync(new List<object>());

        //    var result = await _controller.GetGroupedWorkScheduleList(1, 10);

        //    Assert.IsType<OkObjectResult>(result);
        //}

        [Fact]
        public async Task UpdateDoctorShiftsInRange_ShouldReturnOk()
        {
            _serviceMock.Setup(s => s.UpdateDoctorShiftsInRangeAsync(It.IsAny<UpdateDoctorShiftRangeRequest>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.UpdateDoctorShiftsInRange(new UpdateDoctorShiftRangeRequest());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateDoctorShiftsInRange_ShouldReturnBadRequest_WhenError()
        {
            _serviceMock.Setup(s => s.UpdateDoctorShiftsInRangeAsync(It.IsAny<UpdateDoctorShiftRangeRequest>()))
                .ThrowsAsync(new Exception("Error Test"));

            var result = await _controller.UpdateDoctorShiftsInRange(new UpdateDoctorShiftRangeRequest());

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
