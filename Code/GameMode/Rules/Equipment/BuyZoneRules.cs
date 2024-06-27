using Sandbox.Events;

namespace Facepunch;

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

	[Early]
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
	IGameEventHandler<CanOpenBuyMenuEvent>
{
	public void OnGameEvent( CanOpenBuyMenuEvent eventArgs )
	{
		eventArgs.CanOpen = PlayerState.Local.PlayerPawn?.HealthComponent.IsGodMode ?? false;
	}
}
