namespace SEP490_BE.DAL.DTOs.PrescriptionDoctorDTO
{
    public class CreatePrescriptionRequest
    {
        public int RecordId { get; set; }
        public DateTime? IssuedDate { get; set; }
        public string? Notes { get; set; }

        public List<CreatePrescriptionItem> Items { get; set; } = new();
    }
}
