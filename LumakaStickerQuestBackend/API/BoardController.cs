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

		[HttpGet("get/{id}")]
		public async Task<ActionResult<Board>> GetBoard(int id)
		{
			var board = await _boardService.GetBoard(id);
			if (board == null)
			{
				return NotFound();
			}
			return Ok(board);
		}
	}
}