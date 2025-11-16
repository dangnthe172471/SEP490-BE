using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class TestType
{
    public int TestTypeId { get; set; }

    public string TestName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();

    public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
}
