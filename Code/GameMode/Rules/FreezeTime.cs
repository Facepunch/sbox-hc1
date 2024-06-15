using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Keep players frozen while this state is active.
/// </summary>
public sealed class FreezePlayers : Component,
	IGameEventHandler<EnterStateEvent>,
	IGameEventHandler<LeaveStateEvent>,
	IGameEventHandler<PlayerSpawnedEvent>
{
	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		foreach ( var player in GameUtils.ActivePlayers )
		{
			player.IsFrozen = true;
		}
	}

	void IGameEventHandler<LeaveStateEvent>.OnGameEvent( LeaveStateEvent eventArgs )
	{
		foreach ( var player in GameUtils.ActivePlayers )
		{
			player.IsFrozen = false;
		}
	}

	void IGameEventHandler<PlayerSpawnedEvent>.OnGameEvent( PlayerSpawnedEvent eventArgs )
	{
		eventArgs.Player.IsFrozen = true;
	}
}
