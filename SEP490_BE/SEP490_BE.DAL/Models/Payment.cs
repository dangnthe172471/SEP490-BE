using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int AppointmentId { get; set; }

    public decimal Amount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? Method { get; set; }

    public string? Status { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;
}
