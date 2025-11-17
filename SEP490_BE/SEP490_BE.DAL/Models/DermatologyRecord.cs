using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class DermatologyRecord
{
    public int DermRecordId { get; set; }

    public int RecordId { get; set; }

    public int? PerformedByUserId { get; set; }

    public string RequestedProcedure { get; set; } = null!;

    public string? BodyArea { get; set; }

    public string? ProcedureNotes { get; set; }

    public string? ResultSummary { get; set; }

    public string? Attachment { get; set; }

    public DateTime PerformedAt { get; set; }

    public virtual User? PerformedByUser { get; set; }

    public virtual MedicalRecord Record { get; set; } = null!;
}
