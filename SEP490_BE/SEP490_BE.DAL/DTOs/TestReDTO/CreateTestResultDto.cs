namespace SEP490_BE.DAL.DTOs.TestReDTO
{
    public class CreateTestResultDto
    {
        public int RecordId { get; set; }
        public int TestTypeId { get; set; }
        public string ResultValue { get; set; } = null!;
        public string? Unit { get; set; }
        public string? Attachment { get; set; }
        public DateTime? ResultDate { get; set; }
        public string? Notes { get; set; }
    }
}
