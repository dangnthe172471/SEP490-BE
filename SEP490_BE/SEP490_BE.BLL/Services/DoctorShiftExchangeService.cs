using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.BLL.Services
{
    public class DoctorShiftExchangeService : IDoctorShiftExchangeService
    {
        private readonly IDoctorShiftExchangeRepository _repository;

        public DoctorShiftExchangeService(IDoctorShiftExchangeRepository repository)
        {
            _repository = repository;
        }

        public async Task<ShiftSwapRequestResponseDTO> CreateShiftSwapRequestAsync(CreateShiftSwapRequestDTO request)
        {
            // Validate request
            if (!await ValidateShiftSwapRequestAsync(request))
            {
                throw new ArgumentException("Invalid shift swap request");
            }

            var exchange = await _repository.CreateShiftSwapRequestAsync(request);
            var requests = await _repository.GetRequestsByDoctorIdAsync(exchange.Doctor1Id);
            var dto = requests.FirstOrDefault(r => r.ExchangeId == exchange.ExchangeId);
            
            if (dto == null)
            {
                throw new InvalidOperationException("Failed to create shift swap request");
            }

            return dto;
        }

        public async Task<List<ShiftSwapRequestResponseDTO>> GetAllRequestsAsync()
        {
            return await _repository.GetAllRequestsAsync();
        }

        public async Task<List<ShiftSwapRequestResponseDTO>> GetRequestsByDoctorIdAsync(int doctorId)
        {
            return await _repository.GetRequestsByDoctorIdAsync(doctorId);
        }

        public async Task<ShiftSwapRequestResponseDTO?> GetRequestByIdAsync(int exchangeId)
        {
            var requests = await _repository.GetAllRequestsAsync();
            return requests.FirstOrDefault(r => r.ExchangeId == exchangeId);
        }

        public async Task<bool> ReviewShiftSwapRequestAsync(ReviewShiftSwapRequestDTO review)
        {
            // Validate status
            if (review.Status != "Approved" && review.Status != "Rejected")
            {
                throw new ArgumentException("Status must be 'Approved' or 'Rejected'");
            }

            // Check if request exists and is pending
            var request = await _repository.GetRequestByIdAsync(review.ExchangeId);
            if (request == null)
            {
                throw new ArgumentException("Shift swap request not found");
            }

            if (request.Status != "Pending")
            {
                throw new InvalidOperationException("Request has already been processed");
            }

            return await _repository.UpdateRequestStatusAsync(review.ExchangeId, review.Status, null, null);
        }

        public async Task<List<DoctorShiftDTO>> GetDoctorShiftsAsync(int doctorId, DateOnly from, DateOnly to)
        {
            return await _repository.GetDoctorShiftsAsync(doctorId, from, to);
        }

        public async Task<List<DoctorDTO>> GetDoctorsBySpecialtyAsync(string specialty)
        {
            return await _repository.GetDoctorsBySpecialtyAsync(specialty);
        }

        public async Task<List<DoctorDTO>> GetAllDoctorsAsync()
        {
            return await _repository.GetAllDoctorsAsync();
        }

        public async Task<List<string>> GetSpecialtiesAsync()
        {
            return await _repository.GetSpecialtiesAsync();
        }

        public async Task<Doctor?> GetDoctorByIdAsync(int doctorId)
        {
            return await _repository.GetDoctorByIdAsync(doctorId);
        }

        public async Task<bool> ValidateShiftSwapRequestAsync(CreateShiftSwapRequestDTO request)
        {
            // Check if doctors are the same
            if (request.Doctor1Id == request.Doctor2Id)
            {
                return false;
            }

            // Check if doctors have the same specialty
            if (!await _repository.IsSameSpecialtyAsync(request.Doctor1Id, request.Doctor2Id))
            {
                return false;
            }

            // Check if there's already a pending request for the same doctors and date
            if (await _repository.HasPendingRequestAsync(request.Doctor1Id, request.Doctor2Id, request.ExchangeDate))
            {
                return false;
            }

            // Check if both doctors have the shifts they want to swap
            if (request.SwapType?.ToLower() == "permanent")
            {
                // Permanent: Chỉ cho phép đổi ca của tháng sau
                var nextMonthStart = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1));
                
                // Check Doctor1ShiftRefId có EffectiveFrom >= đầu tháng sau
                var doctor1Shift = await _repository.GetDoctorShiftByIdAsync(request.Doctor1ShiftRefId);
                if (doctor1Shift == null || doctor1Shift.EffectiveFrom < nextMonthStart)
                {
                    return false;
                }
                
                // Check Doctor2ShiftRefId có EffectiveFrom >= đầu tháng sau
                var doctor2Shift = await _repository.GetDoctorShiftByIdAsync(request.Doctor2ShiftRefId);
                if (doctor2Shift == null || doctor2Shift.EffectiveFrom < nextMonthStart)
                {
                    return false;
                }
            }
            else
            {
                // Temporary: dùng ExchangeDate để check
                if (!request.ExchangeDate.HasValue)
                {
                    return false;
                }
                
                if (!await _repository.HasExistingShiftAsync(request.Doctor1Id, request.Doctor1ShiftRefId, request.ExchangeDate.Value))
                {
                    return false;
                }

                if (!await _repository.HasExistingShiftAsync(request.Doctor2Id, request.Doctor2ShiftRefId, request.ExchangeDate.Value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
