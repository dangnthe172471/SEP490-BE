namespace SEP490_BE.DAL.DTOs
{
	public class UserDto
	{
		public int UserId { get; set; }
		public string? Phone { get; set; }
		public string? FullName { get; set; }
		public string? Email { get; set; }
		public string? Role { get; set; }
		public string? Gender { get; set; }
		public DateOnly? Dob { get; set; }
		
		// Patient specific fields
		public string? Allergies { get; set; }
		public string? MedicalHistory { get; set; }
	}
}
