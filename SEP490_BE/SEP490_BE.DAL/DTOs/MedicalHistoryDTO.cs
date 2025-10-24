namespace SEP490_BE.DAL.DTOs
{
    public class MedicalHistoryDTO
    {
        public int RecordId { get; set; }
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSpecialty { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string? Diagnosis { get; set; }
        public string? DoctorNotes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public List<PrescriptionDTO> Prescriptions { get; set; } = new List<PrescriptionDTO>();
        public List<TestResultDTO> TestResults { get; set; } = new List<TestResultDTO>();
    }

    public class PrescriptionDTO
    {
        public int PrescriptionId { get; set; }
        public int RecordId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime? IssuedDate { get; set; }
        public List<PrescriptionDetailDTO> PrescriptionDetails { get; set; } = new List<PrescriptionDetailDTO>();
    }

    public class PrescriptionDetailDTO
    {
        public int PrescriptionDetailId { get; set; }
        public int PrescriptionId { get; set; }
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
    }

    public class TestResultDTO
    {
        public int TestResultId { get; set; }
        public int RecordId { get; set; }
        public int TestTypeId { get; set; }
        public string TestTypeName { get; set; } = string.Empty;
        public string ResultValue { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public string? Attachment { get; set; }
        public DateTime? ResultDate { get; set; }
        public string? Notes { get; set; }
    }
}
