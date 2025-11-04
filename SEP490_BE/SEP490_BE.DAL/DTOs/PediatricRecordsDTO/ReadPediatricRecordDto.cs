namespace SEP490_BE.DAL.DTOs.PediatricRecordsDTO
{
    public class ReadPediatricRecordDto
    {
        public int RecordId { get; set; }
        public decimal? WeightKg { get; set; }
        public decimal? HeightCm { get; set; }
        public int? HeartRate { get; set; }
        public decimal? TemperatureC { get; set; }
    }
}
