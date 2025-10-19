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
}
