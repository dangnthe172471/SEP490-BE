using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.DTOs.Common;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.Repositories
{
    public class AppointmentDoctorRepository : IAppointmentDoctorRepository
    {
        private readonly DiamondHealthContext _ctx;

        public AppointmentDoctorRepository(DiamondHealthContext ctx)
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

        public async Task<List<Appointment>> GetByDoctorIdAsync(
            int doctorId,
            CancellationToken ct)
        {
            return await _ctx.Appointments
                .AsNoTracking()
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Where(a => a.DoctorId == doctorId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync(ct);
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
