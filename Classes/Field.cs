
namespace LumakaStickerQuestBackend.Classes
{
	public class Field	
	{
		/// <summary>
		/// Class to store information about the fields of a bingo board
		/// </summary>

		public int Id { get; set; }
		public string Position { get; set; }
		public bool IsFilled { get; set; } // modify later for stickers?
	}
}