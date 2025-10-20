using System;

namespace SEP490_BE.DAL.DTOs;

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

