using Facepunch;
using Sandbox.Events;

namespace Facepunch.GameRules;

/// <summary>
/// Keep players frozen while this state is active.
/// </summary>
public sealed class FreezePlayers : Component,
	IGameEventHandler<EnterStateEventArgs>,
	IGameEventHandler<LeaveStateEventArgs>,
	IGameEventHandler<PlayerSpawnedEvent>
{
	void IGameEventHandler<EnterStateEventArgs>.OnGameEvent( EnterStateEventArgs eventArgs )
	{
		foreach ( var player in GameUtils.ActivePlayers )
		{
			player.IsFrozen = true;
		}
	}

	void IGameEventHandler<LeaveStateEventArgs>.OnGameEvent( LeaveStateEventArgs eventArgs )
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
