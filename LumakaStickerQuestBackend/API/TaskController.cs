using LumakaStickerQuestBackend.Classes;
using LumakaStickerQuestBackend.Functions;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LumakaStickerQuestBackend.API
{
	[ApiController]
	[Route("api/task")]
	public class TaskController : ControllerBase
	{
		private readonly Services.ListS _taskService;

		public TaskController(Services.ListS taskService)
		{
			_taskService = taskService;
		}

		[HttpPost]
		public async Task<ActionResult<object>> CreateTask([FromBody] TaskCreateRequest request)
		{
			if (request == null || request.UserId <= 0 || string.IsNullOrWhiteSpace(request.TaskDescription) || request.CategoryId <= 0)
			{
				return BadRequest();
			}

			var taskId = await _taskService.AddTask(request);
			if (taskId == null)
			{
				return BadRequest();
			}

			return Ok(new { task_id = taskId.Value });
		}

		[HttpGet("{userId:int}")]
		public async Task<ActionResult<IEnumerable<TaskResponse>>> GetTasksForUser(int userId)
		{
			if (userId <= 0)
			{
				return BadRequest();
			}

			var tasks = await _taskService.GetTasksByUserId(userId);
			return Ok(tasks);
		}

		[HttpDelete("{id:int}")]
		public async Task<IActionResult> DeleteTask(int id)
		{
			var success = await _taskService.DeleteTask(id);
			if (!success)
			{
				return NotFound();
			}

			return NoContent();
		}

		[HttpPut("{id:int}")]
		public async Task<ActionResult<object>> UpdateTaskCompletion(int id, [FromBody] TaskUpdateRequest request)
		{
			if (request == null)
			{
				return BadRequest();
			}

			var newPoints = await _taskService.UpdateTaskCompletion(id, request.IsCompleted);
			if (newPoints == null)
			{
				return NotFound();
			}

			return Ok(new { user_points = newPoints.Value });
		}
	}
}
