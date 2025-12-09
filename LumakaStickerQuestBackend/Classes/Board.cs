
namespace LumakaStickerQuestBackend.Classes
{
	public class Board
	{
		/// <summary>
		/// Class to store information about the bingo board
		/// </summary>

		public int Id { get; set; }
		public int UserId { get; set; }
		public Field[] Fields { get; set; }
		public bool IsCompleted { get; set; }
	}
}