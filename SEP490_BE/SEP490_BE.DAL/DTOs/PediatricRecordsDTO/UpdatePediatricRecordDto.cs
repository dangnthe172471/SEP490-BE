namespace SEP490_BE.DAL.DTOs.PediatricRecordsDTO
{
    public class UpdatePediatricRecordDto
    {
        public decimal? WeightKg { get; set; }
        public decimal? HeightCm { get; set; }
        public int? HeartRate { get; set; }
        public decimal? TemperatureC { get; set; }
    }
}
