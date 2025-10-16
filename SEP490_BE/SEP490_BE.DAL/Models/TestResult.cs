using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class TestResult
{
    public int TestResultId { get; set; }

    public int RecordId { get; set; }

    public int TestTypeId { get; set; }

    public string ResultValue { get; set; } = null!;

    public string? Unit { get; set; }

    public string? Attachment { get; set; }

    public DateTime? ResultDate { get; set; }

    public string? Notes { get; set; }

    public virtual MedicalRecord Record { get; set; } = null!;

    public virtual TestType TestType { get; set; } = null!;
}
