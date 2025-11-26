using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly DiamondHealthContext _dbContext;

        public ServiceRepository(DiamondHealthContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Service>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Services
                .AsNoTracking()
                .OrderBy(s => s.ServiceName)
                .ToListAsync(cancellationToken);
        }

        public async Task<Service?> GetByIdAsync(int serviceId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Services
                .Include(s => s.MedicalServices)
                .Include(s => s.TestResults)
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId, cancellationToken);
        }

        public async Task<(List<Service> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Services.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(s => 
                    s.ServiceName.ToLower().Contains(term) ||
                    (s.Description != null && s.Description.ToLower().Contains(term)) ||
                    (s.Category != null && s.Category.ToLower().Contains(term))
                );
            }

            // Get total count before paging
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply paging
            var items = await query
                .AsNoTracking()
                .OrderBy(s => s.ServiceName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<bool> ExistsByNameAsync(string serviceName, int? excludeServiceId = null, CancellationToken cancellationToken = default)
        {
            var normalizedName = serviceName.Trim().ToLower();
            var query = _dbContext.Services
                .Where(s => s.ServiceName.ToLower() == normalizedName);

            if (excludeServiceId.HasValue)
            {
                query = query.Where(s => s.ServiceId != excludeServiceId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task AddAsync(Service service, CancellationToken cancellationToken = default)
        {
            await _dbContext.Services.AddAsync(service, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Service service, CancellationToken cancellationToken = default)
        {
            _dbContext.Services.Update(service);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int serviceId, CancellationToken cancellationToken = default)
        {
            var service = await _dbContext.Services
                .Include(s => s.MedicalServices)
                .Include(s => s.TestResults)
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId, cancellationToken);

            if (service != null)
            {
                // Check if service has related records
                if (service.MedicalServices.Any())
                {
                    throw new InvalidOperationException($"Cannot delete service '{service.ServiceName}' because it has {service.MedicalServices.Count} medical service record(s).");
                }

                if (service.TestResults.Any())
                {
                    throw new InvalidOperationException($"Cannot delete service '{service.ServiceName}' because it has {service.TestResults.Count} test result record(s).");
                }

                _dbContext.Services.Remove(service);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

