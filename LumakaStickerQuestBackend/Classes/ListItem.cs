
namespace LumakaStickerQuestBackend.Classes
{
	public class ListItem
	{
		/// <summary>
		/// Class to store information of list items
		/// </summary>

		public int Id { get; set; }
		public string UserId { get; set; }
		public int Position { get; set; }
		public string Description { get; set; }
		public int Category {  get; set; }
		public bool CheckBox { get; set; }
		public int Points { get; set; }
	}
}