using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers.ManagerControllers;
using SEP490_BE.BLL.IServices.IManagerServices;
using SEP490_BE.DAL.Helpers;
using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.DTOs.ManagerDTO.ManagerSchedule;
using SEP490_BE.DAL.DTOs;

namespace SEP490_BE.Tests.Controllers
{
	public class ManageScheduleControllerTests
	{
		private readonly Mock<IScheduleService> _svc = new(MockBehavior.Strict);

		private ManageScheduleController NewController()
		{
			return new ManageScheduleController(_svc.Object);
		}

		[Fact]
		public async Task GetAllShifts_ReturnsOkWithData()
		{
			var shifts = new List<ShiftResponseDTO> { new ShiftResponseDTO { ShiftID = 1, ShiftType = "Morning" } };
			_svc.Setup(s => s.GetAllShiftsAsync()).ReturnsAsync(shifts);

			var ctrl = NewController();
			var result = await ctrl.GetAllShifts();
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Same(shifts, ok.Value);
			_svc.VerifyAll();
		}

		[Fact]
		public async Task GetAllDoctors_ReturnsOkWithData_IgnoresKeyword()
		{
			var doctors = new List<DoctorDTO> { new DoctorDTO { DoctorID = 1, FullName = "Dr A" } };
			_svc.Setup(s => s.GetAllDoctorsAsync()).ReturnsAsync(doctors);

			var ctrl = NewController();
			var result = await ctrl.GetAllDoctors("abc");
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Same(doctors, ok.Value);
			_svc.VerifyAll();
		}

		[Fact]
		public async Task SearchDoctors_NoKeyword_ReturnsAll()
		{
			var doctors = new List<DoctorDTO> { new DoctorDTO { DoctorID = 2, FullName = "Dr B" } };
			_svc.Setup(s => s.GetAllDoctorsAsync()).ReturnsAsync(doctors);

			var ctrl = NewController();
			var result = await ctrl.SearchDoctors(null);
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Same(doctors, ok.Value);
			_svc.VerifyAll();
		}

		[Fact]
		public async Task SearchDoctors_WithKeyword_ReturnsFiltered()
		{
			var filtered = new List<DoctorDTO> { new DoctorDTO { DoctorID = 3, FullName = "Dr C" } };
			_svc.Setup(s => s.SearchDoctorsAsync("c")).ReturnsAsync(filtered);

			var ctrl = NewController();
			var result = await ctrl.SearchDoctors("c");
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Same(filtered, ok.Value);
			_svc.VerifyAll();
		}

		[Fact]
		public async Task CheckDoctorAvailability_NoConflict_ReturnsAvailable()
		{
			_svc.Setup(s => s.CheckDoctorConflictAsync(1, 2, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
				.ReturnsAsync(false);
			var ctrl = NewController();
			var result = await ctrl.CheckDoctorAvailability(1, 2, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 2));
			var ok = Assert.IsType<OkObjectResult>(result);
			var obj = ok.Value!;
			var isAvailable = (bool)obj.GetType().GetProperty("isAvailable")!.GetValue(obj)!;
			var message = (string)obj.GetType().GetProperty("message")!.GetValue(obj)!;
			Assert.True(isAvailable);
			Assert.Contains("rảnh", message);
			_svc.VerifyAll();
		}

		[Fact]
		public async Task CheckDoctorAvailability_Conflict_ReturnsNotAvailable()
		{
			_svc.Setup(s => s.CheckDoctorConflictAsync(5, 7, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
				.ReturnsAsync(true);
			var ctrl = NewController();
			var result = await ctrl.CheckDoctorAvailability(5, 7, new DateOnly(2025, 2, 1), new DateOnly(2025, 2, 3));
			var ok = Assert.IsType<OkObjectResult>(result);
			var obj = ok.Value!;
			var isAvailable = (bool)obj.GetType().GetProperty("isAvailable")!.GetValue(obj)!;
			var message = (string)obj.GetType().GetProperty("message")!.GetValue(obj)!;
			Assert.False(isAvailable);
			Assert.Contains("trùng", message);
			_svc.VerifyAll();
		}

		[Fact]
		public async Task CreateSchedule_ReturnsOkWithCount()
		{
			var dto = new CreateScheduleRequestDTO();
			_svc.Setup(s => s.CreateScheduleAsync(dto)).ReturnsAsync(3);
			var ctrl = NewController();
			var result = await ctrl.CreateSchedule(dto);
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Contains("3", ok.Value!.ToString());
			_svc.VerifyAll();
		}

		[Fact]
		public async Task GetSchedules_ProjectsDataCorrectly()
		{
			var ds = new DoctorShift
			{
				DoctorShiftId = 10,
				EffectiveFrom = new DateOnly(2025, 3, 15),
				Status = "Active",
				Doctor = new Doctor { User = new User { FullName = "Dr House", Phone = "", PasswordHash = "", RoleId = 1, IsActive = true } },
				Shift = new Shift { ShiftType = "Evening", StartTime = new TimeOnly(18, 0), EndTime = new TimeOnly(22, 0) }
			};
			_svc.Setup(s => s.GetSchedulesAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
				.ReturnsAsync(new List<DoctorShift> { ds });

			var ctrl = NewController();
			var result = await ctrl.GetSchedules(new DateOnly(2025, 3, 1), new DateOnly(2025, 3, 31));
			var ok = Assert.IsType<OkObjectResult>(result);
			var enumerable = Assert.IsAssignableFrom<IEnumerable<object>>(ok.Value!);
			var first = enumerable.First();
			Assert.Equal(10, first.GetType().GetProperty("DoctorShiftId")!.GetValue(first));
			Assert.Equal("Dr House", first.GetType().GetProperty("Doctor")!.GetValue(first));
			Assert.Equal("Evening", first.GetType().GetProperty("Shift")!.GetValue(first));
			Assert.Equal("18:00 - 22:00", first.GetType().GetProperty("Time")!.GetValue(first));
			Assert.Equal("2025-03-15", first.GetType().GetProperty("Date")!.GetValue(first));
			Assert.Equal("Active", first.GetType().GetProperty("Status")!.GetValue(first));
			_svc.VerifyAll();
		}

		[Fact]
		public async Task GetAllSchedule_ReturnsOkWithPagedResult()
		{
			var paged = new PaginationHelper.PagedResult<WorkScheduleDto> { Items = new List<WorkScheduleDto>(), PageNumber = 1, PageSize = 10, TotalCount = 0 };
			_svc.Setup(s => s.GetAllSchedulesAsync(1, 10)).ReturnsAsync(paged);

			var ctrl = NewController();
			var result = await ctrl.GetAllSchedule(1, 10);
			var ok = Assert.IsType<OkObjectResult>(result.Result);
			Assert.Same(paged, ok.Value);
			_svc.VerifyAll();
		}

		[Fact]
		public async Task GetByRange_ReturnsOk()
		{
			var list = new List<DailyWorkScheduleViewDto>();
			_svc.Setup(s => s.GetWorkScheduleByDateRangeAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).ReturnsAsync(list);
			var ctrl = NewController();
			var result = await ctrl.GetByRange(new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31));
			var ok = Assert.IsType<OkObjectResult>(result.Result);
			Assert.Same(list, ok.Value);
			_svc.VerifyAll();
		}

		[Fact]
		public async Task GetByDate_ReturnsOk()
		{
			var paged = new PaginationHelper.PagedResult<DailyWorkScheduleDto> { Items = new List<DailyWorkScheduleDto>(), PageNumber = 2, PageSize = 5, TotalCount = 0 };
			_svc.Setup(s => s.GetWorkSchedulesByDateAsync(new DateOnly(2025, 4, 1), 2, 5)).ReturnsAsync(paged);
			var ctrl = NewController();
			var result = await ctrl.GetByDate(new DateOnly(2025, 4, 1), 2, 5);
			var ok = Assert.IsType<OkObjectResult>(result.Result);
			Assert.Same(paged, ok.Value);
			_svc.VerifyAll();
		}

		[Fact]
		public async Task GetMonthlyWorkSummary_Success_ReturnsOk()
		{
			var list = new List<DailySummaryDto> { new DailySummaryDto() };
			_svc.Setup(s => s.GetMonthlyWorkSummaryAsync(2025, 5)).ReturnsAsync(list);
			var ctrl = NewController();
			var result = await ctrl.GetMonthlyWorkSummary(2025, 5);
			var ok = Assert.IsType<OkObjectResult>(result.Result);
			Assert.Same(list, ok.Value);
			_svc.VerifyAll();
		}

		[Fact]
		public async Task GetMonthlyWorkSummary_Error_ReturnsBadRequest()
		{
			_svc.Setup(s => s.GetMonthlyWorkSummaryAsync(2025, 6)).ThrowsAsync(new InvalidOperationException("boom"));
			var ctrl = NewController();
			var result = await ctrl.GetMonthlyWorkSummary(2025, 6);
			var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
			Assert.Contains("boom", bad.Value!.ToString());
			_svc.VerifyAll();
		}

		[Fact]
		public async Task UpdateByDate_Success_ReturnsOk()
		{
			var req = new UpdateWorkScheduleByDateRequest();
			_svc.Setup(s => s.UpdateWorkScheduleByDateAsync(req)).Returns(Task.CompletedTask);
			var ctrl = NewController();
			var result = await ctrl.UpdateByDate(req);
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Contains("thành công", ok.Value!.ToString());
			_svc.VerifyAll();
		}

		[Fact]
		public async Task UpdateByDate_Error_ReturnsBadRequest()
		{
			var req = new UpdateWorkScheduleByDateRequest();
			_svc.Setup(s => s.UpdateWorkScheduleByDateAsync(req)).ThrowsAsync(new Exception("e1"));
			var ctrl = NewController();
			var result = await ctrl.UpdateByDate(req);
			var bad = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Contains("e1", bad.Value!.ToString());
			_svc.VerifyAll();
		}

		[Fact]
		public async Task UpdateById_Success_ReturnsOk()
		{
			var req = new UpdateWorkScheduleByIdRequest();
			_svc.Setup(s => s.UpdateWorkScheduleByIdAsync(req)).Returns(Task.CompletedTask);
			var ctrl = NewController();
			var result = await ctrl.UpdateById(req);
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Contains("Cập nhật lịch thành công", ok.Value!.ToString());
			_svc.VerifyAll();
		}
        [Fact]
        public async Task UpdateById_Error_ReturnsBadRequest()
        {
            var req = new UpdateWorkScheduleByIdRequest();
            _svc.Setup(s => s.UpdateWorkScheduleByIdAsync(req))
                .ThrowsAsync(new Exception("id-error"));

            var ctrl = NewController();
            var result = await ctrl.UpdateById(req);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("id-error", bad.Value!.ToString());
            _svc.VerifyAll();
        }


        [Fact]
		public async Task GetGroupedWorkScheduleList_ReturnsOk()
		{
			var paged = new PaginationHelper.PagedResult<WorkScheduleGroupDto> { Items = new List<WorkScheduleGroupDto>(), PageNumber = 1, PageSize = 10, TotalCount = 0 };
			_svc.Setup(s => s.GetGroupedWorkScheduleListAsync(1, 10)).ReturnsAsync(paged);
			var ctrl = NewController();
			var result = await ctrl.GetGroupedWorkScheduleList(1, 10);
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Same(paged, ok.Value);
			_svc.VerifyAll();
		}

		[Fact]
		public async Task UpdateDoctorShiftsInRange_Success_ReturnsOk()
		{
			var req = new UpdateDoctorShiftRangeRequest();
			_svc.Setup(s => s.UpdateDoctorShiftsInRangeAsync(req)).Returns(Task.CompletedTask);
			var ctrl = NewController();
			var result = await ctrl.UpdateDoctorShiftsInRange(req);
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Contains("Cập nhật lịch làm việc thành công", ok.Value!.ToString());
			_svc.VerifyAll();
		}

		[Fact]
		public async Task UpdateDoctorShiftsInRange_Error_ReturnsBadRequest()
		{
			var req = new UpdateDoctorShiftRangeRequest();
			_svc.Setup(s => s.UpdateDoctorShiftsInRangeAsync(req)).ThrowsAsync(new Exception("err-root"));
			var ctrl = NewController();
			var result = await ctrl.UpdateDoctorShiftsInRange(req);
			var bad = Assert.IsType<BadRequestObjectResult>(result);
			var str = bad.Value!.ToString();
			Assert.Contains("error", str!, StringComparison.OrdinalIgnoreCase);
			Assert.Contains("err-root", str!);
			_svc.VerifyAll();
		}
	}
}
