
namespace LumakaStickerQuestBackend.Classes
{
	public class User
	{
		/// <summary>
		/// Class to store user data in
		/// </summary>
		
		public int Id { get; set; }
		public string Name { get; set; }
		public string Password { get; set; }
		public string Email { get; set; }
		public int Points { get; set; }
		public int[] Stickers { get; set; }
		public string Birthday { get; set; }
	}
}