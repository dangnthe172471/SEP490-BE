using SEP490_BE.DAL.DTOs.ManageReceptionist.ManageAppointment;

namespace SEP490_BE.DAL.DTOs.PrescriptionDoctorDTO
{
    public class PrescriptionSummaryDto
    {
        public int PrescriptionId { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DiagnosisInfoDto Diagnosis { get; set; } = new();
        public PrescriptionDoctorInfoDto Doctor { get; set; } = new();
        public PrescriptionPatientInfoDto Patient { get; set; } = new();
        public List<PrescriptionLineDto> Items { get; set; } = new();

        // Nếu muốn hiển thị ghi chú đơn (từ CreatePrescriptionRequest.Notes)
        public string? Notes { get; set; }
    }
}
