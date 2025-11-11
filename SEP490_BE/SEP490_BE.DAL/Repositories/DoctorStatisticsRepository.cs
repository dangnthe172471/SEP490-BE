using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.DTOs.DoctorStatisticsDTO;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.Repositories
{
    public class DoctorStatisticsRepository : IDoctorStatisticsRepository
    {
        private readonly DiamondHealthContext _context;

        public DoctorStatisticsRepository(DiamondHealthContext context)
        {
            _context = context;
        }

        // Chart 1: Số lượng bệnh nhân theo bác sĩ
        public async Task<List<DoctorPatientCountDto>> GetPatientCountByDoctorAsync(DateTime fromDate, DateTime toDate)
        {
            var completedStatus = "Confirmed";

            var query =
                from d in _context.Doctors
                join u in _context.Users on d.UserId equals u.UserId
                join a in _context.Appointments
                    .Where(a => a.AppointmentDate >= fromDate &&
                                a.AppointmentDate <= toDate &&
                                a.Status == completedStatus)
                    on d.DoctorId equals a.DoctorId into da
                from aGroup in da.DefaultIfEmpty()
                group aGroup by new { d.DoctorId, u.FullName, d.Specialty }
                into g
                select new DoctorPatientCountDto
                {
                    DoctorId = g.Key.DoctorId,
                    DoctorName = g.Key.FullName,
                    Specialty = g.Key.Specialty,
                    TotalAppointments = g.Count(x => x != null),
                    TotalPatients = g.Where(x => x != null)
                                     .Select(x => x.PatientId)
                                     .Distinct()
                                     .Count()
                };

            return await query
                .OrderByDescending(x => x.TotalAppointments)
                .ToListAsync();
        }

        // Chart 2: Xu hướng số ca khám theo thời gian
        public async Task<List<DoctorVisitTrendPointDto>> GetDoctorVisitTrendAsync(DateTime fromDate, DateTime toDate, int? doctorId = null)
        {
            var completedStatus = "Confirmed";

            var query =
                from a in _context.Appointments
                join d in _context.Doctors on a.DoctorId equals d.DoctorId
                join u in _context.Users on d.UserId equals u.UserId
                where a.AppointmentDate >= fromDate &&
                      a.AppointmentDate <= toDate &&
                      a.Status == completedStatus &&
                      (!doctorId.HasValue || a.DoctorId == doctorId.Value)
                group a by new
                {
                    a.DoctorId,
                    u.FullName,
                    Date = a.AppointmentDate.Date
                }
                into g
                select new DoctorVisitTrendPointDto
                {
                    DoctorId = g.Key.DoctorId,
                    DoctorName = g.Key.FullName,
                    Date = g.Key.Date,
                    VisitCount = g.Count()
                };

            return await query
                .OrderBy(x => x.Date)
                .ThenBy(x => x.DoctorName)
                .ToListAsync();
        }

        // Chart 3: Tỷ lệ bệnh nhân tái khám theo bác sĩ
        public async Task<List<DoctorReturnRateDto>> GetDoctorReturnRatesAsync(DateTime fromDate, DateTime toDate)
        {
            var completedStatus = "Confirmed";

            var baseQuery =
                from a in _context.Appointments
                join d in _context.Doctors on a.DoctorId equals d.DoctorId
                join u in _context.Users on d.UserId equals u.UserId
                where a.AppointmentDate >= fromDate &&
                      a.AppointmentDate <= toDate &&
                      a.Status == completedStatus
                select new
                {
                    a.DoctorId,
                    DoctorName = u.FullName,
                    a.PatientId
                };

            var grouped = await baseQuery
                .GroupBy(x => new { x.DoctorId, x.DoctorName })
                .Select(g => new
                {
                    g.Key.DoctorId,
                    g.Key.DoctorName,
                    TotalPatients = g.Select(x => x.PatientId).Distinct().Count(),
                    ReturnPatients = g.GroupBy(x => x.PatientId)
                                      .Count(pg => pg.Count() >= 2)
                })
                .ToListAsync();

            var result = grouped
                .Select(x => new DoctorReturnRateDto
                {
                    DoctorId = x.DoctorId,
                    DoctorName = x.DoctorName,
                    TotalPatients = x.TotalPatients,
                    ReturnPatients = x.ReturnPatients,
                    ReturnRate = x.TotalPatients == 0
                        ? 0
                        : Math.Round((double)x.ReturnPatients * 100 / x.TotalPatients, 2)
                })
                .OrderByDescending(x => x.ReturnRate)
                .ToList();

            return result;
        }
    }
}
