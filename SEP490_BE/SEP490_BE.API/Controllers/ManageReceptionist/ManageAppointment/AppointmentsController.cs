using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.DTOs.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.IRepositories.ManageReceptionist.ManageAppointment;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SEP490_BE.API.Controllers.ManageReceptionist.ManageAppointment
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IAppointmentRepository _appointmentRepository;

        public AppointmentsController(IAppointmentService appointmentService, IAppointmentRepository appointmentRepository)
        {
            _appointmentService = appointmentService;
            _appointmentRepository = appointmentRepository;
        }

        #region Helper Methods

        /// <summary>
        /// Lấy UserId từ JWT token với logic fallback
        /// </summary>
        private int GetUserIdFromToken()
        {
            Console.WriteLine($"[DEBUG] === JWT Token Claims Analysis ===");
            Console.WriteLine($"[DEBUG] All claims in token:");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"[DEBUG] Claim Type: {claim.Type}, Value: {claim.Value}");
            }

            // ✅ FIX: Tìm tất cả NameIdentifier claims và ưu tiên cái có giá trị nhỏ nhất (UserId thực)
            var nameIdentifierClaims = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).ToList();
            Console.WriteLine($"[DEBUG] Found {nameIdentifierClaims.Count} NameIdentifier claims:");

            foreach (var claim in nameIdentifierClaims)
            {
                Console.WriteLine($"[DEBUG] NameIdentifier: '{claim.Value}'");
            }

            // Ưu tiên claim có giá trị nhỏ nhất (UserId thực thay vì phone number)
            var userIdClaim = nameIdentifierClaims
                .Where(c => int.TryParse(c.Value, out int val) && val < 1000) // UserId thường < 1000
                .OrderBy(c => int.Parse(c.Value))
                .FirstOrDefault()?.Value;

            Console.WriteLine($"[DEBUG] Selected NameIdentifier claim value: '{userIdClaim}'");

            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                Console.WriteLine($"[DEBUG] ✅ Successfully parsed UserId from NameIdentifier: {userId}");
                return userId;
            }

            // Fallback: nếu không tìm thấy UserId nhỏ, thử tìm UserId bất kỳ
            var anyUserIdClaim = nameIdentifierClaims
                .Where(c => int.TryParse(c.Value, out int val))
                .OrderBy(c => int.Parse(c.Value))
                .FirstOrDefault()?.Value;

            if (!string.IsNullOrEmpty(anyUserIdClaim) && int.TryParse(anyUserIdClaim, out int anyUserId))
            {
                Console.WriteLine($"[DEBUG] ✅ Using any valid UserId from NameIdentifier: {anyUserId}");
                return anyUserId;
            }

            // Fallback: thử lấy từ Sub claim (số điện thoại)
            var subClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            Console.WriteLine($"[DEBUG] Sub claim value: '{subClaim}'");

            if (!string.IsNullOrEmpty(subClaim) && subClaim.Length >= 10)
            {
                Console.WriteLine($"[DEBUG] Sub looks like phone number: {subClaim}");
                if (long.TryParse(subClaim, out long phoneAsLong))
                {
                    Console.WriteLine($"[DEBUG] ✅ Parsed phone as long: {phoneAsLong}");
                    int phoneAsInt = (int)(phoneAsLong % int.MaxValue); // Truncate if necessary
                    Console.WriteLine($"[DEBUG] ✅ Using phone as int: {phoneAsInt}");
                    return phoneAsInt;
                }
            }

            var errorMessage = $"Cannot parse UserId from JWT token. " +
                              $"NameIdentifier: '{userIdClaim}', Sub: '{subClaim}'. " +
                              $"Please check token generation in AuthController.";

            Console.WriteLine($"[ERROR] {errorMessage}");
            throw new UnauthorizedAccessException(errorMessage);
        }

        /// <summary>
        /// Lấy ReceptionistId từ JWT token (dành riêng cho Receptionist)
        /// </summary>
        private async Task<int> GetReceptionistIdFromTokenAsync(CancellationToken cancellationToken)
        {
            try
            {
                int userId = GetUserIdFromToken();
                Console.WriteLine($"[DEBUG] Getting ReceptionistId for UserId: {userId}");

                // Tìm receptionist theo userId
                var receptionist = await _appointmentService.GetReceptionistByUserIdAsync(userId, cancellationToken);
                if (receptionist == null)
                {
                    throw new UnauthorizedAccessException($"Receptionist not found for UserId: {userId}");
                }

                Console.WriteLine($"[DEBUG] ✅ Found ReceptionistId: {receptionist.ReceptionistId} for UserId: {userId}");
                return receptionist.ReceptionistId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to get ReceptionistId: {ex.Message}");
                throw new UnauthorizedAccessException($"Cannot get ReceptionistId: {ex.Message}");
            }
        }

        #endregion

        #region Debug Endpoints

        // GET: api/appointments/debug/user-info
        [HttpGet("debug/user-info")]
        [Authorize(Roles = "Patient")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult> DebugUserInfo()
        {
            try
            {
                int userId = GetUserIdFromToken();

                // Debug: Kiểm tra User trong database
                var user = await _appointmentRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return Ok(new
                    {
                        message = "User not found",
                        userId = userId,
                        claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
                    });
                }

                // Debug: Kiểm tra Patient
                var patient = await _appointmentRepository.GetPatientByUserIdAsync(userId);
                if (patient == null)
                {
                    return Ok(new
                    {
                        message = "Patient not found for User",
                        userId = userId,
                        user = new { user.UserId, user.Phone, user.FullName, user.Email },
                        claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
                    });
                }

                return Ok(new
                {
                    message = "Success",
                    userId = userId,
                    user = new { user.UserId, user.Phone, user.FullName, user.Email },
                    patient = new { patient.PatientId, patient.UserId },
                    claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // GET: api/appointments/debug/receptionist-info
        [HttpGet("debug/receptionist-info")]
        [Authorize(Roles = "Receptionist")]
        public async Task<ActionResult> DebugReceptionistInfo(CancellationToken cancellationToken)
        {
            try
            {
                int userId = GetUserIdFromToken();
                Console.WriteLine($"[DEBUG] Getting receptionist info for UserId: {userId}");

                // Debug: Kiểm tra User trong database
                var user = await _appointmentRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return Ok(new
                    {
                        message = "User not found",
                        userId = userId,
                        claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
                    });
                }

                // Debug: Kiểm tra Receptionist
                var receptionist = await _appointmentService.GetReceptionistByUserIdAsync(userId, cancellationToken);
                if (receptionist == null)
                {
                    return Ok(new
                    {
                        message = "Receptionist not found for User",
                        userId = userId,
                        user = new { user.UserId, user.Phone, user.FullName, user.Email },
                        claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
                    });
                }

                return Ok(new
                {
                    message = "Success",
                    userId = userId,
                    user = new { user.UserId, user.Phone, user.FullName, user.Email },
                    receptionist = new { receptionist.ReceptionistId, receptionist.UserId },
                    claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        #endregion

        #region Appointment Endpoints

        // GET: api/appointments
        [HttpGet]
        //[Authorize(Roles = "Clinic Manager,Receptionist,Doctor")]
        public async Task<ActionResult<List<AppointmentDto>>> GetAll(CancellationToken cancellationToken)
        {
            var appointments = await _appointmentService.GetAllAsync(cancellationToken);
            return Ok(appointments);
        }

        // GET: api/appointments/{id}
        [HttpGet("{id}")]
        //[Authorize(Roles = "Clinic Manager,Receptionist,Doctor")]
        public async Task<ActionResult<AppointmentDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var appointment = await _appointmentService.GetByIdAsync(id, cancellationToken);
            if (appointment == null)
            {
                return NotFound(new { message = "Appointment not found." });
            }

            return Ok(appointment);
        }

        // POST: api/appointments/book (Patient tự đặt lịch)
        [HttpPost("book")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<int>> BookAppointment(
            [FromBody] BookAppointmentRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                int userId = GetUserIdFromToken();
                var id = await _appointmentService.CreateAppointmentByPatientAsync(request, userId, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id }, new { appointmentId = id });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/appointments/create (Receptionist tạo appointment cho patient)
        [HttpPost("create")]
        [Authorize(Roles = "Receptionist")]
        public async Task<ActionResult<int>> CreateAppointment(
            [FromBody] CreateAppointmentByReceptionistRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"[DEBUG] === Creating Appointment by Receptionist ===");
                Console.WriteLine($"[DEBUG] Request data: PatientId={request.PatientId}, DoctorId={request.DoctorId}, AppointmentDate={request.AppointmentDate}, ReasonForVisit={request.ReasonForVisit}");

                // Sử dụng method mới để lấy ReceptionistId
                int receptionistId = await GetReceptionistIdFromTokenAsync(cancellationToken);
                Console.WriteLine($"[DEBUG] Using ReceptionistId: {receptionistId}");

                var id = await _appointmentService.CreateAppointmentByReceptionistAsync(request, receptionistId, cancellationToken);
                Console.WriteLine($"[DEBUG] ✅ Created appointment with ID: {id}");

                return CreatedAtAction(nameof(GetById), new { id }, new { appointmentId = id });
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"[ERROR] Unauthorized: {ex.Message}");
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"[ERROR] Argument error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Unexpected error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // PUT: api/appointments/{id}/reschedule
        [HttpPut("{id}/reschedule")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Reschedule(
            int id,
            [FromBody] RescheduleAppointmentRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                int userId = GetUserIdFromToken();
                var success = await _appointmentService.RescheduleAppointmentAsync(id, userId, request, cancellationToken);
                if (!success)
                {
                    return NotFound(new { message = "Appointment not found." });
                }

                return Ok(new { message = "Appointment rescheduled successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/appointments/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Doctor,Receptionist,Clinic Manager,Patient")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            [FromBody] UpdateAppointmentStatusRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var success = await _appointmentService.UpdateAppointmentStatusAsync(id, request, cancellationToken);
                if (!success)
                {
                    return NotFound(new { message = "Appointment not found." });
                }

                return Ok(new { message = "Appointment status updated successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/appointments/{id}/confirmation
        [HttpGet("{id}/confirmation")]
        [Authorize]
        public async Task<ActionResult<AppointmentConfirmationDto>> GetConfirmation(int id, CancellationToken cancellationToken)
        {
            var confirmation = await _appointmentService.GetAppointmentConfirmationAsync(id, cancellationToken);
            if (confirmation == null)
            {
                return NotFound(new { message = "Appointment not found." });
            }

            return Ok(confirmation);
        }

        // GET: api/appointments/patient/my-appointments
        [HttpGet("patient/my-appointments")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<List<AppointmentDto>>> GetMyAppointments(CancellationToken cancellationToken)
        {
            try
            {
                int userId = GetUserIdFromToken();
                var patient = await _appointmentRepository.GetPatientByUserIdAsync(userId, cancellationToken);
                if (patient == null)
                {
                    return NotFound(new { message = "Patient not found." });
                }

                var appointments = await _appointmentService.GetByPatientIdAsync(patient.PatientId, cancellationToken);
                return Ok(appointments);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/appointments/doctor/my-appointments
        [HttpGet("doctor/my-appointments")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<List<AppointmentDto>>> GetMyDoctorAppointments(CancellationToken cancellationToken)
        {
            try
            {
                int userId = GetUserIdFromToken();
                var user = await _appointmentService.GetUserByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                // Find doctor by userId
                var doctor = await _appointmentService.GetDoctorByIdAsync(userId, cancellationToken);
                if (doctor == null)
                {
                    return NotFound(new { message = "Doctor not found." });
                }

                var appointments = await _appointmentService.GetByDoctorIdAsync(doctor.DoctorId, cancellationToken);
                return Ok(appointments);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/appointments/receptionist/my-appointments
        [HttpGet("receptionist/my-appointments")]
        [Authorize(Roles = "Receptionist")]
        public async Task<ActionResult<List<AppointmentDto>>> GetMyReceptionistAppointments(CancellationToken cancellationToken)
        {
            try
            {
                int userId = GetUserIdFromToken();
                var user = await _appointmentService.GetUserByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                // Find receptionist by userId
                var receptionist = await _appointmentService.GetReceptionistByUserIdAsync(userId, cancellationToken);
                if (receptionist == null)
                {
                    return NotFound(new { message = "Receptionist not found." });
                }

                var appointments = await _appointmentService.GetByReceptionistIdAsync(receptionist.ReceptionistId, cancellationToken);
                return Ok(appointments);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/appointments/statistics (Manager view)
        [HttpGet("statistics")]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<AppointmentStatisticsDto>> GetStatistics(CancellationToken cancellationToken)
        {
            var statistics = await _appointmentService.GetAppointmentStatisticsAsync(cancellationToken);
            return Ok(statistics);
        }

        // GET: api/appointments/stats/timeseries
        [HttpGet("stats/timeseries")]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<List<AppointmentTimeSeriesPointDto>>> GetTimeSeries(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string groupBy = "day",
            CancellationToken cancellationToken = default)
        {
            var result = await _appointmentService.GetAppointmentTimeSeriesAsync(from, to, groupBy, cancellationToken);
            return Ok(result);
        }

        // GET: api/appointments/stats/heatmap
        [HttpGet("stats/heatmap")]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<List<AppointmentHeatmapPointDto>>> GetHeatmap(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken cancellationToken = default)
        {
            var result = await _appointmentService.GetAppointmentHeatmapAsync(from, to, cancellationToken);
            return Ok(result);
        }

        // DELETE: api/appointments/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Clinic Manager,Receptionist")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var success = await _appointmentService.DeleteAsync(id, cancellationToken);
            if (!success)
            {
                return NotFound(new { message = "Appointment not found." });
            }

            return NoContent();
        }

        #endregion

        #region Doctor Endpoints

        // GET: api/appointments/doctors
        [HttpGet("doctors")]
        [Authorize]
        public async Task<ActionResult<List<DoctorInfoDto>>> GetAllDoctors(CancellationToken cancellationToken)
        {
            var doctors = await _appointmentService.GetAllDoctorsAsync(cancellationToken);
            return Ok(doctors);
        }

        // GET: api/appointments/doctors/{id}
        [HttpGet("doctors/{id}")]
        [Authorize]
        public async Task<ActionResult<DoctorInfoDto>> GetDoctorById(int id, CancellationToken cancellationToken)
        {
            var doctor = await _appointmentService.GetDoctorByIdAsync(id, cancellationToken);
            if (doctor == null)
            {
                return NotFound(new { message = "Doctor not found." });
            }

            return Ok(doctor);
        }

        #endregion

        #region Patient Endpoints

        // GET: api/appointments/patients/{id}
        [HttpGet("patients/{id}")]
        [Authorize(Roles = "Doctor,Receptionist,Clinic Manager")]
        public async Task<ActionResult<PatientInfoDto>> GetPatientById(int id, CancellationToken cancellationToken)
        {
            var patient = await _appointmentService.GetPatientByIdAsync(id, cancellationToken);
            if (patient == null)
            {
                return NotFound(new { message = "Patient not found." });
            }

            return Ok(patient);
        }

        // GET: api/appointments/patients/user/{userId}
        [HttpGet("patients/user/{userId}")]
        [Authorize(Roles = "Doctor,Receptionist,Clinic Manager")]
        public async Task<ActionResult<PatientInfoDto>> GetPatientByUserId(int userId, CancellationToken cancellationToken)
        {
            var patient = await _appointmentService.GetPatientInfoByUserIdAsync(userId, cancellationToken);
            if (patient == null)
            {
                return NotFound(new { message = "Patient not found for this user." });
            }

            return Ok(patient);
        }

        #endregion

        #region Receptionist Endpoints

        // GET: api/appointments/receptionists/{id}
        [HttpGet("receptionists/{id}")]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<ReceptionistInfoDto>> GetReceptionistById(int id, CancellationToken cancellationToken)
        {
            var receptionist = await _appointmentService.GetReceptionistByIdAsync(id, cancellationToken);
            if (receptionist == null)
            {
                return NotFound(new { message = "Receptionist not found." });
            }

            return Ok(receptionist);
        }

        #endregion
    }
}