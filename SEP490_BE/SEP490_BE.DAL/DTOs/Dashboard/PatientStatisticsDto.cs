using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.DTOs.Dashboard
{
    public class PatientStatisticsDto
    {
        public int TotalPatients { get; set; }

        public GenderCounters ByGender { get; set; } = new GenderCounters();

        public AgeGroupCounters ByAgeGroups { get; set; } = new AgeGroupCounters();

        public List<MonthlyCount> MonthlyNewPatients { get; set; } = new();

        public class GenderCounters
        {
            public int Male { get; set; }
            public int Female { get; set; }
            public int Other { get; set; }
        }

        public class AgeGroupCounters
        {
            public int _0_17 { get; set; }
            public int _18_35 { get; set; }
            public int _36_55 { get; set; }
            public int _56_Plus { get; set; }
        }

        public class MonthlyCount
        {
            public string Month { get; set; } = string.Empty; // yyyy-MM
            public int Count { get; set; }
        }
    }
}


