using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class NotificationReceiver
{
    public int NotificationId { get; set; }

    public int ReceiverId { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadDate { get; set; }

    public virtual Notification Notification { get; set; } = null!;

    public virtual User Receiver { get; set; } = null!;
}
