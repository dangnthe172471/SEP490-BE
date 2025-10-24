using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.BLL.IServices
{
    public interface IDoctorShiftExchangeService
    {
        Task<ShiftSwapRequestResponseDTO> CreateShiftSwapRequestAsync(CreateShiftSwapRequestDTO request);
        Task<List<ShiftSwapRequestResponseDTO>> GetAllRequestsAsync();
        Task<List<ShiftSwapRequestResponseDTO>> GetRequestsByDoctorIdAsync(int doctorId);
        Task<ShiftSwapRequestResponseDTO?> GetRequestByIdAsync(int exchangeId);
        Task<bool> ReviewShiftSwapRequestAsync(ReviewShiftSwapRequestDTO review);
        Task<List<DoctorShiftDTO>> GetDoctorShiftsAsync(int doctorId, DateOnly from, DateOnly to);
        Task<List<DoctorDTO>> GetDoctorsBySpecialtyAsync(string specialty);
        Task<List<DoctorDTO>> GetAllDoctorsAsync();
        Task<List<string>> GetSpecialtiesAsync();
        Task<bool> ValidateShiftSwapRequestAsync(CreateShiftSwapRequestDTO request);
        Task<Doctor?> GetDoctorByIdAsync(int doctorId);
    }
}
