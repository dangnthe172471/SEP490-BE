namespace SEP490_BE.DAL.DTOs.InternalMedRecordsDTO
{
    public class UpdateInternalMedRecordDto
    {
        public int? BloodPressure { get; set; }
        public int? HeartRate { get; set; }
        public decimal? BloodSugar { get; set; }
        public string? Notes { get; set; }
    }
}
