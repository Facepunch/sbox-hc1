using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Enable the buy menu at the start of this state, and optionally disable it after a time limit.
/// </summary>
public sealed class EnableBuyMenu : Component,
	IGameEventHandler<EnterStateEvent>,
	IGameEventHandler<UpdateStateEvent>,
	IGameEventHandler<LeaveStateEvent>
{
	/// <summary>
	/// Disable the buy menu after this time limit, if greater than zero.
	/// </summary>
	[Property, Sync( SyncFlags.FromHost )]
	public float TimeLimit { get; set; }

	[Sync( SyncFlags.FromHost )]
	public float StartTime { get; private set; }

	public float DisableTime => TimeLimit <= 0f ? float.PositiveInfinity : StartTime + TimeLimit;

	[Property] public bool InBuyZoneOnly { get; set; }

	public BuyMenuMode BuyMenuMode => Time.Now < DisableTime
		? InBuyZoneOnly ? BuyMenuMode.EnabledInBuyZone : BuyMenuMode.EnabledEverywhere
		: BuyMenuMode.Disabled;

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		StartTime = Time.Now;
	}

	void IGameEventHandler<UpdateStateEvent>.OnGameEvent( UpdateStateEvent eventArgs )
	{
		var mode = BuyMenuMode;

		foreach ( var player in GameUtils.AllPlayers )
		{
			player.BuyMenuMode = mode;
		}
	}

	void IGameEventHandler<LeaveStateEvent>.OnGameEvent( LeaveStateEvent eventArgs )
	{
		foreach ( var player in GameUtils.AllPlayers )
		{
			player.BuyMenuMode = BuyMenuMode.Disabled;
		}
	}
}

/// <summary>
/// Enable the buy menu for players that are spawn protected.
/// </summary>
public sealed class EnableBuyMenuDuringSpawnProtection : Component,
	IGameEventHandler<UpdateStateEvent>,
	IGameEventHandler<LeaveStateEvent>
{
	void IGameEventHandler<UpdateStateEvent>.OnGameEvent( UpdateStateEvent eventArgs )
	{
		foreach ( var player in GameUtils.AllPlayers )
		{
			player.BuyMenuMode = player.PlayerPawn is { HealthComponent: { IsGodMode: true } }
				? BuyMenuMode.EnabledEverywhere
				: BuyMenuMode.Disabled;
		}
	}

	void IGameEventHandler<LeaveStateEvent>.OnGameEvent( LeaveStateEvent eventArgs )
	{
		foreach ( var player in GameUtils.AllPlayers )
		{
			player.BuyMenuMode = BuyMenuMode.Disabled;
		}
	}
}
