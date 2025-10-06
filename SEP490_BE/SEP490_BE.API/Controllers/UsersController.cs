using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;

namespace SEP490_BE.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UsersController : ControllerBase
	{
		private readonly IUserService _userService;

		public UsersController(IUserService userService)
		{
			_userService = userService;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<UserDto>>> GetAll(CancellationToken cancellationToken)
		{
			var users = await _userService.GetAllAsync(cancellationToken);
			return Ok(users);
		}
	}
}
