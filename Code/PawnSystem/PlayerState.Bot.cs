
using System.Text.Json.Serialization;

namespace Facepunch;

public partial class PlayerState 
{
	/// <summary>
	/// Unique ID of this Bot
	/// </summary>
	[Sync( SyncFlags.FromHost )] public int BotId { get; set; } = -1;

	/// <summary>
	/// Is this a bot?
	/// </summary>
	[Property, ReadOnly, JsonIgnore] public bool IsBot => BotId != -1;
}
