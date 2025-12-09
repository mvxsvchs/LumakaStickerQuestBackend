using LumakaStickerQuestBackend.Functions;
using LumakaStickerQuestBackend.Classes;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

namespace LumakaStickerQuestBackend.API
{
	[ApiController] // properties for api controllers
	[Route("api/user")]
	public class UserController : ControllerBase
	{
		private readonly Services.UserS _userService;

		// dependency injection (provides an instance of the service class UserS
		public UserController(Services.UserS userService)
		{
			_userService = userService;
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<UserDto>> GetUser(int id)
		{
			var user = await _userService.GetById(id);
			if (user == null)
			{
				return NotFound();
			}

			return Ok(user);
		}

		[HttpGet("get/{id}")]
		public async Task<ActionResult<UserDto>> GetUserByID(int id)
		{
			var user = await _userService.GetById(id);
			if (user == null)
			{
				return NotFound(); //HTTP 404 error
			}
			return Ok(user); // HTTP 200 success & user
		}

		[HttpPost("login")]
		public async Task<ActionResult<UserDto>> GetUserByMailPwd([FromBody] LoginDto login)
		{
			if (!ModelState.IsValid || login == null)
			{
				return BadRequest(ModelState);
			}
			
			var user = await _userService.GetByMailAndPwd(login.Mail, login.Password);
			if (user == null)
			{
				return Unauthorized();
			}

			return Ok(user);
		}

		[HttpPost("register")]
		public async Task<ActionResult<bool>> RegisterNewUser([FromBody] RegisterDto register)
		{
			if (!ModelState.IsValid || register == null)
			{
				return BadRequest(ModelState);
			}

			var success = await _userService.RegisterUser(register);
			if (success)
			{
				return Ok();
			}
			
			return BadRequest();
		}

		[HttpPut("{id}/stickers")]
		public async Task<IActionResult> UpdateUserStickers(int id, [FromBody] UpdateStickersRequest request)
		{
			if (request == null || request.stickers == null)
			{
				return BadRequest();
			}

			var success = await _userService.UpdateStickers(id, request.stickers.ToArray());
			if (!success)
			{
				return NotFound();
			}

			return NoContent();
		}

		[HttpPost("points")]
		public async Task<ActionResult<object>> UpdatePoints([FromBody] PointsDto request)
		{
			if (request == null || request.UserId <= 0)
			{
				return BadRequest();
			}

			var newPoints = await _userService.UpdateUserPoints(request.UserId, request.Points);
			if (newPoints == null)
			{
				return NotFound();
			}

			return Ok(new { user_points = newPoints.Value });
		}
	}
}
