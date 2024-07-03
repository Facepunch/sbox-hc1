using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Respawn all players at the start of this state.
/// </summary>
public sealed class RespawnPlayers : Component,
	IGameEventHandler<EnterStateEvent>
{
	/// <summary>
	/// If true, reset alive players as if they'd died.
	/// </summary>
	[Property]
	public bool ForceNew { get; set; }

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		GameUtils.LogPlayers();

		foreach ( var player in GameUtils.AllPlayers )
		{
			player.Respawn( ForceNew );
		}
	}
}
