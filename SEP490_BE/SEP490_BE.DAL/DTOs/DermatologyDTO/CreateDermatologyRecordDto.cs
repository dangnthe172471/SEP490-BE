namespace SEP490_BE.DAL.DTOs.DermatologyDTO
{
    public class CreateDermatologyRecordDto
    {
        public int RecordId { get; set; }
        public string? RequestedProcedure { get; set; }
        public string? BodyArea { get; set; }
        public string? ProcedureNotes { get; set; }
    }
}
