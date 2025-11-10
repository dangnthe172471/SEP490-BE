using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.DTOs;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IDoctorShiftExchangeRepository
    {
        Task<DoctorShiftExchange> CreateShiftSwapRequestAsync(CreateShiftSwapRequestDTO request);
        Task<List<ShiftSwapRequestResponseDTO>> GetAllRequestsAsync();
        Task<List<ShiftSwapRequestResponseDTO>> GetRequestsByDoctorIdAsync(int doctorId);
        Task<DoctorShiftExchange?> GetRequestByIdAsync(int exchangeId);
        Task<bool> UpdateRequestStatusAsync(int exchangeId, string status, string? managerNote, int? reviewedBy);
        Task<bool> IsSameSpecialtyAsync(int doctor1Id, int doctor2Id);
        Task<bool> HasExistingShiftAsync(int doctorId, int doctorShiftId, DateOnly date);
        Task<bool> HasPendingRequestAsync(int doctor1Id, int doctor2Id, DateOnly? exchangeDate);
        Task<DoctorShift?> GetDoctorShiftByIdAsync(int doctorShiftId);
        Task<List<DoctorShiftDTO>> GetDoctorShiftsAsync(int doctorId, DateOnly from, DateOnly to);
        Task<List<DoctorDTO>> GetDoctorsBySpecialtyAsync(string specialty);
        Task<List<DoctorDTO>> GetAllDoctorsAsync();
        Task<List<string>> GetSpecialtiesAsync();
        Task<Doctor?> GetDoctorByIdAsync(int doctorId);
    }
}
