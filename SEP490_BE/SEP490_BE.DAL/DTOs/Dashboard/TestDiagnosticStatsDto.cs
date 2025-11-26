using System.Collections.Generic;

namespace SEP490_BE.DAL.DTOs.Dashboard
{
    public class CategoryCountDto
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal? Revenue { get; set; }
    }

    public class DiagnosticTrendPointDto
    {
        public string Period { get; set; } = string.Empty; // yyyy-MM or yyyy-MM-dd
        public int VisitCount { get; set; }
        public int TestCount { get; set; }
    }

    public class TestDiagnosticStatsDto
    {
        public int TotalVisits { get; set; }
        public int TotalTests { get; set; }

        public List<CategoryCountDto> VisitTypeCounts { get; set; } = new();
        public List<CategoryCountDto> TestTypeCounts { get; set; } = new();
        public List<CategoryCountDto> TopVisitServices { get; set; } = new();
        public List<CategoryCountDto> TopTestServices { get; set; } = new();
        public List<DiagnosticTrendPointDto> Trends { get; set; } = new();
    }
}

