namespace SEP490_BE.DAL.DTOs.DermatologyDTO
{
    public class ReadDermatologyRecordDto
    {
        public int DermRecordId { get; set; }
        public int RecordId { get; set; }
        public string RequestedProcedure { get; set; } = string.Empty;
        public string? BodyArea { get; set; }
        public string? ProcedureNotes { get; set; }
        public string? ResultSummary { get; set; }
        public string? Attachment { get; set; }
        public DateTime PerformedAt { get; set; }
        public int? PerformedByUserId { get; set; }
    }
}
