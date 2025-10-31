using System;

namespace SEP490_BE.DAL.DTOs.Dashboard
{
    public class ClinicStatusDto
    {
        public DateOnly Date { get; set; }

        public AppointmentCounters Appointments { get; set; } = new AppointmentCounters();

        public int TodayNewPatients { get; set; }

        public class AppointmentCounters
        {
            public int Total { get; set; }
            public int Pending { get; set; }
            public int Confirmed { get; set; }
            public int Completed { get; set; }
            public int Cancelled { get; set; }
        }
    }
}


