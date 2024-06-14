using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// While this state is active, players can buy anywhere.
/// </summary>
public sealed class BuyAnywhere : Component, IGameEventHandler<CanOpenBuyMenuEvent>
{
	[Late]
	void IGameEventHandler<CanOpenBuyMenuEvent>.OnGameEvent( CanOpenBuyMenuEvent eventArgs )
	{
		eventArgs.CanOpen = true;
	}
}

/// <summary>
/// Enable the buy menu at the start of this state, and optionally disable it after a time limit.
/// </summary>
public sealed class EnableBuyMenu : Component,
	IGameEventHandler<EnterStateEvent>,
	IGameEventHandler<CanOpenBuyMenuEvent>
{
	/// <summary>
	/// Disable the buy menu after this time limit, if greater than zero.
	/// </summary>
	[Property, HostSync]
	public float TimeLimit { get; set; }

	[HostSync]
	public float StartTime { get; private set; }

	public float DisableTime => TimeLimit <= 0f ? float.PositiveInfinity : StartTime + TimeLimit;

	[Property] public bool InBuyZoneOnly { get; set; }

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		StartTime = Time.Now;
	}

	void IGameEventHandler<CanOpenBuyMenuEvent>.OnGameEvent( CanOpenBuyMenuEvent eventArgs )
	{ 
		var enabled = Time.Now < DisableTime;
		eventArgs.CanOpen = enabled;
		if ( InBuyZoneOnly && !BuySystem.IsInBuyZone() ) eventArgs.CanOpen = false; 
	}
}

/// <summary>
/// Enable the buy menu for players that are spawn protected.
/// </summary>
public sealed class EnableBuyMenuDuringSpawnProtection : Component,
	IGameEventHandler<SpawnProtectionStart>,
	IGameEventHandler<SpawnProtectionEnd>,
	IGameEventHandler<LeaveStateEvent>,
	IGameEventHandler<CanOpenBuyMenuEvent>
{
	bool InSpawnProtection { get; set; } = false;

	public void OnGameEvent( SpawnProtectionStart eventArgs )
	{
		InSpawnProtection = true;
	}

	public void OnGameEvent( SpawnProtectionEnd eventArgs )
	{
		InSpawnProtection = false;
	}

	public void OnGameEvent( LeaveStateEvent eventArgs )
	{
		InSpawnProtection = false;
	}

	public void OnGameEvent( CanOpenBuyMenuEvent eventArgs )
	{
		eventArgs.CanOpen = InSpawnProtection;
	}
}
