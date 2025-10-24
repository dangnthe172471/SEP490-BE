using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.DTOs;

namespace SEP490_BE.DAL.Repositories
{
    public class DoctorShiftExchangeRepository : IDoctorShiftExchangeRepository
    {
        private readonly DiamondHealthContext _context;

        public DoctorShiftExchangeRepository(DiamondHealthContext context)
        {
            _context = context;
        }

        public async Task<DoctorShiftExchange> CreateShiftSwapRequestAsync(CreateShiftSwapRequestDTO request)
        {
            // Validate: Chỉ được đổi lịch từ ngày mai trở đi
            var tomorrow = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
            if (request.ExchangeDate < tomorrow)
            {
                throw new ArgumentException("Chỉ được đổi lịch từ ngày mai trở đi. Không thể đổi lịch hôm nay.");
            }

            var exchange = new DoctorShiftExchange
            {
                Doctor1Id = request.Doctor1Id,
                Doctor2Id = request.Doctor2Id,
                Doctor1ShiftRefId = request.Doctor1ShiftRefId,
                Doctor2ShiftRefId = request.Doctor2ShiftRefId,
                ExchangeDate = request.ExchangeDate,
                Status = "Pending",
                SwapType = request.SwapType
            };

            _context.DoctorShiftExchanges.Add(exchange);
            await _context.SaveChangesAsync();
            return exchange;
        }


        public async Task<List<ShiftSwapRequestResponseDTO>> GetAllRequestsAsync()
        {
            return await _context.DoctorShiftExchanges
                .Include(x => x.Doctor1)
                    .ThenInclude(d => d.User)
                .Include(x => x.Doctor2)
                    .ThenInclude(d => d.User)
                .Include(x => x.Doctor1ShiftRef)
                    .ThenInclude(s => s.Shift)
                .Include(x => x.Doctor2ShiftRef)
                    .ThenInclude(s => s.Shift)
                .Select(x => new ShiftSwapRequestResponseDTO
                {
                    ExchangeId = x.ExchangeId,
                    Doctor1Id = x.Doctor1Id,
                    Doctor1Name = x.Doctor1.User.FullName,
                    Doctor1Specialty = x.Doctor1.Specialty,
                    Doctor2Id = x.Doctor2Id ?? 0,
                    Doctor2Name = x.Doctor2 != null ? x.Doctor2.User.FullName : "",
                    Doctor2Specialty = x.Doctor2 != null ? x.Doctor2.Specialty : "",
                    Doctor1ShiftRefId = x.Doctor1ShiftRefId,
                    Doctor1ShiftName = x.Doctor1ShiftRef.Shift.ShiftType,
                    Doctor2ShiftRefId = x.Doctor2ShiftRefId ?? 0,
                    Doctor2ShiftName = x.Doctor2ShiftRef != null ? x.Doctor2ShiftRef.Shift.ShiftType : "",
                    ExchangeDate = x.ExchangeDate,
                    Status = x.Status ?? "Pending",
                    SwapType = x.SwapType ?? "Temporary"
                })
                .OrderByDescending(x => x.ExchangeDate)
                .ToListAsync();
        }

        public async Task<List<ShiftSwapRequestResponseDTO>> GetRequestsByDoctorIdAsync(int doctorId)
        {
            return await _context.DoctorShiftExchanges
                .Where(x => x.Doctor1Id == doctorId || x.Doctor2Id == doctorId)
                .Include(x => x.Doctor1)
                    .ThenInclude(d => d.User)
                .Include(x => x.Doctor2)
                    .ThenInclude(d => d.User)
                .Include(x => x.Doctor1ShiftRef)
                    .ThenInclude(s => s.Shift)
                .Include(x => x.Doctor2ShiftRef)
                    .ThenInclude(s => s.Shift)
                .Select(x => new ShiftSwapRequestResponseDTO
                {
                    ExchangeId = x.ExchangeId,
                    Doctor1Id = x.Doctor1Id,
                    Doctor1Name = x.Doctor1.User.FullName,
                    Doctor1Specialty = x.Doctor1.Specialty,
                    Doctor2Id = x.Doctor2Id ?? 0,
                    Doctor2Name = x.Doctor2 != null ? x.Doctor2.User.FullName : "",
                    Doctor2Specialty = x.Doctor2 != null ? x.Doctor2.Specialty : "",
                    Doctor1ShiftRefId = x.Doctor1ShiftRefId,
                    Doctor1ShiftName = x.Doctor1ShiftRef.Shift.ShiftType,
                    Doctor2ShiftRefId = x.Doctor2ShiftRefId ?? 0,
                    Doctor2ShiftName = x.Doctor2ShiftRef != null ? x.Doctor2ShiftRef.Shift.ShiftType : "",
                    ExchangeDate = x.ExchangeDate,
                    Status = x.Status ?? "Pending",
                    SwapType = x.SwapType ?? "Temporary"
                })
                .ToListAsync();
        }

        public async Task<DoctorShiftExchange?> GetRequestByIdAsync(int exchangeId)
        {
            return await _context.DoctorShiftExchanges
                .Include(x => x.Doctor1)
                    .ThenInclude(d => d.User)
                .Include(x => x.Doctor2)
                    .ThenInclude(d => d.User)
                .Include(x => x.Doctor1ShiftRef)
                    .ThenInclude(s => s.Shift)
                .Include(x => x.Doctor2ShiftRef)
                    .ThenInclude(s => s.Shift)
                .FirstOrDefaultAsync(x => x.ExchangeId == exchangeId);
        }

        public async Task<bool> UpdateRequestStatusAsync(int exchangeId, string status, string? managerNote, int? reviewedBy)
        {
            var exchange = await _context.DoctorShiftExchanges
                .Include(x => x.Doctor1ShiftRef)
                .Include(x => x.Doctor2ShiftRef)
                .FirstOrDefaultAsync(x => x.ExchangeId == exchangeId);

            if (exchange == null) return false;

            exchange.Status = status;

            // Nếu chấp nhận, thực hiện đổi ca
            if (status == "Approved")
            {
                await ProcessShiftSwapAsync(exchange);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task ProcessShiftSwapAsync(DoctorShiftExchange exchange)
        {
            // Lấy thông tin ca hiện tại của 2 bác sĩ
            var doctor1Shift = await _context.DoctorShifts
                .FirstOrDefaultAsync(x => x.DoctorShiftId == exchange.Doctor1ShiftRefId);
            
            var doctor2Shift = await _context.DoctorShifts
                .FirstOrDefaultAsync(x => x.DoctorShiftId == exchange.Doctor2ShiftRefId);

            if (doctor1Shift == null || doctor2Shift == null) return;

            // Xử lý theo loại đổi ca
            if (exchange.SwapType == "permanent")
            {
                // Đổi ca vĩnh viễn: Cập nhật lịch cũ thành NotActive
                doctor1Shift.Status = "NotActive";
                doctor2Shift.Status = "NotActive";
                
                // Tạo lịch từ hôm nay đến ngày đổi (tạm thời)
                var tempDoctor1Shift = new DoctorShift
                {
                    DoctorId = doctor1Shift.DoctorId,
                    ShiftId = doctor2Shift.ShiftId, // Bác sĩ 1 làm ca của bác sĩ 2
                    EffectiveFrom = DateOnly.FromDateTime(DateTime.Now),
                    EffectiveTo = exchange.ExchangeDate.AddDays(-1),
                    Status = "Active"
                };

                var tempDoctor2Shift = new DoctorShift
                {
                    DoctorId = doctor2Shift.DoctorId,
                    ShiftId = doctor1Shift.ShiftId, // Bác sĩ 2 làm ca của bác sĩ 1
                    EffectiveFrom = DateOnly.FromDateTime(DateTime.Now),
                    EffectiveTo = exchange.ExchangeDate.AddDays(-1),
                    Status = "Active"
                };

                // Tạo lịch từ ngày đổi trở đi (vĩnh viễn)
                var newDoctor1Shift = new DoctorShift
                {
                    DoctorId = doctor1Shift.DoctorId,
                    ShiftId = doctor2Shift.ShiftId, // Bác sĩ 1 làm ca của bác sĩ 2
                    EffectiveFrom = exchange.ExchangeDate,
                    EffectiveTo = doctor2Shift.EffectiveTo, // Giữ nguyên EffectiveTo của lịch cũ
                    Status = "Active"
                };

                var newDoctor2Shift = new DoctorShift
                {
                    DoctorId = doctor2Shift.DoctorId,
                    ShiftId = doctor1Shift.ShiftId, // Bác sĩ 2 làm ca của bác sĩ 1
                    EffectiveFrom = exchange.ExchangeDate,
                    EffectiveTo = doctor1Shift.EffectiveTo, // Giữ nguyên EffectiveTo của lịch cũ
                    Status = "Active"
                };

                _context.DoctorShifts.Add(tempDoctor1Shift);
                _context.DoctorShifts.Add(tempDoctor2Shift);
                _context.DoctorShifts.Add(newDoctor1Shift);
                _context.DoctorShifts.Add(newDoctor2Shift);
            }
            else
            {
                // Đổi ca 1 ngày: Cập nhật lịch cũ thành NotActive
                doctor1Shift.Status = "NotActive";
                doctor2Shift.Status = "NotActive";
                
                // 1. Tạo lịch từ hôm nay đến ngày đổi (giữ nguyên ca cũ)
                var beforeDoctor1Shift = new DoctorShift
                {
                    DoctorId = doctor1Shift.DoctorId,
                    ShiftId = doctor1Shift.ShiftId, // Bác sĩ 1 làm ca của mình
                    EffectiveFrom = DateOnly.FromDateTime(DateTime.Now),
                    EffectiveTo = exchange.ExchangeDate.AddDays(-1),
                    Status = "Active"
                };

                var beforeDoctor2Shift = new DoctorShift
                {
                    DoctorId = doctor2Shift.DoctorId,
                    ShiftId = doctor2Shift.ShiftId, // Bác sĩ 2 làm ca của mình
                    EffectiveFrom = DateOnly.FromDateTime(DateTime.Now),
                    EffectiveTo = exchange.ExchangeDate.AddDays(-1),
                    Status = "Active"
                };

                // 2. Tạo lịch cho ngày đổi (đổi ca)
                var tempDoctor1Shift = new DoctorShift
                {
                    DoctorId = doctor1Shift.DoctorId,
                    ShiftId = doctor2Shift.ShiftId, // Bác sĩ 1 làm ca của bác sĩ 2
                    EffectiveFrom = exchange.ExchangeDate,
                    EffectiveTo = exchange.ExchangeDate,
                    Status = "Active"
                };

                var tempDoctor2Shift = new DoctorShift
                {
                    DoctorId = doctor2Shift.DoctorId,
                    ShiftId = doctor1Shift.ShiftId, // Bác sĩ 2 làm ca của bác sĩ 1
                    EffectiveFrom = exchange.ExchangeDate,
                    EffectiveTo = exchange.ExchangeDate,
                    Status = "Active"
                };

                // 3. Tạo lịch từ ngày sau đổi đến EffectiveTo (trở lại ca cũ)
                var afterDoctor1Shift = new DoctorShift
                {
                    DoctorId = doctor1Shift.DoctorId,
                    ShiftId = doctor1Shift.ShiftId, // Bác sĩ 1 trở lại ca của mình
                    EffectiveFrom = exchange.ExchangeDate.AddDays(1),
                    EffectiveTo = doctor1Shift.EffectiveTo, // Giữ nguyên EffectiveTo của lịch cũ
                    Status = "Active"
                };

                var afterDoctor2Shift = new DoctorShift
                {
                    DoctorId = doctor2Shift.DoctorId,
                    ShiftId = doctor2Shift.ShiftId, // Bác sĩ 2 trở lại ca của mình
                    EffectiveFrom = exchange.ExchangeDate.AddDays(1),
                    EffectiveTo = doctor2Shift.EffectiveTo, // Giữ nguyên EffectiveTo của lịch cũ
                    Status = "Active"
                };

                _context.DoctorShifts.Add(beforeDoctor1Shift);
                _context.DoctorShifts.Add(beforeDoctor2Shift);
                _context.DoctorShifts.Add(tempDoctor1Shift);
                _context.DoctorShifts.Add(tempDoctor2Shift);
                _context.DoctorShifts.Add(afterDoctor1Shift);
                _context.DoctorShifts.Add(afterDoctor2Shift);
            }
        }

        public async Task<bool> IsSameSpecialtyAsync(int doctor1Id, int doctor2Id)
        {
            var doctor1 = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DoctorId == doctor1Id);

            var doctor2 = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DoctorId == doctor2Id);

            if (doctor1 == null || doctor2 == null) return false;

            return doctor1.Specialty == doctor2.Specialty;
        }

        public async Task<bool> HasExistingShiftAsync(int doctorId, int doctorShiftId, DateOnly date)
        {
            return await _context.DoctorShifts
                .AnyAsync(x => x.DoctorId == doctorId && 
                              x.DoctorShiftId == doctorShiftId && 
                              x.EffectiveFrom <= date && 
                              (x.EffectiveTo == null || x.EffectiveTo >= date));
        }

        public async Task<bool> HasPendingRequestAsync(int doctor1Id, int doctor2Id, DateOnly exchangeDate)
        {
            return await _context.DoctorShiftExchanges
                .AnyAsync(x => ((x.Doctor1Id == doctor1Id && x.Doctor2Id == doctor2Id) ||
                               (x.Doctor1Id == doctor2Id && x.Doctor2Id == doctor1Id)) &&
                              x.ExchangeDate == exchangeDate &&
                              x.Status == "Pending");
        }

        public async Task<List<DoctorShiftDTO>> GetDoctorShiftsAsync(int doctorId, DateOnly from, DateOnly to)
        {
            return await _context.DoctorShifts
                .Where(x => x.DoctorId == doctorId && 
                           x.EffectiveFrom <= to && 
                           (x.EffectiveTo == null || x.EffectiveTo >= from))
                .Include(x => x.Doctor)
                    .ThenInclude(d => d.User)
                .Include(x => x.Shift)
                .Select(x => new DoctorShiftDTO
                {
                    DoctorShiftId = x.DoctorShiftId,
                    DoctorId = x.DoctorId,
                    DoctorName = x.Doctor.User.FullName,
                    Specialty = x.Doctor.Specialty,
                    ShiftId = x.ShiftId,
                    ShiftName = x.Shift.ShiftType,
                    ShiftType = x.Shift.ShiftType,
                    EffectiveFrom = x.EffectiveFrom,
                    EffectiveTo = x.EffectiveTo ?? x.EffectiveFrom,
                    Status = x.Status ?? "Active"
                })
                .ToListAsync();
        }

        public async Task<List<DoctorDTO>> GetDoctorsBySpecialtyAsync(string specialty)
        {
            return await _context.Doctors
                .Where(d => d.Specialty == specialty)
                .Include(d => d.User)
                .Select(d => new DoctorDTO
                {
                    DoctorID = d.DoctorId,
                    FullName = d.User.FullName,
                    Specialty = d.Specialty,
                    Email = d.User.Email
                })
                .ToListAsync();
        }

        public async Task<List<DoctorDTO>> GetAllDoctorsAsync()
        {
            return await _context.Doctors
                .Include(d => d.User)
                .Select(d => new DoctorDTO
                {
                    DoctorID = d.DoctorId,
                    FullName = d.User.FullName,
                    Specialty = d.Specialty,
                    Email = d.User.Email
                })
                .ToListAsync();
        }

        public async Task<List<string>> GetSpecialtiesAsync()
        {
            return await _context.Doctors
                .Select(d => d.Specialty)
                .Distinct()
                .ToListAsync();
        }

        public async Task<Doctor?> GetDoctorByIdAsync(int doctorId)
        {
            return await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId);
        }
    }
}
