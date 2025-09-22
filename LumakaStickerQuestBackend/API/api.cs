using LumakaStickerQuestBackend.Functions;
using LumakaStickerQuestBackend.Classes;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LumakaStickerQuestBackend.API
{
	[ApiController] // properties for api controllers
	[Route("api/[controller]")]
	public class QuestController : ControllerBase
	{
		private readonly Services.UserS _userService;

		// dependency injection (provides an instance of the service class UserS
		public QuestController(Services.UserS userService)
		{
			_userService = userService;
		}

		[HttpGet("get/{id}")]
		public async Task<ActionResult<FeUser>> GetUserByID(int id)
		{
			var user = await _userService.GetById(id);
			if (user == null)
			{
				return NotFound(); //HTTP 404 error
			}
			return Ok(user); // HTTP 200 success & user
		}

		[HttpPut("login")]
		public async Task<ActionResult<FeUser>> GetUserByMailPwd([FromBody] FeLogin login)
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
		public async Task<ActionResult<bool>> RegisterNewUser([FromBody] FeRegister register)
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
	}
}