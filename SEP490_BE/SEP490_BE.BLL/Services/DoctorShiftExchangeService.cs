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
            
            // Get the created request with full details
            var result = await _repository.GetRequestByIdAsync(exchange.ExchangeId);
            if (result == null)
            {
                throw new InvalidOperationException("Failed to create shift swap request");
            }

            return new ShiftSwapRequestResponseDTO
            {
                ExchangeId = result.ExchangeId,
                Doctor1Id = result.Doctor1Id,
                Doctor1Name = result.Doctor1?.User?.FullName ?? "",
                Doctor1Specialty = result.Doctor1?.Specialty ?? "",
                Doctor2Id = result.Doctor2Id ?? 0,
                Doctor2Name = result.Doctor2?.User?.FullName ?? "",
                Doctor2Specialty = result.Doctor2?.Specialty ?? "",
                Doctor1ShiftRefId = result.Doctor1ShiftRefId,
                Doctor1ShiftName = result.Doctor1ShiftRef?.Shift?.ShiftType ?? "",
                Doctor2ShiftRefId = result.Doctor2ShiftRefId ?? 0,
                Doctor2ShiftName = result.Doctor2ShiftRef?.Shift?.ShiftType ?? "",
                ExchangeDate = result.ExchangeDate,
                Status = result.Status ?? "Pending",
                SwapType = result.SwapType ?? "Temporary"
            };
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
            var result = await _repository.GetRequestByIdAsync(exchangeId);
            if (result == null)
                return null;

            return new ShiftSwapRequestResponseDTO
            {
                ExchangeId = result.ExchangeId,
                Doctor1Id = result.Doctor1Id,
                Doctor1Name = result.Doctor1?.User?.FullName ?? "",
                Doctor1Specialty = result.Doctor1?.Specialty ?? "",
                Doctor2Id = result.Doctor2Id ?? 0,
                Doctor2Name = result.Doctor2?.User?.FullName ?? "",
                Doctor2Specialty = result.Doctor2?.Specialty ?? "",
                Doctor1ShiftRefId = result.Doctor1ShiftRefId,
                Doctor1ShiftName = result.Doctor1ShiftRef?.Shift?.ShiftType ?? "",
                Doctor2ShiftRefId = result.Doctor2ShiftRefId ?? 0,
                Doctor2ShiftName = result.Doctor2ShiftRef?.Shift?.ShiftType ?? "",
                ExchangeDate = result.ExchangeDate,
                Status = result.Status ?? "Pending",
                SwapType = result.SwapType ?? "Temporary"
            };
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

            // Update status và xử lý đổi ca (nếu chấp nhận)
            var result = await _repository.UpdateRequestStatusAsync(
                review.ExchangeId, 
                review.Status, 
                null, 
                null
            );

            if (result && review.Status == "Approved")
            {
                // Log thành công đổi ca
                Console.WriteLine($"Successfully processed shift swap for exchange {review.ExchangeId}");
            }

            return result;
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
            if (!await _repository.HasExistingShiftAsync(request.Doctor1Id, request.Doctor1ShiftRefId, request.ExchangeDate))
            {
                return false;
            }

            if (!await _repository.HasExistingShiftAsync(request.Doctor2Id, request.Doctor2ShiftRefId, request.ExchangeDate))
            {
                return false;
            }

            return true;
        }

    }
}
