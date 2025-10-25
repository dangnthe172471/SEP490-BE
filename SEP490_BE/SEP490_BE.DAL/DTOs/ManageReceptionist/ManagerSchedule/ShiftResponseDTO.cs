using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.DTOs.ManageReceptionist.ManagerSchedule
{
    public class ShiftResponseDTO
    {
        public int ShiftID { get; set; }
        public string ShiftType { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
