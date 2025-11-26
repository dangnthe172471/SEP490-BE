using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.DTOs.Dashboard;
using SEP490_BE.DAL.IRepositories.Dashboard;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.Repositories.Dashboard
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly DiamondHealthContext _db;

        public DashboardRepository(DiamondHealthContext db)
        {
            _db = db;
        }

        public async Task<ClinicStatusDto> GetClinicStatusAsync(DateOnly date, CancellationToken cancellationToken = default)
        {
            var start = date.ToDateTime(TimeOnly.MinValue);
            var end = date.ToDateTime(new TimeOnly(23, 59, 59));

            var statuses = await _db.Appointments.AsNoTracking()
                .Where(a => a.AppointmentDate >= start && a.AppointmentDate <= end)
                .Select(a => a.Status ?? "")
                .ToListAsync(cancellationToken);

            int total = statuses.Count;
            int pending = statuses.Count(s => s == "Pending");
            int confirmed = statuses.Count(s => s == "Confirmed");
            int completed = statuses.Count(s => s == "Completed");
            int cancelled = statuses.Count(s => s == "Cancelled");

            var firstAppointmentsToday = await _db.Appointments
                .AsNoTracking()
                .GroupBy(a => a.PatientId)
                .Select(g => g.Min(a => a.AppointmentDate))
                .Where(minDate => minDate >= start && minDate <= end)
                .CountAsync(cancellationToken);

            return new ClinicStatusDto
            {
                Date = date,
                TodayNewPatients = firstAppointmentsToday,
                Appointments = new ClinicStatusDto.AppointmentCounters
                {
                    Total = total,
                    Pending = pending,
                    Confirmed = confirmed,
                    Completed = completed,
                    Cancelled = cancelled
                }
            };
        }

        public async Task<PatientStatisticsDto> GetPatientStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        {
            var start = from.ToDateTime(TimeOnly.MinValue);
            var end = to.ToDateTime(new TimeOnly(23, 59, 59));

            var patients = _db.Patients.Include(p => p.User).AsNoTracking();

            int totalPatients = await patients.CountAsync(cancellationToken);

            var genders = await patients.Select(p => (p.User.Gender ?? "").Trim().ToLower())
                .ToListAsync(cancellationToken);
            int male = genders.Count(g => g == "nam" || g == "male" || g == "m");
            int female = genders.Count(g => g == "nữ" || g == "nu" || g == "female" || g == "f");
            int other = Math.Max(0, genders.Count - male - female);

            var today = DateOnly.FromDateTime(DateTime.Today);
            var dobs = await patients.Select(p => p.User.Dob).ToListAsync(cancellationToken);
            int g0_17 = dobs.Count(d => d.HasValue && GetAge(d.Value, today) <= 17);
            int g18_35 = dobs.Count(d => d.HasValue && GetAge(d.Value, today) >= 18 && GetAge(d.Value, today) <= 35);
            int g36_55 = dobs.Count(d => d.HasValue && GetAge(d.Value, today) >= 36 && GetAge(d.Value, today) <= 55);
            int g56p = dobs.Count(d => d.HasValue && GetAge(d.Value, today) >= 56);

            // Monthly appointments (count all appointments per month in range), only months that have data
            var monthlyRaw = await _db.Appointments
                .AsNoTracking()
                .Where(a => a.AppointmentDate >= start && a.AppointmentDate <= end)
                .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync(cancellationToken);

            var monthly = monthlyRaw
                .Select(x => new PatientStatisticsDto.MonthlyCount
                {
                    Month = $"{x.Year:D4}-{x.Month:D2}",
                    Count = x.Count
                })
                .ToList();

            return new PatientStatisticsDto
            {
                TotalPatients = totalPatients,
                ByGender = new PatientStatisticsDto.GenderCounters
                {
                    Male = male,
                    Female = female,
                    Other = other
                },
                ByAgeGroups = new PatientStatisticsDto.AgeGroupCounters
                {
                    _0_17 = g0_17,
                    _18_35 = g18_35,
                    _36_55 = g36_55,
                    _56_Plus = g56p
                },
                MonthlyNewPatients = monthly
            };
        }

        public async Task<TestDiagnosticStatsDto> GetTestDiagnosticStatsAsync(
            DateOnly from,
            DateOnly to,
            string groupBy,
            CancellationToken cancellationToken = default)
        {
            var start = from.ToDateTime(TimeOnly.MinValue);
            var end = to.ToDateTime(new TimeOnly(23, 59, 59));
            var normalizedGroupBy = string.Equals(groupBy, "month", StringComparison.OrdinalIgnoreCase) ? "month" : "day";

            var visitQuery = _db.MedicalServices
                .AsNoTracking()
                .Where(ms => ms.Record.Appointment.AppointmentDate >= start &&
                             ms.Record.Appointment.AppointmentDate <= end);

            var visitServices = visitQuery.Where(ms => ms.Service.Category != null && ms.Service.Category != "Test");

            var visitTypeCounts = await visitServices
                .GroupBy(ms => ms.Service.Category ?? "Khác")
                .Select(g => new CategoryCountDto
                {
                    Label = g.Key,
                    Count = g.Sum(ms => ms.Quantity)
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync(cancellationToken);

            var topVisitServices = await visitServices
                .GroupBy(ms => ms.Service.ServiceName)
                .Select(g => new CategoryCountDto
                {
                    Label = g.Key,
                    Count = g.Sum(ms => ms.Quantity),
                    Revenue = g.Sum(ms => ms.TotalPrice ?? (ms.Quantity * ms.UnitPrice))
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync(cancellationToken);

            var visitSeries = normalizedGroupBy == "month"
                ? await visitServices
                    .Select(ms => new { ms.Record.Appointment.AppointmentDate, ms.Quantity })
                    .GroupBy(x => new { x.AppointmentDate.Year, x.AppointmentDate.Month })
                    .Select(g => new
                    {
                        Period = $"{g.Key.Year:D4}-{g.Key.Month:D2}",
                        Count = g.Sum(x => x.Quantity)
                    })
                    .ToListAsync(cancellationToken)
                : await visitServices
                    .Select(ms => new { Date = ms.Record.Appointment.AppointmentDate.Date, ms.Quantity })
                    .GroupBy(x => x.Date)
                    .Select(g => new
                    {
                        Period = g.Key.ToString("yyyy-MM-dd"),
                        Count = g.Sum(x => x.Quantity)
                    })
                    .ToListAsync(cancellationToken);

            var visitTotal = await visitServices.SumAsync(ms => (int?)ms.Quantity ?? 0, cancellationToken);

            var testQuery = _db.TestResults
                .AsNoTracking()
                .Where(tr => tr.Record.Appointment.AppointmentDate >= start &&
                             tr.Record.Appointment.AppointmentDate <= end);

            var testTypeCounts = await testQuery
                .GroupBy(tr => tr.Service.ServiceName)
                .Select(g => new CategoryCountDto
                {
                    Label = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync(cancellationToken);

            var testCategoryCounts = await testQuery
                .GroupBy(tr => tr.Service.Category ?? "Khác")
                .Select(g => new CategoryCountDto
                {
                    Label = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync(cancellationToken);

            var topTestServices = testTypeCounts.Take(5).ToList();

            var testSeries = normalizedGroupBy == "month"
                ? await testQuery
                    .Select(tr => tr.Record.Appointment.AppointmentDate)
                    .GroupBy(date => new { date.Year, date.Month })
                    .Select(g => new
                    {
                        Period = $"{g.Key.Year:D4}-{g.Key.Month:D2}",
                        Count = g.Count()
                    })
                    .ToListAsync(cancellationToken)
                : await testQuery
                    .Select(tr => tr.Record.Appointment.AppointmentDate.Date)
                    .GroupBy(date => date)
                    .Select(g => new
                    {
                        Period = g.Key.ToString("yyyy-MM-dd"),
                        Count = g.Count()
                    })
                    .ToListAsync(cancellationToken);

            var testTotal = await testQuery.CountAsync(cancellationToken);

            var trendMap = new Dictionary<string, DiagnosticTrendPointDto>(StringComparer.Ordinal);
            foreach (var item in visitSeries)
            {
                if (!trendMap.TryGetValue(item.Period, out var point))
                {
                    point = new DiagnosticTrendPointDto { Period = item.Period };
                    trendMap[item.Period] = point;
                }
                point.VisitCount = item.Count;
            }

            foreach (var item in testSeries)
            {
                if (!trendMap.TryGetValue(item.Period, out var point))
                {
                    point = new DiagnosticTrendPointDto { Period = item.Period };
                    trendMap[item.Period] = point;
                }
                point.TestCount = item.Count;
            }

            var orderedTrends = trendMap.Values
                .OrderBy(p => ParsePeriod(p.Period, normalizedGroupBy))
                .ToList();

            return new TestDiagnosticStatsDto
            {
                TotalVisits = visitTotal,
                TotalTests = testTotal,
                VisitTypeCounts = visitTypeCounts,
                TestTypeCounts = testCategoryCounts,
                TopVisitServices = topVisitServices,
                TopTestServices = topTestServices,
                Trends = orderedTrends
            };
        }

        private static int GetAge(DateOnly dob, DateOnly today)
        {
            int age = today.Year - dob.Year;
            if (today < dob.AddYears(age)) age--;
            return age;
        }

        private static DateTime ParsePeriod(string period, string groupBy)
        {
            if (string.Equals(groupBy, "month", StringComparison.OrdinalIgnoreCase))
            {
                return DateTime.ParseExact($"{period}-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            return DateTime.ParseExact(period, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}



