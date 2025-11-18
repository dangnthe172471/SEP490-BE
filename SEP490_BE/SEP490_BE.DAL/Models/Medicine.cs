using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class Medicine
{
    public int MedicineId { get; set; }

    public int ProviderId { get; set; }

    public string MedicineName { get; set; } = null!;

    public string? Status { get; set; }

    public string? ActiveIngredient { get; set; }

    public string? Strength { get; set; }

    public string? DosageForm { get; set; }

    public string? Route { get; set; }

    public string? PrescriptionUnit { get; set; }

    public string? TherapeuticClass { get; set; }

    public string? PackSize { get; set; }

    public string? CommonSideEffects { get; set; }

    public string? NoteForDoctor { get; set; }

    public virtual ICollection<MedicineVersion> MedicineVersions { get; set; } = new List<MedicineVersion>();

    public virtual PharmacyProvider Provider { get; set; } = null!;
}
