namespace LumakaStickerQuestBackend.Classes
{
	public class TaskCreateRequest
	{
		public int UserId { get; set; }
		public string TaskDescription { get; set; }
		public int CategoryId { get; set; }
		public int Position { get; set; } = 0;
		public int PointsReward { get; set; } = 5;
	}

	public class TaskUpdateRequest
	{
		public bool IsCompleted { get; set; }
	}

	public class TaskResponse
	{
		public int TaskId { get; set; }
		public string TaskDescription { get; set; }
		public int CategoryId { get; set; }
		public int Position { get; set; }
		public int PointsReward { get; set; }
		public bool IsCompleted { get; set; }
	}
}
