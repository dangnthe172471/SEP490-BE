using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class PharmacyProvider
{
    public int ProviderId { get; set; }

    public int UserId { get; set; }

    public string? Contact { get; set; }

    public virtual ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();

    public virtual User User { get; set; } = null!;
}
