using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LumakaStickerQuestBackend.Classes
{
	public class UpdateStickersRequest
	{
		[JsonPropertyName("stickers")]
		public List<int> stickers { get; set; }
	}
}
