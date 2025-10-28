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
        public IReadOnlyList<PrescriptionLineDto> Items { get; set; } = Array.Empty<PrescriptionLineDto>();
    }
}
