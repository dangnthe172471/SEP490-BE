using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;

namespace SEP490_BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetAll(CancellationToken cancellationToken)
        {
            var rooms = await _roomService.GetAllAsync(cancellationToken);
            return Ok(rooms);
        }

        
        [HttpGet("paged")]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<PagedResponse<RoomDto>>> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _roomService.GetPagedAsync(pageNumber, pageSize, searchTerm, cancellationToken);
            return Ok(result);
        }

        
        [HttpGet("{id}")]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<RoomDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var room = await _roomService.GetByIdAsync(id, cancellationToken);
            if (room == null)
            {
                return NotFound();
            }

            return Ok(room);
        }

        
        [HttpPost]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateRoomRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var id = await _roomService.CreateAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

      
        [HttpPut("{id}")]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<ActionResult<RoomDto>> Update(
            int id,
            [FromBody] UpdateRoomRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _roomService.UpdateAsync(id, request, cancellationToken);
                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        
        [HttpDelete("{id}")]
        [Authorize(Roles = "Clinic Manager")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var success = await _roomService.DeleteAsync(id, cancellationToken);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
