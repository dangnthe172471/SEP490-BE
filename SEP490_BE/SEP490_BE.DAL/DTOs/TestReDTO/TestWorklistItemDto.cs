namespace SEP490_BE.DAL.DTOs.TestReDTO
{
    public class TestWorklistItemDto
    {
        public int RecordId { get; set; }
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = null!;
        public bool HasAllRequiredResults { get; set; }
        public List<ReadTestResultDto> Results { get; set; } = new();
    }
}
