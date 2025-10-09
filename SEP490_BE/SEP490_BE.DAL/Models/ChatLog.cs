using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class ChatLog
{
    public int ChatId { get; set; }

    public int PatientId { get; set; }

    public int ReceptionistId { get; set; }

    public string RoomChat { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Patient Patient { get; set; } = null!;

    public virtual User Receptionist { get; set; } = null!;
}
