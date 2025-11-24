using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.Dashboard;

namespace SEP490_BE.API.Controllers.Dashboard
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("clinic-status")]
        [Authorize(Roles = "Receptionist")]
        public async Task<ActionResult<ClinicStatusDto>> GetClinicStatus([FromQuery] DateOnly? date, CancellationToken cancellationToken)
        {
            var d = date ?? DateOnly.FromDateTime(DateTime.Today);
            var result = await _dashboardService.GetClinicStatusAsync(d, cancellationToken);
            return Ok(result);
        }

        [HttpGet("patient-statistics")]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<PatientStatisticsDto>> GetPatientStatistics([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken)
        {
            var toDate = to ?? DateOnly.FromDateTime(DateTime.Today);
            var fromDate = from ?? toDate.AddMonths(-11).AddDays(1 - toDate.Day);

            if (fromDate > toDate)
            {
                return BadRequest(new { message = "Thời gian 'từ' phải nhỏ hơn hoặc bằng 'đến'." });
            }

            var result = await _dashboardService.GetPatientStatisticsAsync(fromDate, toDate, cancellationToken);
            return Ok(result);
        }

        [HttpGet("test-diagnostic-stats")]
        [Authorize(Roles = "Clinic Manager,Administrator")]
        public async Task<ActionResult<TestDiagnosticStatsDto>> GetTestDiagnosticStats(
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to,
            [FromQuery] string? groupBy,
            CancellationToken cancellationToken)
        {
            var toDate = to ?? DateOnly.FromDateTime(DateTime.Today);
            var fromDate = from ?? toDate.AddDays(-29);

            if (fromDate > toDate)
            {
                return BadRequest(new { message = "'from' phải nhỏ hơn hoặc bằng 'to'." });
            }

            var normalizedGroupBy = string.Equals(groupBy, "month", StringComparison.OrdinalIgnoreCase) ? "month" : "day";
            var result = await _dashboardService.GetTestDiagnosticStatsAsync(fromDate, toDate, normalizedGroupBy, cancellationToken);
            return Ok(result);
        }
    }
}



