using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Reset player cash to the given amount when entering this state, or when they are assigned to a team.
/// </summary>
public sealed class ResetBalance : Component,
	IGameEventHandler<EnterStateEvent>,
	IGameEventHandler<TeamAssignedEvent>
{
	[Property]
	public int Value { get; set; } = 800;

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		foreach ( var player in GameUtils.ActivePlayers )
		{
			player.Inventory.SetCash( Value );
		}
	}

	void IGameEventHandler<TeamAssignedEvent>.OnGameEvent( TeamAssignedEvent eventArgs )
	{
		eventArgs.Player.Inventory.SetCash( Value );
	}
}
