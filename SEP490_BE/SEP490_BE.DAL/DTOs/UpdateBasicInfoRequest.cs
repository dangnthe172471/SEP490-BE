namespace SEP490_BE.DAL.DTOs
{
    public class UpdateBasicInfoRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Dob { get; set; }
        public string? Gender { get; set; }
    }
}
