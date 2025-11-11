using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.DoctorStatisticsDTO;

namespace SEP490_BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorStatisticsController : ControllerBase
    {
        private readonly IDoctorStatisticsService _doctorStatisticsService;

        public DoctorStatisticsController(IDoctorStatisticsService doctorStatisticsService)
        {
            _doctorStatisticsService = doctorStatisticsService;
        }

        /// Chart 1:
        /// Số lượng bệnh nhân & lượt khám theo từng bác sĩ trong khoảng thời gian.
        /// Dùng để so sánh khối lượng công việc giữa các bác sĩ.
        [HttpGet("patient-count")]
        public async Task<ActionResult<List<DoctorPatientCountDto>>> GetPatientCountByDoctor(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            var data = await _doctorStatisticsService.GetPatientCountByDoctorAsync(fromDate, toDate);
            return Ok(data);
        }

        /// Chart 2:
        /// Xu hướng số ca khám theo thời gian.
        /// Dùng để theo dõi hoạt động khám chữa bệnh theo ngày/tuần/tháng.
        [HttpGet("visit-trend")]
        public async Task<ActionResult<List<DoctorVisitTrendPointDto>>> GetDoctorVisitTrend(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] int? doctorId = null)
        {
            var data = await _doctorStatisticsService.GetDoctorVisitTrendAsync(fromDate, toDate, doctorId);
            return Ok(data);
        }

        /// Chart 3:
        /// Tỷ lệ bệnh nhân tái khám theo từng bác sĩ.
        /// Dùng để đo mức độ quay lại/tin tưởng của bệnh nhân.
        [HttpGet("return-rate")]
        public async Task<ActionResult<List<DoctorReturnRateDto>>> GetDoctorReturnRates(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            var data = await _doctorStatisticsService.GetDoctorReturnRatesAsync(fromDate, toDate);
            return Ok(data);
        }

        /// Trả về cả 3 loại dữ liệu (patient-count, visit-trend, return-rate) cho dashboard.
        [HttpGet("summary")]
        public async Task<ActionResult<DoctorStatisticsSummaryDto>> GetDoctorStatisticsSummary(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] int? doctorId = null)
        {
            var data = await _doctorStatisticsService.GetDoctorStatisticsSummaryAsync(fromDate, toDate, doctorId);
            return Ok(data);
        }
    }
}
