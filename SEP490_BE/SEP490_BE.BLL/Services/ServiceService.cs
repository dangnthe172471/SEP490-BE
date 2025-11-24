using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _serviceRepository;

        public ServiceService(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<IEnumerable<ServiceDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var services = await _serviceRepository.GetAllAsync(cancellationToken);
            return services.Select(s => new ServiceDto
            {
                ServiceId = s.ServiceId,
                ServiceName = s.ServiceName,
                Description = s.Description,
                Price = s.Price,
                Category = s.Category,
                IsActive = s.IsActive
            });
        }

        public async Task<ServiceDto?> GetByIdAsync(int serviceId, CancellationToken cancellationToken = default)
        {
            var service = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken);
            if (service == null)
            {
                return null;
            }

            return new ServiceDto
            {
                ServiceId = service.ServiceId,
                ServiceName = service.ServiceName,
                Description = service.Description,
                Price = service.Price,
                Category = service.Category,
                IsActive = service.IsActive
            };
        }

        public async Task<PagedResponse<ServiceDto>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var (items, totalCount) = await _serviceRepository.GetPagedAsync(pageNumber, pageSize, searchTerm, cancellationToken);

            return new PagedResponse<ServiceDto>
            {
                Items = items.Select(s => new ServiceDto
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.ServiceName,
                    Description = s.Description,
                    Price = s.Price,
                    Category = s.Category,
                    IsActive = s.IsActive
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<int> CreateAsync(CreateServiceRequest request, CancellationToken cancellationToken = default)
        {
            // Validate service name is required
            if (string.IsNullOrWhiteSpace(request.ServiceName))
            {
                throw new ArgumentException("Tên dịch vụ là bắt buộc.");
            }

            var trimmedName = request.ServiceName.Trim();

            // Validate service name length
            if (trimmedName.Length < 2)
            {
                throw new ArgumentException("Tên dịch vụ phải có ít nhất 2 ký tự.");
            }

            if (trimmedName.Length > 150)
            {
                throw new ArgumentException("Tên dịch vụ không được vượt quá 150 ký tự.");
            }

            // Check duplicate service name
            if (await _serviceRepository.ExistsByNameAsync(trimmedName, null, cancellationToken))
            {
                throw new InvalidOperationException($"Tên dịch vụ '{trimmedName}' đã tồn tại. Vui lòng chọn tên khác.");
            }

            // Validate description length if provided
            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                var trimmedDescription = request.Description.Trim();
                if (trimmedDescription.Length > 500)
                {
                    throw new ArgumentException("Mô tả không được vượt quá 500 ký tự.");
                }
            }

            // Validate price if provided
            if (request.Price.HasValue)
            {
                if (request.Price.Value < 0)
                {
                    throw new ArgumentException("Giá dịch vụ không được âm.");
                }

                if (request.Price.Value > 999999999)
                {
                    throw new ArgumentException("Giá dịch vụ không được vượt quá 999.999.999 VNĐ.");
                }
            }

            var service = new Service
            {
                ServiceName = trimmedName,
                Description = request.Description?.Trim(),
                Price = request.Price,
                Category = "Test", // Always set to "Test" as default
                IsActive = request.IsActive
            };

            await _serviceRepository.AddAsync(service, cancellationToken);
            return service.ServiceId;
        }

        public async Task<ServiceDto?> UpdateAsync(int serviceId, UpdateServiceRequest request, CancellationToken cancellationToken = default)
        {
            var service = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken);
            if (service == null)
            {
                return null;
            }

            // Validate service name is required
            if (string.IsNullOrWhiteSpace(request.ServiceName))
            {
                throw new ArgumentException("Tên dịch vụ là bắt buộc.");
            }

            var trimmedName = request.ServiceName.Trim();

            // Validate service name length
            if (trimmedName.Length < 2)
            {
                throw new ArgumentException("Tên dịch vụ phải có ít nhất 2 ký tự.");
            }

            if (trimmedName.Length > 150)
            {
                throw new ArgumentException("Tên dịch vụ không được vượt quá 150 ký tự.");
            }

            // Check duplicate service name (excluding current service)
            if (await _serviceRepository.ExistsByNameAsync(trimmedName, serviceId, cancellationToken))
            {
                throw new InvalidOperationException($"Tên dịch vụ '{trimmedName}' đã tồn tại. Vui lòng chọn tên khác.");
            }

            // Validate description length if provided
            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                var trimmedDescription = request.Description.Trim();
                if (trimmedDescription.Length > 500)
                {
                    throw new ArgumentException("Mô tả không được vượt quá 500 ký tự.");
                }
            }

            // Validate price if provided
            if (request.Price.HasValue)
            {
                if (request.Price.Value < 0)
                {
                    throw new ArgumentException("Giá dịch vụ không được âm.");
                }

                if (request.Price.Value > 999999999)
                {
                    throw new ArgumentException("Giá dịch vụ không được vượt quá 999.999.999 VNĐ.");
                }
            }

            service.ServiceName = trimmedName;
            service.Description = request.Description?.Trim();
            service.Price = request.Price;
            service.Category = "Test"; // Always set to "Test" when updating
            service.IsActive = request.IsActive;

            await _serviceRepository.UpdateAsync(service, cancellationToken);

            return new ServiceDto
            {
                ServiceId = service.ServiceId,
                ServiceName = service.ServiceName,
                Description = service.Description,
                Price = service.Price,
                Category = service.Category,
                IsActive = service.IsActive
            };
        }

        public async Task<bool> DeleteAsync(int serviceId, CancellationToken cancellationToken = default)
        {
            try
            {
                await _serviceRepository.DeleteAsync(serviceId, cancellationToken);
                return true;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch
            {
                return false;
            }
        }
    }
}

