using System;

namespace SEP490_BE.DAL.DTOs.MedicalRecordDTO
{
    public class CreateMedicalRecordDto
    {
        public int AppointmentId { get; set; }
        public string? DoctorNotes { get; set; }
        public string? Diagnosis { get; set; }
    }
}



