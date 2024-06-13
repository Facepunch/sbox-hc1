using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// While this state is active, players can buy anywhere.
/// </summary>
public sealed class BuyAnywhere : Component,
	IGameEventHandler<EnterStateEvent>,
	IGameEventHandler<LeaveStateEvent>,
	IGameEventHandler<PlayerSpawnedEvent>
{
	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		foreach ( var player in GameUtils.ActivePlayers )
		{
			player.CanBuyAnywhere = true;
		}
	}

	void IGameEventHandler<LeaveStateEvent>.OnGameEvent( LeaveStateEvent eventArgs )
	{
		foreach ( var player in GameUtils.ActivePlayers )
		{
			player.CanBuyAnywhere = false;
		}
	}

	void IGameEventHandler<PlayerSpawnedEvent>.OnGameEvent( PlayerSpawnedEvent eventArgs )
	{
		eventArgs.Player.CanBuyAnywhere = true;
	}
}

/// <summary>
/// Enable the buy menu at the start of this state, and optionally disable it after a time limit.
/// </summary>
public sealed class EnableBuyMenu : Component,
	IGameEventHandler<EnterStateEvent>,
	IGameEventHandler<UpdateStateEvent>,
	IGameEventHandler<LeaveStateEvent>,
	IGameEventHandler<PlayerSpawnedEvent>
{
	/// <summary>
	/// Disable the buy menu after this time limit, if greater than zero.
	/// </summary>
	[Property, HostSync]
	public float TimeLimit { get; set; }

	[HostSync]
	public float StartTime { get; private set; }

	public float DisableTime => TimeLimit <= 0f ? float.PositiveInfinity : StartTime + TimeLimit;

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		StartTime = Time.Now;
	}

	void IGameEventHandler<UpdateStateEvent>.OnGameEvent( UpdateStateEvent eventArgs )
	{
		var enabled = Time.Now < DisableTime;

		foreach ( var player in GameUtils.ActivePlayers )
		{
			player.BuyMenuEnabled = enabled;
		}
	}

	void IGameEventHandler<LeaveStateEvent>.OnGameEvent( LeaveStateEvent eventArgs )
	{
		foreach ( var player in GameUtils.ActivePlayers )
		{
			player.BuyMenuEnabled = false;
		}
	}

	void IGameEventHandler<PlayerSpawnedEvent>.OnGameEvent( PlayerSpawnedEvent eventArgs )
	{
		eventArgs.Player.BuyMenuEnabled = Time.Now < DisableTime;
	}
}

/// <summary>
/// Enable the buy menu for players that are spawn protected.
/// </summary>
public sealed class EnableBuyMenuDuringSpawnProtection : Component,
	IGameEventHandler<SpawnProtectionStart>,
	IGameEventHandler<SpawnProtectionEnd>,
	IGameEventHandler<LeaveStateEvent>
{
	public void OnGameEvent( SpawnProtectionStart eventArgs )
	{
		eventArgs.Player.CanBuyAnywhere = true;
		eventArgs.Player.BuyMenuEnabled = true;
	}

	public void OnGameEvent( SpawnProtectionEnd eventArgs )
	{
		eventArgs.Player.CanBuyAnywhere = false;
		eventArgs.Player.BuyMenuEnabled = false;
	}

	public void OnGameEvent( LeaveStateEvent eventArgs )
	{
		foreach ( var player in GameUtils.ActivePlayers )
		{
			player.CanBuyAnywhere = false;
			player.BuyMenuEnabled = false;
		}
	}
}
