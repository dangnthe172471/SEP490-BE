using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class Prescription
{
    public int PrescriptionId { get; set; }

    public int RecordId { get; set; }

    public int DoctorId { get; set; }

    public DateTime? IssuedDate { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();

    public virtual MedicalRecord Record { get; set; } = null!;
}
