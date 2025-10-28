using System;

namespace SEP490_BE.DAL.DTOs.ManageReceptionist.ManagerSchedule;

public class WorkScheduleDto
{
    public int DoctorShiftId { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = null!;
    public string Specialty { get; set; } = null!;
    public int RoomId { get; set; }
    public string RoomName { get; set; } = null!;
    public int ShiftId { get; set; }
    public string ShiftType { get; set; } = null!;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string? Status { get; set; }
}

public class DailyWorkScheduleDto
{
    public DateOnly Date { get; set; }
    public List<WorkScheduleDto> Shifts { get; set; } = new List<WorkScheduleDto>();
}
// Cập nhật lịch theo ngày
public class UpdateWorkScheduleByDateRequest
{
    public DateOnly Date { get; set; }
    public List<int> AddDoctorIds { get; set; } = new(); 
    public List<int> RemoveDoctorIds { get; set; } = new(); 
    public int ShiftId { get; set; } 
}

// Cập nhật lịch theo ID (DoctorShiftId)
public class UpdateWorkScheduleByIdRequest
{
    public int DoctorShiftId { get; set; }
    public int? NewDoctorId { get; set; }
    public int? NewShiftId { get; set; }
    public DateOnly? NewEffectiveFrom { get; set; }
    public DateOnly? NewEffectiveTo { get; set; }
    public string? Status { get; set; }
}

public class ShiftResponseDto
{
    public int ShiftID { get; set; }
    public string ShiftType { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public List<DoctorDTO> Doctors { get; set; } = new();
}


public class DailyWorkScheduleViewDto
{
    public DateOnly Date { get; set; }
    public List<ShiftResponseDto> Shifts { get; set; } = new();
}

public class DailySummaryDto
{
    public DateOnly Date { get; set; }
    public int ShiftCount { get; set; }
    public int DoctorCount { get; set; }
}

// Lịch làm việc nhóm theo ca, nhóm theo khoảng ngày trong db
//public class WorkScheduleGroupDto
//{
//    public DateOnly EffectiveFrom { get; set; }
//    public DateOnly? EffectiveTo { get; set; }
//    public int ShiftId { get; set; }
//    public string ShiftType { get; set; } = string.Empty;
//    public TimeOnly StartTime { get; set; }
//    public TimeOnly EndTime { get; set; }
//    public List<DoctorDTO> Doctors { get; set; } = new();
//}
public class WorkScheduleGroupDto
{
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public List<ShiftResponseDto> Shifts { get; set; } = new();
}
public class UpdateDoctorShiftRangeRequest
{
    public int ShiftId { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; } 
    public DateOnly? NewToDate { get; set; } 
    public List<int> AddDoctorIds { get; set; } = new();
    public List<int> RemoveDoctorIds { get; set; } = new();
}