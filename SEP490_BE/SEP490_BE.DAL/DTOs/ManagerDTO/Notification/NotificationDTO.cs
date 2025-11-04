using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.DTOs.ManagerDTO.Notification
{
  
        public class CreateNotificationDTO
        {
            public string Title { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public string Type { get; set; } = "General";
            public int? CreatedBy { get; set; }
            public bool IsGlobal { get; set; } = false;

            // Gửi cho Role
            public List<string>? RoleNames { get; set; }

            // Gửi cho user
            public List<int>? ReceiverIds { get; set; }
        }

        public class NotificationDTO
        {
            public int NotificationId { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public string Type { get; set; }
            public DateTime CreatedDate { get; set; }
            public bool IsRead { get; set; }
        }

    
}
