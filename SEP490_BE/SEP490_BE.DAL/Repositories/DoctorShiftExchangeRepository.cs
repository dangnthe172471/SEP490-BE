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
            // Validate: Chỉ được đổi lịch từ ngày mai trở đi (chỉ cho Temporary)
            // Permanent: ExchangeDate sẽ được set thành đầu tháng sau
            DateOnly? exchangeDate = request.ExchangeDate;
            
            if (request.SwapType?.ToLower() == "temporary")
            {
                if (!request.ExchangeDate.HasValue)
                {
                    throw new ArgumentException("ExchangeDate là bắt buộc cho đổi ca 1 ngày (Temporary).");
                }
                
                var tomorrow = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
                if (request.ExchangeDate.Value < tomorrow)
                {
                    throw new ArgumentException("Chỉ được đổi lịch từ ngày mai trở đi. Không thể đổi lịch hôm nay.");
                }
            }
            else if (request.SwapType?.ToLower() == "permanent")
            {
                // Permanent: Set ExchangeDate = đầu tháng sau (thay vì null)
                var nextMonthStart = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1));
                exchangeDate = nextMonthStart;
            }

            // Lấy ShiftId hiện tại từ DoctorShift để lưu lịch sử
            var doctor1Shift = await _context.DoctorShifts
                .FirstOrDefaultAsync(x => x.DoctorShiftId == request.Doctor1ShiftRefId);
            
            var doctor2Shift = await _context.DoctorShifts
                .FirstOrDefaultAsync(x => x.DoctorShiftId == request.Doctor2ShiftRefId);

            if (doctor1Shift == null || doctor2Shift == null)
            {
                throw new ArgumentException("Không tìm thấy ca làm việc của bác sĩ.");
            }

            var exchange = new DoctorShiftExchange
            {
                Doctor1Id = request.Doctor1Id,
                Doctor2Id = request.Doctor2Id,
                Doctor1ShiftRefId = request.Doctor1ShiftRefId,
                Doctor2ShiftRefId = request.Doctor2ShiftRefId,
                DoctorOld1ShiftId = doctor1Shift.ShiftId,
                DoctorOld2ShiftId = doctor2Shift.ShiftId,
                ExchangeDate = exchangeDate,
                Status = "Pending",
                SwapType = request.SwapType
            };

            _context.DoctorShiftExchanges.Add(exchange);
            await _context.SaveChangesAsync();
            return exchange;
        }

        public async Task<List<ShiftSwapRequestResponseDTO>> GetAllRequestsAsync()
        {
            var exchanges = await _context.DoctorShiftExchanges
                .Include(x => x.Doctor1)
                    .ThenInclude(d => d.User)
                .Include(x => x.Doctor2)
                    .ThenInclude(d => d.User)
                .Include(x => x.Doctor1ShiftRef)
                    .ThenInclude(s => s.Shift)
                .Include(x => x.Doctor2ShiftRef)
                    .ThenInclude(s => s.Shift)
                .ToListAsync();

            var result = new List<ShiftSwapRequestResponseDTO>();

            foreach (var x in exchanges)
            {
                result.Add(await MapToDTOAsync(x));
            }

            return result.OrderByDescending(x => x.ExchangeDate ?? DateOnly.MinValue).ToList();
        }

        public async Task<List<ShiftSwapRequestResponseDTO>> GetRequestsByDoctorIdAsync(int doctorId)
        {
            var exchanges = await _context.DoctorShiftExchanges
                .Where(x => x.Doctor1Id == doctorId || x.Doctor2Id == doctorId)
                .Include(x => x.Doctor1)
                    .ThenInclude(d => d.User)
                .Include(x => x.Doctor2)
                    .ThenInclude(d => d.User)
                .Include(x => x.Doctor1ShiftRef)
                    .ThenInclude(s => s.Shift)
                .Include(x => x.Doctor2ShiftRef)
                    .ThenInclude(s => s.Shift)
                .ToListAsync();

            var result = new List<ShiftSwapRequestResponseDTO>();

            foreach (var x in exchanges)
            {
                result.Add(await MapToDTOAsync(x));
            }

            return result;
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

        private async Task<ShiftSwapRequestResponseDTO> MapToDTOAsync(DoctorShiftExchange x)
        {
            var old1Shift = await _context.Shifts.FirstOrDefaultAsync(s => s.ShiftId == x.DoctorOld1ShiftId);
            var old2Shift = x.DoctorOld2ShiftId.HasValue
                ? await _context.Shifts.FirstOrDefaultAsync(s => s.ShiftId == x.DoctorOld2ShiftId.Value)
                : null;

            return new ShiftSwapRequestResponseDTO
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
                DoctorOld1ShiftId = x.DoctorOld1ShiftId,
                DoctorOld1ShiftName = old1Shift?.ShiftType ?? "",
                DoctorOld2ShiftId = x.DoctorOld2ShiftId,
                DoctorOld2ShiftName = old2Shift?.ShiftType ?? "",
                ExchangeDate = x.ExchangeDate,
                Status = x.Status ?? "Pending",
                SwapType = x.SwapType ?? "Temporary"
            };
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
            if (exchange.SwapType?.ToLower() != "permanent")
            {
                return;
            }

            // Lấy thông tin ca hiện tại của 2 bác sĩ
            var doctor1Shift = await _context.DoctorShifts
                .FirstOrDefaultAsync(x => x.DoctorShiftId == exchange.Doctor1ShiftRefId);
            
            var doctor2Shift = await _context.DoctorShifts
                .FirstOrDefaultAsync(x => x.DoctorShiftId == exchange.Doctor2ShiftRefId);

            if (doctor1Shift == null || doctor2Shift == null) return;

            doctor1Shift.ShiftId = exchange.DoctorOld2ShiftId ?? doctor2Shift.ShiftId;
            doctor2Shift.ShiftId = exchange.DoctorOld1ShiftId;

            // ExchangeDate đã được set thành đầu tháng sau cho permanent swaps
            var effectiveDate = exchange.ExchangeDate ?? DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1));
            
            if (effectiveDate > doctor1Shift.EffectiveFrom)
            {
                doctor1Shift.EffectiveFrom = effectiveDate;
            }
            if (effectiveDate > doctor2Shift.EffectiveFrom)
            {
                doctor2Shift.EffectiveFrom = effectiveDate;
            }

            doctor1Shift.Status = "Active";
            doctor2Shift.Status = "Active";
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

        public async Task<bool> HasPendingRequestAsync(int doctor1Id, int doctor2Id, DateOnly? exchangeDate)
        {
            return await _context.DoctorShiftExchanges
                .AnyAsync(x => ((x.Doctor1Id == doctor1Id && x.Doctor2Id == doctor2Id) ||
                               (x.Doctor1Id == doctor2Id && x.Doctor2Id == doctor1Id)) &&
                              (exchangeDate == null ? x.ExchangeDate == null : x.ExchangeDate == exchangeDate) &&
                              x.Status == "Pending");
        }

        public async Task<DoctorShift?> GetDoctorShiftByIdAsync(int doctorShiftId)
        {
            return await _context.DoctorShifts
                .FirstOrDefaultAsync(x => x.DoctorShiftId == doctorShiftId);
        }

        public async Task<List<DoctorShiftDTO>> GetDoctorShiftsAsync(int doctorId, DateOnly from, DateOnly to)
        {
            var baseShifts = await _context.DoctorShifts
                .Where(x => x.DoctorId == doctorId && 
                           x.EffectiveFrom <= to && 
                           (x.EffectiveTo == null || x.EffectiveTo >= from) &&
                           (x.Status == null || x.Status == "Active"))
                .Include(x => x.Doctor)
                    .ThenInclude(d => d.User)
                .Include(x => x.Shift)
                .ToListAsync();

            // Lấy tất cả đổi ca 1 ngày đã approved trong khoảng thời gian
            // Chỉ lấy Temporary và có ExchangeDate không null
            var temporaryExchanges = await _context.DoctorShiftExchanges
                .Where(x => x.Status == "Approved" &&
                           x.SwapType != null && 
                           x.SwapType.ToLower() == "temporary" &&
                           x.ExchangeDate != null &&
                           x.ExchangeDate >= from &&
                           x.ExchangeDate <= to &&
                           (x.Doctor1Id == doctorId || x.Doctor2Id == doctorId))
                .Include(x => x.Doctor1ShiftRef)
                    .ThenInclude(s => s.Shift)
                .Include(x => x.Doctor2ShiftRef)
                    .ThenInclude(s => s.Shift)
                .ToListAsync();

            var result = new List<DoctorShiftDTO>();

            foreach (var baseShift in baseShifts)
            {
                var effectiveFrom = baseShift.EffectiveFrom;
                var effectiveTo = baseShift.EffectiveTo ?? effectiveFrom;

                var exchange = temporaryExchanges.FirstOrDefault(x =>
                    x.ExchangeDate.HasValue &&
                    x.ExchangeDate.Value >= effectiveFrom &&
                    x.ExchangeDate.Value <= effectiveTo &&
                    ((x.Doctor1Id == doctorId && x.Doctor1ShiftRefId == baseShift.DoctorShiftId) ||
                     (x.Doctor2Id == doctorId && x.Doctor2ShiftRefId == baseShift.DoctorShiftId)));

                if (exchange != null && exchange.ExchangeDate.HasValue)
                {
                    var exchangeDate = exchange.ExchangeDate!.Value;
                    
                    if (exchangeDate > effectiveFrom)
                    {
                        // Khoảng trước ngày đổi (ca gốc)
                        result.Add(new DoctorShiftDTO
                        {
                            DoctorShiftId = baseShift.DoctorShiftId,
                            DoctorId = baseShift.DoctorId,
                            DoctorName = baseShift.Doctor.User.FullName,
                            Specialty = baseShift.Doctor.Specialty,
                            ShiftId = baseShift.ShiftId,
                            ShiftName = baseShift.Shift.ShiftType,
                            ShiftType = baseShift.Shift.ShiftType,
                            EffectiveFrom = effectiveFrom,
                            EffectiveTo = exchangeDate.AddDays(-1),
                            Status = baseShift.Status ?? "Active"
                        });
                    }

                    int swappedShiftId;
                    string swappedShiftName;
                    string swappedShiftType;

                    if (exchange.Doctor1Id == doctorId)
                    {
                        // BS1 đổi với BS2 -> BS1 làm ca của BS2
                        swappedShiftId = exchange.Doctor2ShiftRef?.ShiftId ?? baseShift.ShiftId;
                        swappedShiftName = exchange.Doctor2ShiftRef?.Shift?.ShiftType ?? baseShift.Shift.ShiftType;
                        swappedShiftType = exchange.Doctor2ShiftRef?.Shift?.ShiftType ?? baseShift.Shift.ShiftType;
                    }
                    else
                    {
                        // BS2 đổi với BS1 -> BS2 làm ca của BS1
                        swappedShiftId = exchange.Doctor1ShiftRef?.ShiftId ?? baseShift.ShiftId;
                        swappedShiftName = exchange.Doctor1ShiftRef?.Shift?.ShiftType ?? baseShift.Shift.ShiftType;
                        swappedShiftType = exchange.Doctor1ShiftRef?.Shift?.ShiftType ?? baseShift.Shift.ShiftType;
                    }

                    result.Add(new DoctorShiftDTO
                    {
                        DoctorShiftId = baseShift.DoctorShiftId,
                        DoctorId = baseShift.DoctorId,
                        DoctorName = baseShift.Doctor.User.FullName,
                        Specialty = baseShift.Doctor.Specialty,
                        ShiftId = swappedShiftId,
                        ShiftName = swappedShiftName,
                        ShiftType = swappedShiftType,
                        EffectiveFrom = exchangeDate,
                        EffectiveTo = exchangeDate,
                        Status = baseShift.Status ?? "Active"
                    });

                    if (exchangeDate < effectiveTo)
                    {
                        // Khoảng sau ngày đổi (ca gốc)
                        result.Add(new DoctorShiftDTO
                        {
                            DoctorShiftId = baseShift.DoctorShiftId,
                            DoctorId = baseShift.DoctorId,
                            DoctorName = baseShift.Doctor.User.FullName,
                            Specialty = baseShift.Doctor.Specialty,
                            ShiftId = baseShift.ShiftId,
                            ShiftName = baseShift.Shift.ShiftType,
                            ShiftType = baseShift.Shift.ShiftType,
                            EffectiveFrom = exchangeDate.AddDays(1),
                            EffectiveTo = effectiveTo,
                            Status = baseShift.Status ?? "Active"
                        });
                    }
                }
                else
                {
                    // Không có đổi ca, giữ nguyên
                    result.Add(new DoctorShiftDTO
                    {
                        DoctorShiftId = baseShift.DoctorShiftId,
                        DoctorId = baseShift.DoctorId,
                        DoctorName = baseShift.Doctor.User.FullName,
                        Specialty = baseShift.Doctor.Specialty,
                        ShiftId = baseShift.ShiftId,
                        ShiftName = baseShift.Shift.ShiftType,
                        ShiftType = baseShift.Shift.ShiftType,
                        EffectiveFrom = effectiveFrom,
                        EffectiveTo = effectiveTo,
                        Status = baseShift.Status ?? "Active"
                    });
                }
            }

            return result;
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
