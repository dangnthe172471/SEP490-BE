using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class MedicineVersion
{
    public int MedicineVersionId { get; set; }

    public int MedicineId { get; set; }

    public string MedicineName { get; set; } = null!;

    public string? Strength { get; set; }

    public string? DosageForm { get; set; }

    public string? Route { get; set; }

    public string? PrescriptionUnit { get; set; }

    public string? TherapeuticClass { get; set; }

    public int ProviderId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ActiveIngredient { get; set; }

    public string? PackSize { get; set; }

    public string? CommonSideEffects { get; set; }

    public string? NoteForDoctor { get; set; }

    public string? ProviderName { get; set; }

    public string? ProviderContact { get; set; }

    public virtual Medicine Medicine { get; set; } = null!;

    public virtual ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();
}
