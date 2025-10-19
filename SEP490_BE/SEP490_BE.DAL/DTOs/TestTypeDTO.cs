using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.DTOs
{
    public class TestTypeDto
    {
        public int TestTypeId { get; set; }
        public string TestName { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class CreateTestTypeRequest
    {
        public string TestName { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class UpdateTestTypeRequest
    {
        public string TestName { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    }
}
