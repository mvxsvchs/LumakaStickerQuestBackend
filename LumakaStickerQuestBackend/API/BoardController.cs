using LumakaStickerQuestBackend.Classes;
using LumakaStickerQuestBackend.Functions;
using Microsoft.AspNetCore.Mvc;

namespace LumakaStickerQuestBackend.API
{
	[ApiController]
	[Route("api/board")]
	public class BoardController : ControllerBase
	{
		private readonly Services.BoardS _boardService;

		public BoardController(Services.BoardS boardService)
		{
			_boardService = boardService;
		}

		[HttpGet("get/{userId}")]
		public async Task<ActionResult<Board>> GetBoard(int userId)
		{
			var board = await _boardService.GetBoard(userId);
			if (board == null)
			{
				return NotFound();
			}
			return Ok(board);
		}

		[HttpPost("post/{userId}")]
		public async Task<ActionResult<bool>> AddBoard(int userId)
		{
			var success = await _boardService.AddBoard(userId);
			if (success)
			{
				return Ok(success);
			}
			return BadRequest();
		}
		
		[HttpPost("post/field/{userId}")]
		public async Task<ActionResult<string>> FillRandomField(int userId, [FromBody] stickerIdDto stickerId)
		{
			var success = await _boardService.FillRandomField(userId, stickerId);
			if (success != null)
			{
				return Ok(success);
			}
			return BadRequest();
		}
		
		[HttpPut("put")]
		public async Task<ActionResult<bool>> UpdateBoard([FromBody] Board board)
		{
			var success = await _boardService.UpdateBoard(board);
			if (success)
			{
				return Ok(success);
			}
			return BadRequest();
		}

		[HttpDelete("delete/{userId}")]
		public async Task<ActionResult<bool>> DeleteBoard(int userId)
		{
			var success = await _boardService.DeleteBoard(userId);
			if (success)
			{
				return Ok(success);
			}
			return BadRequest();
		}
	}
}