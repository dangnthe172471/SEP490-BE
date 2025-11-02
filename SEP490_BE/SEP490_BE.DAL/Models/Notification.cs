using System;
using System.Collections.Generic;

namespace SEP490_BE.DAL.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string Type { get; set; } = null!;

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool IsGlobal { get; set; }

    public bool IsEmailSent { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<NotificationReceiver> NotificationReceivers { get; set; } = new List<NotificationReceiver>();
}
