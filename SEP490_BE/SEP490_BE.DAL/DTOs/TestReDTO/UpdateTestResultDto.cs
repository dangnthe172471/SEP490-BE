namespace SEP490_BE.DAL.DTOs.TestReDTO
{
    public class UpdateTestResultDto
    {
        public string? ResultValue { get; set; }
        public string? Unit { get; set; }
        public string? Attachment { get; set; }
        public DateTime? ResultDate { get; set; }
        public string? Notes { get; set; }
    }
}
