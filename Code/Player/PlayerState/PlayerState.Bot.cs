
namespace Facepunch;

public partial class PlayerState 
{
	/// <summary>
	/// Unique ID of this Bot
	/// </summary>
	[HostSync] public int BotId { get; set; } = -1;

	/// <summary>
	/// Is this a bot?
	/// </summary>
	public bool IsBot => BotId != -1;
}
