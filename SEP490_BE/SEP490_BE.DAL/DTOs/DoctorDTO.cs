using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.DTOs
{
    public class DoctorDTO
    {
        public int DoctorID { get; set; }
        public string FullName { get; set; }
        public string Specialty { get; set; }
        public string Email { get; set; }
    }
    public class DoctorActiveScheduleRangeDto
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string Specialty { get; set; }
        public string RoomName { get; set; }

        public DateOnly Date { get; set; } 
        public string ShiftType { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string? Status { get; set; }
    }
}
