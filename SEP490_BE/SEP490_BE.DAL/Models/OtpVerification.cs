using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEP490_BE.DAL.Models
{
    public class OtpVerification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = null!;

        [Required]
        [StringLength(6)]
        public string OtpCode { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Purpose { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

