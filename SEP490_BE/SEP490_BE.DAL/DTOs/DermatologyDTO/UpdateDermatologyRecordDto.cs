namespace SEP490_BE.DAL.DTOs.DermatologyDTO
{
    public class UpdateDermatologyRecordDto
    {
        public string? RequestedProcedure { get; set; }
        public string? BodyArea { get; set; }
        public string? ProcedureNotes { get; set; }
        public string? ResultSummary { get; set; }
        public string? Attachment { get; set; }

        public int? PerformedByUserId { get; set; }
    }
}
