namespace SEP490_BE.DAL.DTOs
{
	public class RegisterRequest
	{
		public string Username { get; set; } = null!;
		public string Password { get; set; } = null!;
		public string FullName { get; set; } = null!;
		public string? Email { get; set; }
		public string? Phone { get; set; }
		public DateOnly? Dob { get; set; }
		public string? Gender { get; set; }
		public int RoleId { get; set; }
	}
}


