using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.DTOs
{
    public class RoomDto
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; } = null!;
    }

    public class CreateRoomRequest
    {
        public string RoomName { get; set; } = null!;
    }

    public class UpdateRoomRequest
    {
        public string RoomName { get; set; } = null!;
    }
}
