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
        
        [Required]
        public DateOnly ExchangeDate { get; set; }
        
        [Required]
        public string SwapType { get; set; } = "Temporary"; // "Temporary" or "Permanent"
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
        public DateOnly ExchangeDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string SwapType { get; set; } = "Temporary";
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
