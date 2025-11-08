using System.ComponentModel.DataAnnotations;

namespace SEP490_BE.DAL.DTOs
{
    public class CreateShiftSwapRequestDTO
    {
        [Required]
        public int Doctor1Id { get; set; }
        
        [Required]
        public int Doctor2Id { get; set; }
        
        [Required]
        public int Doctor1ShiftRefId { get; set; }
        
        [Required]
        public int Doctor2ShiftRefId { get; set; }
        
        public DateOnly? ExchangeDate { get; set; } // Nullable: required cho Temporary, sẽ được set thành đầu tháng sau cho Permanent
        
        [Required]
        public string SwapType { get; set; } = "temporary"; // "temporary" or "permanent"
    }

    public class ReviewShiftSwapRequestDTO
    {
        [Required]
        public int ExchangeId { get; set; }
        
        [Required]
        public string Status { get; set; } = string.Empty; // "Approved" or "Rejected"
    }

    public class ShiftSwapRequestResponseDTO
    {
        public int ExchangeId { get; set; }
        public int Doctor1Id { get; set; }
        public string Doctor1Name { get; set; } = string.Empty;
        public string Doctor1Specialty { get; set; } = string.Empty;
        public int Doctor2Id { get; set; }
        public string Doctor2Name { get; set; } = string.Empty;
        public string Doctor2Specialty { get; set; } = string.Empty;
        public int Doctor1ShiftRefId { get; set; }
        public string Doctor1ShiftName { get; set; } = string.Empty;
        public int Doctor2ShiftRefId { get; set; }
        public string Doctor2ShiftName { get; set; } = string.Empty;
        public int DoctorOld1ShiftId { get; set; }
        public string DoctorOld1ShiftName { get; set; } = string.Empty;
        public int? DoctorOld2ShiftId { get; set; }
        public string DoctorOld2ShiftName { get; set; } = string.Empty;
        public DateOnly? ExchangeDate { get; set; } // Đầu tháng sau cho permanent, ngày cụ thể cho temporary
        public string Status { get; set; } = string.Empty;
        public string SwapType { get; set; } = "temporary";
    }

    public class DoctorShiftDTO
    {
        public int DoctorShiftId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public int ShiftId { get; set; }
        public string ShiftName { get; set; } = string.Empty;
        public string ShiftType { get; set; } = string.Empty;
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly EffectiveTo { get; set; }
        public string Status { get; set; } = string.Empty;
    }

}
