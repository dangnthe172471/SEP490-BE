using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class PrescriptionDetail
{
    public int PrescriptionDetailId { get; set; }

    public int PrescriptionId { get; set; }

    public int MedicineId { get; set; }

    public string Dosage { get; set; } = null!;

    public string Duration { get; set; } = null!;

    public virtual Medicine Medicine { get; set; } = null!;

    public virtual Prescription Prescription { get; set; } = null!;
}
