namespace Facepunch;

public sealed partial class PlayerController
{
	/// <summary>
	/// Unique ID of this Bot
	/// </summary>
	[HostSync] public int BotId { get; set; } = -1;

	/// <summary>
	/// Development: should bots follow the player's input?
	/// </summary>
	[ConVar( "hc1_bot_follow" )] public static bool BotFollowHostInput { get; set; }

	/// <summary>
	/// Is this a bot?
	/// </summary>
	public bool IsBot => BotId != -1;
}
