namespace SEP490_BE.DAL.DTOs.InternalMedRecordsDTO
{
    public class CreateInternalMedRecordDto
    {
        public int RecordId { get; set; }
        public int? BloodPressure { get; set; }
        public int? HeartRate { get; set; }
        public decimal? BloodSugar { get; set; }
        public string? Notes { get; set; }
    }
}
