using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.DTOs
{
    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateServiceRequest
    {
        public string ServiceName { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateServiceRequest
    {
        public string ServiceName { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public bool IsActive { get; set; }
    }
}

