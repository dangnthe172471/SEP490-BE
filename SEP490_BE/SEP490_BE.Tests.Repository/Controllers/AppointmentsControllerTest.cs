using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers.ManageReceptionist.ManageAppointment;
using SEP490_BE.BLL.IServices.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.DTOs.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.IRepositories.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.Models;
using System.Linq;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers.ManageReceptionist.ManageAppointment
{
    public class AppointmentsControllerTests
    {
        private readonly Mock<IAppointmentService> _svc = new();
        private readonly Mock<IAppointmentRepository> _repo = new();

        private AppointmentsController MakeControllerWithUser(int? userId = 1, string? role = "Patient")
        {
            var controller = new AppointmentsController(_svc.Object, _repo.Object);

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

        #region GetAll Tests

        [Fact]
        public async Task GetAll_ReturnsOk_WithData()
        {
            _svc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AppointmentDto>
                {
                    new() { AppointmentId = 1, PatientName = "John Doe", DoctorName = "Dr. Smith" },
                    new() { AppointmentId = 2, PatientName = "Jane Doe", DoctorName = "Dr. Johnson" }
                });

            var ctrl = MakeControllerWithUser(1, "Clinic Manager");

            var result = await ctrl.GetAll(CancellationToken.None);
            var okResult = result.Result as OkObjectResult;

            okResult.Should().NotBeNull();
            var list = okResult!.Value as IEnumerable<AppointmentDto>;
            list.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAll_ReturnsEmpty_WhenNoAppointments()
        {
            _svc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AppointmentDto>());

            var ctrl = MakeControllerWithUser(1, "Receptionist");

            var result = await ctrl.GetAll(CancellationToken.None);
            var okResult = result.Result as OkObjectResult;

            okResult.Should().NotBeNull();
            var list = okResult!.Value as IEnumerable<AppointmentDto>;
            list.Should().BeEmpty();
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_ReturnsOk_WhenFound()
        {
            _svc.Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppointmentDto
                {
                    AppointmentId = 5,
                    PatientName = "John Doe",
                    DoctorName = "Dr. Smith"
                });

            var ctrl = MakeControllerWithUser(1, "Doctor");

            var result = await ctrl.GetById(5, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNull()
        {
            _svc.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((AppointmentDto?)null);

            var ctrl = MakeControllerWithUser(1, "Doctor");

            var result = await ctrl.GetById(999, CancellationToken.None);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        #endregion

        #region BookAppointment (Patient) Tests

        [Fact]
        public async Task BookAppointment_ReturnsBadRequest_When_DoctorNotAvailable()
        {
            _svc.Setup(s => s.CreateAppointmentByPatientAsync(It.IsAny<BookAppointmentRequest>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Doctor is not available at this time."));

            var ctrl = MakeControllerWithUser(1, "Patient");
            var request = new BookAppointmentRequest
            {
                DoctorId = 5,
                AppointmentDate = DateTime.Now.AddDays(1),
                ReasonForVisit = "Checkup"
            };

            var result = await ctrl.BookAppointment(request, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task BookAppointment_ReturnsBadRequest_When_PastDate()
        {
            _svc.Setup(s => s.CreateAppointmentByPatientAsync(It.IsAny<BookAppointmentRequest>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Appointment date cannot be in the past."));

            var ctrl = MakeControllerWithUser(1, "Patient");
            var request = new BookAppointmentRequest
            {
                DoctorId = 5,
                AppointmentDate = DateTime.Now.AddDays(-1),
                ReasonForVisit = "Checkup"
            };

            var result = await ctrl.BookAppointment(request, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region CreateAppointment (Receptionist) Tests

        [Fact]
        public async Task CreateAppointment_ReturnsCreated_WhenSuccess()
        {
            var receptionist = new ReceptionistInfoDto { ReceptionistId = 20, UserId = 2 };

            _svc.Setup(s => s.GetReceptionistByUserIdAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(receptionist);
            _svc.Setup(s => s.CreateAppointmentByReceptionistAsync(It.IsAny<CreateAppointmentByReceptionistRequest>(), 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(101);

            var ctrl = MakeControllerWithUser(2, "Receptionist");
            var request = new CreateAppointmentByReceptionistRequest
            {
                PatientId = 10,
                DoctorId = 5,
                AppointmentDate = DateTime.Now.AddDays(2),
                ReasonForVisit = "Consultation"
            };

            var result = await ctrl.CreateAppointment(request, CancellationToken.None);

            result.Result.Should().BeOfType<CreatedAtActionResult>();
            _svc.Verify(s => s.CreateAppointmentByReceptionistAsync(request, 20, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAppointment_ReturnsBadRequest_When_PatientNotFound()
        {
            var receptionist = new ReceptionistInfoDto { ReceptionistId = 20, UserId = 2 };

            _svc.Setup(s => s.GetReceptionistByUserIdAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(receptionist);
            _svc.Setup(s => s.CreateAppointmentByReceptionistAsync(It.IsAny<CreateAppointmentByReceptionistRequest>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Patient not found."));

            var ctrl = MakeControllerWithUser(2, "Receptionist");
            var request = new CreateAppointmentByReceptionistRequest
            {
                PatientId = 999,
                DoctorId = 5,
                AppointmentDate = DateTime.Now.AddDays(2),
                ReasonForVisit = "Consultation"
            };

            var result = await ctrl.CreateAppointment(request, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateAppointment_ReturnsUnauthorized_When_ReceptionistNotFound()
        {
            _svc.Setup(s => s.GetReceptionistByUserIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReceptionistInfoDto?)null);

            var ctrl = MakeControllerWithUser(999, "Receptionist");
            var request = new CreateAppointmentByReceptionistRequest
            {
                PatientId = 10,
                DoctorId = 5,
                AppointmentDate = DateTime.Now.AddDays(2),
                ReasonForVisit = "Consultation"
            };

            var result = await ctrl.CreateAppointment(request, CancellationToken.None);

            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        #endregion

        #region Reschedule Tests

        [Fact]
        public async Task Reschedule_ReturnsOk_WhenSuccess()
        {
            _svc.Setup(s => s.RescheduleAppointmentAsync(5, 1, It.IsAny<RescheduleAppointmentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var ctrl = MakeControllerWithUser(1, "Patient");
            var request = new RescheduleAppointmentRequest
            {
                NewAppointmentDate = DateTime.Now.AddDays(3),
                NewReasonForVisit = "Updated reason"
            };

            var result = await ctrl.Reschedule(5, request, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
            _svc.Verify(s => s.RescheduleAppointmentAsync(5, 1, request, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Reschedule_ReturnsNotFound_WhenAppointmentNotFound()
        {
            _svc.Setup(s => s.RescheduleAppointmentAsync(999, 1, It.IsAny<RescheduleAppointmentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var ctrl = MakeControllerWithUser(1, "Patient");
            var request = new RescheduleAppointmentRequest
            {
                NewAppointmentDate = DateTime.Now.AddDays(3)
            };

            var result = await ctrl.Reschedule(999, request, CancellationToken.None);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Reschedule_ReturnsBadRequest_When_CannotReschedule()
        {
            _svc.Setup(s => s.RescheduleAppointmentAsync(5, 1, It.IsAny<RescheduleAppointmentRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Cannot reschedule a completed appointment."));

            var ctrl = MakeControllerWithUser(1, "Patient");
            var request = new RescheduleAppointmentRequest
            {
                NewAppointmentDate = DateTime.Now.AddDays(3)
            };

            var result = await ctrl.Reschedule(5, request, CancellationToken.None);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region UpdateStatus Tests

        [Fact]
        public async Task UpdateStatus_ReturnsOk_WhenSuccess()
        {
            _svc.Setup(s => s.UpdateAppointmentStatusAsync(5, It.IsAny<UpdateAppointmentStatusRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var ctrl = MakeControllerWithUser(1, "Doctor");
            var request = new UpdateAppointmentStatusRequest { Status = "Confirmed" };

            var result = await ctrl.UpdateStatus(5, request, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
            _svc.Verify(s => s.UpdateAppointmentStatusAsync(5, request, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_ReturnsNotFound_WhenAppointmentNotFound()
        {
            _svc.Setup(s => s.UpdateAppointmentStatusAsync(999, It.IsAny<UpdateAppointmentStatusRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var ctrl = MakeControllerWithUser(1, "Doctor");
            var request = new UpdateAppointmentStatusRequest { Status = "Confirmed" };

            var result = await ctrl.UpdateStatus(999, request, CancellationToken.None);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UpdateStatus_ReturnsBadRequest_When_InvalidStatus()
        {
            _svc.Setup(s => s.UpdateAppointmentStatusAsync(5, It.IsAny<UpdateAppointmentStatusRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Invalid status value."));

            var ctrl = MakeControllerWithUser(1, "Doctor");
            var request = new UpdateAppointmentStatusRequest { Status = "InvalidStatus" };

            var result = await ctrl.UpdateStatus(5, request, CancellationToken.None);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region GetConfirmation Tests

        [Fact]
        public async Task GetConfirmation_ReturnsOk_WhenFound()
        {
            _svc.Setup(s => s.GetAppointmentConfirmationAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppointmentConfirmationDto
                {
                    AppointmentId = 5,
                    PatientName = "John Doe",
                    DoctorName = "Dr. Smith"
                });

            var ctrl = MakeControllerWithUser(1, "Patient");

            var result = await ctrl.GetConfirmation(5, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetConfirmation_ReturnsNotFound_WhenNull()
        {
            _svc.Setup(s => s.GetAppointmentConfirmationAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((AppointmentConfirmationDto?)null);

            var ctrl = MakeControllerWithUser(1, "Patient");

            var result = await ctrl.GetConfirmation(999, CancellationToken.None);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        #endregion

        #region GetMyAppointments (Patient) Tests

        [Fact]
        public async Task GetMyAppointments_ReturnsOk_WhenPatientExists()
        {
            var patient = new Patient { PatientId = 10, UserId = 1 };
            _repo.Setup(r => r.GetPatientByUserIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);
            _svc.Setup(s => s.GetByPatientIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AppointmentDto>
                {
                    new() { AppointmentId = 1, PatientName = "John Doe" }
                });

            var ctrl = MakeControllerWithUser(1, "Patient");

            var result = await ctrl.GetMyAppointments(CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMyAppointments_ReturnsNotFound_WhenPatientNotExists()
        {
            _repo.Setup(r => r.GetPatientByUserIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Patient?)null);

            var ctrl = MakeControllerWithUser(999, "Patient");

            var result = await ctrl.GetMyAppointments(CancellationToken.None);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        #endregion

        #region GetMyDoctorAppointments Tests

        [Fact]
        public async Task GetMyDoctorAppointments_ReturnsOk_WhenDoctorExists()
        {
            var user = new User { UserId = 1, FullName = "Dr. Smith" };
            var doctor = new DoctorInfoDto { DoctorId = 5, UserId = 1 };
            _svc.Setup(s => s.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _svc.Setup(s => s.GetDoctorByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);
            _svc.Setup(s => s.GetByDoctorIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AppointmentDto>
                {
                    new() { AppointmentId = 1, DoctorName = "Dr. Smith" }
                });

            var ctrl = MakeControllerWithUser(1, "Doctor");

            var result = await ctrl.GetMyDoctorAppointments(CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMyDoctorAppointments_ReturnsNotFound_WhenDoctorNotExists()
        {
            var user = new User { UserId = 1 };
            _svc.Setup(s => s.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _svc.Setup(s => s.GetDoctorByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DoctorInfoDto?)null);

            var ctrl = MakeControllerWithUser(1, "Doctor");

            var result = await ctrl.GetMyDoctorAppointments(CancellationToken.None);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        #endregion

        #region GetMyReceptionistAppointments Tests

        [Fact]
        public async Task GetMyReceptionistAppointments_ReturnsOk_WhenReceptionistExists()
        {
            var user = new User { UserId = 2, FullName = "Receptionist" };
            var receptionist = new ReceptionistInfoDto { ReceptionistId = 20, UserId = 2 };
            _svc.Setup(s => s.GetUserByIdAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _svc.Setup(s => s.GetReceptionistByUserIdAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(receptionist);
            _svc.Setup(s => s.GetByReceptionistIdAsync(20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AppointmentDto>
                {
                    new() { AppointmentId = 1, ReceptionistName = "Receptionist" }
                });

            var ctrl = MakeControllerWithUser(2, "Receptionist");

            var result = await ctrl.GetMyReceptionistAppointments(CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMyReceptionistAppointments_ReturnsNotFound_WhenReceptionistNotExists()
        {
            var user = new User { UserId = 2 };
            _svc.Setup(s => s.GetUserByIdAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _svc.Setup(s => s.GetReceptionistByUserIdAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReceptionistInfoDto?)null);

            var ctrl = MakeControllerWithUser(2, "Receptionist");

            var result = await ctrl.GetMyReceptionistAppointments(CancellationToken.None);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        #endregion

        #region GetTimeSeries Tests

        [Fact]
        public async Task GetTimeSeries_ReturnsOk_WithData()
        {
            var timeSeriesData = new List<AppointmentTimeSeriesPointDto>
            {
                new() { Period = "2024-01-01", Count = 10 },
                new() { Period = "2024-01-02", Count = 15 }
            };
            _svc.Setup(s => s.GetAppointmentTimeSeriesAsync(
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), "day", It.IsAny<CancellationToken>()))
                .ReturnsAsync(timeSeriesData);

            var ctrl = MakeControllerWithUser(1, "Clinic Manager");

            var result = await ctrl.GetTimeSeries(null, null, "day", CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var data = okResult!.Value as List<AppointmentTimeSeriesPointDto>;
            data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetTimeSeries_ReturnsOk_WithDateRange()
        {
            var from = DateTime.Now.AddDays(-30);
            var to = DateTime.Now;
            var timeSeriesData = new List<AppointmentTimeSeriesPointDto>();
            _svc.Setup(s => s.GetAppointmentTimeSeriesAsync(
                from, to, "month", It.IsAny<CancellationToken>()))
                .ReturnsAsync(timeSeriesData);

            var ctrl = MakeControllerWithUser(1, "Clinic Manager");

            var result = await ctrl.GetTimeSeries(from, to, "month", CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        #endregion

        #region GetHeatmap Tests

        [Fact]
        public async Task GetHeatmap_ReturnsOk_WithData()
        {
            var heatmapData = new List<AppointmentHeatmapPointDto>
            {
                new() { Weekday = 1, Hour = 9, Count = 5 },
                new() { Weekday = 1, Hour = 10, Count = 8 }
            };
            _svc.Setup(s => s.GetAppointmentHeatmapAsync(
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(heatmapData);

            var ctrl = MakeControllerWithUser(1, "Clinic Manager");

            var result = await ctrl.GetHeatmap(null, null, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var data = okResult!.Value as List<AppointmentHeatmapPointDto>;
            data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetHeatmap_ReturnsOk_WithDateRange()
        {
            var from = DateTime.Now.AddDays(-30);
            var to = DateTime.Now;
            var heatmapData = new List<AppointmentHeatmapPointDto>();
            _svc.Setup(s => s.GetAppointmentHeatmapAsync(
                from, to, It.IsAny<CancellationToken>()))
                .ReturnsAsync(heatmapData);

            var ctrl = MakeControllerWithUser(1, "Clinic Manager");

            var result = await ctrl.GetHeatmap(from, to, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        #endregion

        #region GetStatistics Tests

        [Fact]
        public async Task GetStatistics_ReturnsOk_WithData()
        {
            _svc.Setup(s => s.GetAppointmentStatisticsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppointmentStatisticsDto
                {
                    TotalAppointments = 100,
                    PendingAppointments = 20,
                    ConfirmedAppointments = 50,
                    CompletedAppointments = 25,
                    CancelledAppointments = 5
                });

            var ctrl = MakeControllerWithUser(1, "Clinic Manager");

            var result = await ctrl.GetStatistics(CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenSuccess()
        {
            _svc.Setup(s => s.DeleteAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var ctrl = MakeControllerWithUser(1, "Clinic Manager");

            var result = await ctrl.Delete(5, CancellationToken.None);

            result.Should().BeOfType<NoContentResult>();
            _svc.Verify(s => s.DeleteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenNotDeleted()
        {
            _svc.Setup(s => s.DeleteAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var ctrl = MakeControllerWithUser(1, "Clinic Manager");

            var result = await ctrl.Delete(999, CancellationToken.None);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        #endregion

        #region Doctor Endpoints Tests

        [Fact]
        public async Task GetAllDoctors_ReturnsOk_WithData()
        {
            _svc.Setup(s => s.GetAllDoctorsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DoctorInfoDto>
                {
                    new() { DoctorId = 1, FullName = "Dr. Smith", Specialty = "Cardiology" },
                    new() { DoctorId = 2, FullName = "Dr. Johnson", Specialty = "Neurology" }
                });

            var ctrl = MakeControllerWithUser(1, "Patient");

            var result = await ctrl.GetAllDoctors(CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetDoctorById_ReturnsOk_WhenFound()
        {
            _svc.Setup(s => s.GetDoctorByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DoctorInfoDto { DoctorId = 5, FullName = "Dr. Smith" });

            var ctrl = MakeControllerWithUser(1, "Patient");

            var result = await ctrl.GetDoctorById(5, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetDoctorById_ReturnsNotFound_WhenNull()
        {
            _svc.Setup(s => s.GetDoctorByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DoctorInfoDto?)null);

            var ctrl = MakeControllerWithUser(1, "Patient");

            var result = await ctrl.GetDoctorById(999, CancellationToken.None);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        #endregion

        #region Patient Endpoints Tests

        [Fact]
        public async Task GetPatientById_ReturnsOk_WhenFound()
        {
            _svc.Setup(s => s.GetPatientByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PatientInfoDto { PatientId = 10, FullName = "John Doe" });

            var ctrl = MakeControllerWithUser(1, "Doctor");

            var result = await ctrl.GetPatientById(10, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPatientById_ReturnsNotFound_WhenNull()
        {
            _svc.Setup(s => s.GetPatientByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PatientInfoDto?)null);

            var ctrl = MakeControllerWithUser(1, "Doctor");

            var result = await ctrl.GetPatientById(999, CancellationToken.None);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetPatientByUserId_ReturnsOk_WhenFound()
        {
            _svc.Setup(s => s.GetPatientInfoByUserIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PatientInfoDto { PatientId = 10, UserId = 1 });

            var ctrl = MakeControllerWithUser(1, "Receptionist");

            var result = await ctrl.GetPatientByUserId(1, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        #endregion

        #region Receptionist Endpoints Tests

        [Fact]
        public async Task GetReceptionistById_ReturnsOk_WhenFound()
        {
            _svc.Setup(s => s.GetReceptionistByIdAsync(20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReceptionistInfoDto { ReceptionistId = 20, FullName = "Jane Doe" });

            var ctrl = MakeControllerWithUser(1, "Clinic Manager");

            var result = await ctrl.GetReceptionistById(20, CancellationToken.None);

            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetReceptionistById_ReturnsNotFound_WhenNull()
        {
            _svc.Setup(s => s.GetReceptionistByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReceptionistInfoDto?)null);

            var ctrl = MakeControllerWithUser(1, "Clinic Manager");

            var result = await ctrl.GetReceptionistById(999, CancellationToken.None);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        #endregion

        #region Edge Cases and Additional Tests

        [Fact]
        public async Task BookAppointment_Handles_SpecialCharacters_In_ReasonForVisit()
        {
            _svc.Setup(s => s.CreateAppointmentByPatientAsync(It.IsAny<BookAppointmentRequest>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var ctrl = MakeControllerWithUser(1, "Patient");
            var request = new BookAppointmentRequest
            {
                DoctorId = 5,
                AppointmentDate = DateTime.Now.AddDays(1),
                ReasonForVisit = "Khám tổng quát & kiểm tra sức khỏe định kỳ"
            };

            var result = await ctrl.BookAppointment(request, CancellationToken.None);

            result.Result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        public async Task BookAppointment_Handles_LongReasonForVisit()
        {
            _svc.Setup(s => s.CreateAppointmentByPatientAsync(It.IsAny<BookAppointmentRequest>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var ctrl = MakeControllerWithUser(1, "Patient");
            var longReason = new string('A', 500);
            var request = new BookAppointmentRequest
            {
                DoctorId = 5,
                AppointmentDate = DateTime.Now.AddDays(1),
                ReasonForVisit = longReason
            };

            var result = await ctrl.BookAppointment(request, CancellationToken.None);

            result.Result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        public async Task BookAppointment_ReturnsBadRequest_When_ReasonForVisit_TooLong()
        {
            _svc.Setup(s => s.CreateAppointmentByPatientAsync(It.IsAny<BookAppointmentRequest>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("ReasonForVisit cannot exceed 500 characters."));

            var ctrl = MakeControllerWithUser(1, "Patient");
            var request = new BookAppointmentRequest
            {
                DoctorId = 5,
                AppointmentDate = DateTime.Now.AddDays(1),
                ReasonForVisit = new string('A', 501)
            };

            var result = await ctrl.BookAppointment(request, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateAppointment_ReturnsBadRequest_When_DoctorNotAvailable()
        {
            var receptionist = new ReceptionistInfoDto { ReceptionistId = 20, UserId = 2 };

            _svc.Setup(s => s.GetReceptionistByUserIdAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(receptionist);
            _svc.Setup(s => s.CreateAppointmentByReceptionistAsync(It.IsAny<CreateAppointmentByReceptionistRequest>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Doctor is not available at this time."));

            var ctrl = MakeControllerWithUser(2, "Receptionist");
            var request = new CreateAppointmentByReceptionistRequest
            {
                PatientId = 10,
                DoctorId = 5,
                AppointmentDate = DateTime.Now.AddDays(2),
                ReasonForVisit = "Consultation"
            };

            var result = await ctrl.CreateAppointment(request, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

     

        [Fact]
        public async Task Reschedule_ReturnsUnauthorized_When_NotAppointmentOwner()
        {
            _svc.Setup(s => s.RescheduleAppointmentAsync(5, 1, It.IsAny<RescheduleAppointmentRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnauthorizedAccessException("You are not authorized to reschedule this appointment."));

            var ctrl = MakeControllerWithUser(1, "Patient");
            var request = new RescheduleAppointmentRequest
            {
                NewAppointmentDate = DateTime.Now.AddDays(3)
            };

            var result = await ctrl.Reschedule(5, request, CancellationToken.None);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task UpdateStatus_Handles_AllValidStatuses()
        {
            var statuses = new[] { "Pending", "Confirmed", "Completed", "Cancelled", "No Show" };

            foreach (var status in statuses)
            {
                _svc.Setup(s => s.UpdateAppointmentStatusAsync(5, It.Is<UpdateAppointmentStatusRequest>(r => r.Status == status), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

                var ctrl = MakeControllerWithUser(1, "Doctor");
                var request = new UpdateAppointmentStatusRequest { Status = status };

                var result = await ctrl.UpdateStatus(5, request, CancellationToken.None);

                result.Should().BeOfType<OkObjectResult>();
            }
        }

        [Fact]
        public async Task GetStatistics_Returns_ZeroValues_WhenNoAppointments()
        {
            _svc.Setup(s => s.GetAppointmentStatisticsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppointmentStatisticsDto
                {
                    TotalAppointments = 0,
                    PendingAppointments = 0,
                    ConfirmedAppointments = 0,
                    CompletedAppointments = 0,
                    CancelledAppointments = 0,
                    NoShowAppointments = 0
                });

            var ctrl = MakeControllerWithUser(1, "Clinic Manager");

            var result = await ctrl.GetStatistics(CancellationToken.None);
            var okResult = result.Result as OkObjectResult;

            okResult.Should().NotBeNull();
            var stats = okResult!.Value as AppointmentStatisticsDto;
            stats!.TotalAppointments.Should().Be(0);
        }


        [Fact]
        public async Task GetAllDoctors_Returns_EmptyList_WhenNoDoctors()
        {
            _svc.Setup(s => s.GetAllDoctorsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DoctorInfoDto>());

            var ctrl = MakeControllerWithUser(1, "Patient");

            var result = await ctrl.GetAllDoctors(CancellationToken.None);
            var okResult = result.Result as OkObjectResult;

            okResult.Should().NotBeNull();
            var doctors = okResult!.Value as IEnumerable<DoctorInfoDto>;
            doctors.Should().BeEmpty();
        }

        [Fact]
        public async Task BookAppointment_ReturnsBadRequest_When_DoctorId_Invalid()
        {
            _svc.Setup(s => s.CreateAppointmentByPatientAsync(It.IsAny<BookAppointmentRequest>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Doctor not found."));

            var ctrl = MakeControllerWithUser(1, "Patient");
            var request = new BookAppointmentRequest
            {
                DoctorId = -1,
                AppointmentDate = DateTime.Now.AddDays(1),
                ReasonForVisit = "Checkup"
            };

            var result = await ctrl.BookAppointment(request, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateAppointment_ReturnsBadRequest_When_PatientId_Invalid()
        {
            var receptionist = new ReceptionistInfoDto { ReceptionistId = 20, UserId = 2 };

            _svc.Setup(s => s.GetReceptionistByUserIdAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(receptionist);
            _svc.Setup(s => s.CreateAppointmentByReceptionistAsync(It.IsAny<CreateAppointmentByReceptionistRequest>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Patient not found."));

            var ctrl = MakeControllerWithUser(2, "Receptionist");
            var request = new CreateAppointmentByReceptionistRequest
            {
                PatientId = -1,
                DoctorId = 5,
                AppointmentDate = DateTime.Now.AddDays(2),
                ReasonForVisit = "Consultation"
            };

            var result = await ctrl.CreateAppointment(request, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task BookAppointment_ReturnsBadRequest_When_AppointmentDate_TooFarInFuture()
        {
            _svc.Setup(s => s.CreateAppointmentByPatientAsync(It.IsAny<BookAppointmentRequest>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Appointment date cannot be more than 6 months in the future."));

            var ctrl = MakeControllerWithUser(1, "Patient");
            var request = new BookAppointmentRequest
            {
                DoctorId = 5,
                AppointmentDate = DateTime.Now.AddMonths(7),
                ReasonForVisit = "Checkup"
            };

            var result = await ctrl.BookAppointment(request, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateStatus_ReturnsBadRequest_When_Transition_NotAllowed()
        {
            _svc.Setup(s => s.UpdateAppointmentStatusAsync(5, It.IsAny<UpdateAppointmentStatusRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Cannot change status from 'Completed' to 'Pending'."));

            var ctrl = MakeControllerWithUser(1, "Doctor");
            var request = new UpdateAppointmentStatusRequest { Status = "Pending" };

            var result = await ctrl.UpdateStatus(5, request, CancellationToken.None);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsBadRequest_When_Appointment_InProgress()
        {
            _svc.Setup(s => s.DeleteAsync(5, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Cannot delete an appointment that is in progress or completed."));

            var ctrl = MakeControllerWithUser(1, "Clinic Manager");

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await ctrl.Delete(5, CancellationToken.None);
            });
        }

        [Fact]
        public async Task GetConfirmation_Returns_AllRequiredFields()
        {
            var confirmation = new AppointmentConfirmationDto
            {
                AppointmentId = 5,
                PatientName = "John Doe",
                PatientEmail = "john@example.com",
                PatientPhone = "0123456789",
                DoctorName = "Dr. Smith",
                DoctorSpecialty = "Cardiology",
                AppointmentDate = DateTime.Now.AddDays(1),
                ReasonForVisit = "Annual checkup",
                Status = "Confirmed",
                CreatedAt = DateTime.Now,
                ReceptionistName = "Jane Receptionist"
            };

            _svc.Setup(s => s.GetAppointmentConfirmationAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(confirmation);

            var ctrl = MakeControllerWithUser(1, "Patient");

            var result = await ctrl.GetConfirmation(5, CancellationToken.None);
            var okResult = result.Result as OkObjectResult;

            okResult.Should().NotBeNull();
            var returnedConfirmation = okResult!.Value as AppointmentConfirmationDto;
            returnedConfirmation.Should().NotBeNull();
            returnedConfirmation!.AppointmentId.Should().Be(5);
            returnedConfirmation.PatientName.Should().Be("John Doe");
            returnedConfirmation.DoctorName.Should().Be("Dr. Smith");
        }

        [Fact]
        public async Task BookAppointment_ReturnsBadRequest_When_ReasonForVisit_Empty()
        {
            _svc.Setup(s => s.CreateAppointmentByPatientAsync(It.IsAny<BookAppointmentRequest>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("ReasonForVisit is required."));

            var ctrl = MakeControllerWithUser(1, "Patient");
            var request = new BookAppointmentRequest
            {
                DoctorId = 5,
                AppointmentDate = DateTime.Now.AddDays(1),
                ReasonForVisit = ""
            };

            var result = await ctrl.BookAppointment(request, CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateAppointment_Handles_Multiple_Concurrent_Requests()
        {
            var receptionist = new ReceptionistInfoDto { ReceptionistId = 20, UserId = 2 };
            var appointmentIds = new[] { 101, 102, 103 };
            var setupCounter = 0;

            _svc.Setup(s => s.GetReceptionistByUserIdAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(receptionist);
            _svc.Setup(s => s.CreateAppointmentByReceptionistAsync(It.IsAny<CreateAppointmentByReceptionistRequest>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => appointmentIds[setupCounter++ % appointmentIds.Length]);

            var ctrl = MakeControllerWithUser(2, "Receptionist");

            var tasks = Enumerable.Range(1, 3).Select(async i =>
            {
                var request = new CreateAppointmentByReceptionistRequest
                {
                    PatientId = 10 + i,
                    DoctorId = 5,
                    AppointmentDate = DateTime.Now.AddDays(i),
                    ReasonForVisit = $"Consultation {i}"
                };
                return await ctrl.CreateAppointment(request, CancellationToken.None);
            });

            var results = await Task.WhenAll(tasks);

            results.Should().HaveCount(3);
            results.Should().AllSatisfy(r => r.Result.Should().BeOfType<CreatedAtActionResult>());
        }

        [Fact]
        public async Task BookAppointment_ReturnsUnauthorized_When_UserId_Invalid()
        {
            var ctrl = MakeControllerWithUser(null, "Patient");
            var request = new BookAppointmentRequest
            {
                DoctorId = 5,
                AppointmentDate = DateTime.Now.AddDays(1),
                ReasonForVisit = "Checkup"
            };

            var result = await ctrl.BookAppointment(request, CancellationToken.None);

            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task CreateAppointment_Returns500_When_Exception()
        {
            var receptionist = new ReceptionistInfoDto { ReceptionistId = 20, UserId = 2 };

            _svc.Setup(s => s.GetReceptionistByUserIdAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(receptionist);
            _svc.Setup(s => s.CreateAppointmentByReceptionistAsync(It.IsAny<CreateAppointmentByReceptionistRequest>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var ctrl = MakeControllerWithUser(2, "Receptionist");
            var request = new CreateAppointmentByReceptionistRequest
            {
                PatientId = 10,
                DoctorId = 5,
                AppointmentDate = DateTime.Now.AddDays(2),
                ReasonForVisit = "Consultation"
            };

            var result = await ctrl.CreateAppointment(request, CancellationToken.None);

            var statusCode = Assert.IsType<ObjectResult>(result.Result);
            statusCode.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task Reschedule_ReturnsBadRequest_When_ArgumentException()
        {
            _svc.Setup(s => s.RescheduleAppointmentAsync(5, 1, It.IsAny<RescheduleAppointmentRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Invalid date."));

            var ctrl = MakeControllerWithUser(1, "Patient");
            var request = new RescheduleAppointmentRequest
            {
                NewAppointmentDate = DateTime.Now.AddDays(-1)
            };

            var result = await ctrl.Reschedule(5, request, CancellationToken.None);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateStatus_ReturnsBadRequest_When_InvalidOperationException()
        {
            _svc.Setup(s => s.UpdateAppointmentStatusAsync(5, It.IsAny<UpdateAppointmentStatusRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Cannot update status."));

            var ctrl = MakeControllerWithUser(1, "Doctor");
            var request = new UpdateAppointmentStatusRequest { Status = "Completed" };

            var result = await ctrl.UpdateStatus(5, request, CancellationToken.None);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetMyAppointments_ReturnsBadRequest_When_Exception()
        {
            _repo.Setup(r => r.GetPatientByUserIdAsync(1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var ctrl = MakeControllerWithUser(1, "Patient");

            var result = await ctrl.GetMyAppointments(CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetMyDoctorAppointments_ReturnsBadRequest_When_Exception()
        {
            _svc.Setup(s => s.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var ctrl = MakeControllerWithUser(1, "Doctor");

            var result = await ctrl.GetMyDoctorAppointments(CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetMyReceptionistAppointments_ReturnsBadRequest_When_Exception()
        {
            _svc.Setup(s => s.GetUserByIdAsync(2, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var ctrl = MakeControllerWithUser(2, "Receptionist");

            var result = await ctrl.GetMyReceptionistAppointments(CancellationToken.None);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetStatistics_Returns500_When_Exception()
        {
            _svc.Setup(s => s.GetAppointmentStatisticsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var ctrl = MakeControllerWithUser(1, "Clinic Manager");

            var result = await ctrl.GetStatistics(CancellationToken.None);

            var statusCode = Assert.IsType<ObjectResult>(result.Result);
            statusCode.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetTimeSeries_Returns500_When_Exception()
        {
            _svc.Setup(s => s.GetAppointmentTimeSeriesAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var ctrl = MakeControllerWithUser(1, "Clinic Manager");

            var result = await ctrl.GetTimeSeries(null, null, "day", CancellationToken.None);

            var statusCode = Assert.IsType<ObjectResult>(result.Result);
            statusCode.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetHeatmap_Returns500_When_Exception()
        {
            _svc.Setup(s => s.GetAppointmentHeatmapAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var ctrl = MakeControllerWithUser(1, "Clinic Manager");

            var result = await ctrl.GetHeatmap(null, null, CancellationToken.None);

            var statusCode = Assert.IsType<ObjectResult>(result.Result);
            statusCode.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetMyAppointments_ReturnsUnauthorized_When_UserId_Invalid()
        {
            var ctrl = MakeControllerWithUser(null, "Patient");

            var result = await ctrl.GetMyAppointments(CancellationToken.None);

            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task GetMyDoctorAppointments_ReturnsUnauthorized_When_UserId_Invalid()
        {
            var ctrl = MakeControllerWithUser(null, "Doctor");

            var result = await ctrl.GetMyDoctorAppointments(CancellationToken.None);

            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task GetMyReceptionistAppointments_ReturnsUnauthorized_When_UserId_Invalid()
        {
            var ctrl = MakeControllerWithUser(null, "Receptionist");

            var result = await ctrl.GetMyReceptionistAppointments(CancellationToken.None);

            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Reschedule_ReturnsUnauthorized_When_UserId_Invalid()
        {
            var ctrl = MakeControllerWithUser(null, "Patient");
            var request = new RescheduleAppointmentRequest
            {
                NewAppointmentDate = DateTime.Now.AddDays(3)
            };

            var result = await ctrl.Reschedule(5, request, CancellationToken.None);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        #endregion
    }
}