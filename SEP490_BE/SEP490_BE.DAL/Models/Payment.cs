using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int RecordId { get; set; }

    public decimal Amount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? Method { get; set; }

    public string? Status { get; set; }

    public virtual MedicalRecord Record { get; set; } = null!;
}
