using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.DTOs.ManagerDTO.ManagerSchedule
{
    public class CreateScheduleRequestDTO
    {
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly EffectiveTo { get; set; }
        public List<ShiftDoctorMap> Shifts { get; set; } = new();
    }
    public class ShiftDoctorMap
    {
        public int ShiftId { get; set; }
        public List<int> DoctorIds { get; set; } = new();
    }
}
