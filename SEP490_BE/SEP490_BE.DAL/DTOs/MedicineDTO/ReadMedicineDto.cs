namespace SEP490_BE.DAL.DTOs.MedicineDTO
{
    public class ReadMedicineDto
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string? SideEffects { get; set; }
        public string? Status { get; set; }
        public int ProviderId { get; set; }
        public string? ProviderName { get; set; }
    }
}
