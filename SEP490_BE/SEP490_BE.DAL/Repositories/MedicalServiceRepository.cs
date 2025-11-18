using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.Repositories
{
    public class MedicalServiceRepository : IMedicalServiceRepository
    {
        private readonly DiamondHealthContext _context;

        public MedicalServiceRepository(DiamondHealthContext context)
        {
            _context = context;
        }

        public Task<Service?> GetServiceByCategoryAsync(
            string category,
            CancellationToken ct = default)
        {
            var normalized = category.ToLower();

            return _context.Services
                .Where(s => s.IsActive && s.Category != null)
                .FirstOrDefaultAsync(
                    s => s.Category!.ToLower() == normalized,
                    ct);
        }

        public Task<bool> MedicalServiceExistsAsync(
            int recordId,
            int serviceId,
            CancellationToken ct = default)
        {
            return _context.MedicalServices
                .AnyAsync(ms => ms.RecordId == recordId && ms.ServiceId == serviceId, ct);
        }

        public async Task<MedicalService> CreateMedicalServiceAsync(
            int recordId,
            int serviceId,
            decimal unitPrice,
            string notes,
            CancellationToken ct = default)
        {
            var entity = new MedicalService
            {
                RecordId = recordId,
                ServiceId = serviceId,
                Quantity = 1,
                UnitPrice = unitPrice,
                TotalPrice = unitPrice,
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.MedicalServices.Add(entity);
            await _context.SaveChangesAsync(ct);

            return entity;
        }
    }
}
