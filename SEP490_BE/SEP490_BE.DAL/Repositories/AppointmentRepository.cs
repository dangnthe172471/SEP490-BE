using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly DiamondHealthContext _ctx;

        public AppointmentRepository(DiamondHealthContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<int?> GetDoctorIdByUserIdAsync(int userId, CancellationToken ct)
        {
            return await _ctx.Doctors
                .AsNoTracking()
                .Where(d => d.UserId == userId)
                .Select(d => (int?)d.DoctorId)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<PagedResult<Appointment>> GetByDoctorIdAsync(
            int doctorId,
            DateTime? from,
            DateTime? to,
            string? status,
            int pageNumber,
            int pageSize,
            CancellationToken ct)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 20;

            var q = _ctx.Appointments
                .AsNoTracking()
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Where(a => a.DoctorId == doctorId);

            if (from.HasValue) q = q.Where(a => a.AppointmentDate >= from.Value);
            if (to.HasValue) q = q.Where(a => a.AppointmentDate <= to.Value);
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(a => a.Status == status);

            q = q.OrderByDescending(a => a.AppointmentDate);

            var total = await q.CountAsync(ct);

            var items = await q
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<Appointment>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<Appointment?> GetDetailForDoctorAsync(
            int doctorId,
            int appointmentId,
            CancellationToken ct)
        {
            return await _ctx.Appointments
                .AsNoTracking()
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Where(a => a.DoctorId == doctorId && a.AppointmentId == appointmentId)
                .FirstOrDefaultAsync(ct);
        }
    }
}
