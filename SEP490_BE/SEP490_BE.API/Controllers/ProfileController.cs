using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using System.Security.Claims;

namespace SEP490_BE.API.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IUserService _userService;

        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpPut("basic-info")]
        [Authorize]
        public async Task<IActionResult> UpdateBasicInfo([FromBody] UpdateBasicInfoRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var phoneClaim = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(phoneClaim))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }
                
                // Get user by phone first to verify
                var userByPhone = await _userService.GetUserByPhoneAsync(phoneClaim);
                if (userByPhone == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                }
                
                // Use the correct userId from database
                var updatedUser = await _userService.UpdateBasicInfoAsync(userByPhone.UserId, request, cancellationToken);
                if (updatedUser == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                }

                return Ok(new { 
                    message = "Cập nhật thông tin cơ bản thành công",
                    user = updatedUser
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi cập nhật thông tin", error = ex.Message });
            }
        }

        [HttpPut("medical-info")]
        [Authorize]
        public async Task<IActionResult> UpdateMedicalInfo([FromBody] UpdateMedicalInfoRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var phoneClaim = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(phoneClaim))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }
                
                // Get user by phone first to verify
                var userByPhone = await _userService.GetUserByPhoneAsync(phoneClaim);
                if (userByPhone == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                }
                
                // Use the correct userId from database
                var updatedUser = await _userService.UpdateMedicalInfoAsync(userByPhone.UserId, request, cancellationToken);
                if (updatedUser == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng hoặc người dùng không phải bệnh nhân" });
                }

                return Ok(new { 
                    message = "Cập nhật thông tin y tế thành công",
                    user = updatedUser
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi cập nhật thông tin y tế", error = ex.Message });
            }
        }
    }
}
