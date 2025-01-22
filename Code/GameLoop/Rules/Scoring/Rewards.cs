using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Grants kill reward money.
/// </summary>
public sealed class KillRewards : Component, IGameEventHandler<KillEvent>
{
	[Property, Sync( SyncFlags.FromHost )]
	public bool AllowFriendlyFire { get; set; }

	[Property, Sync( SyncFlags.FromHost ), ShowIf( nameof(AllowFriendlyFire), false )]
	public int FriendlyFirePenalty { get; set; } = 300;

	void IGameEventHandler<KillEvent>.OnGameEvent( KillEvent eventArgs )
	{
		if ( !Networking.IsHost )
			return;

		var damageInfo = eventArgs.DamageInfo;

		if ( GameUtils.GetPlayerFromComponent( damageInfo.Attacker ) is not { } killerPlayer )
			return;

		if ( GameUtils.GetPlayerFromComponent( damageInfo.Victim ) is not { } victimPlayer )
			return;

		if ( !AllowFriendlyFire && killerPlayer.IsFriendly( victimPlayer ) )
		{
			killerPlayer.Client.GiveCash( -FriendlyFirePenalty );
		}
		else if ( damageInfo.Inflictor is Equipment weapon )
		{
			killerPlayer.Client.GiveCash( weapon.Resource.KillReward );
		}
	}
}

public sealed class DefuseObjectiveRewards : Component,
	IGameEventHandler<BombPlantedEvent>,
	IGameEventHandler<BombDefusedEvent>
{
	[Property, Sync( SyncFlags.FromHost )]
	public int BombPlantReward { get; set; } = 300;

	[Property, Sync( SyncFlags.FromHost )]
	public int BombDefuseReward { get; set; } = 300;

	/// <summary>
	/// Team reward for <see cref="Team.Terrorist"/>s if the bomb was planted but the round was lost.
	/// </summary>
	[Property, Sync( SyncFlags.FromHost )]
	public int BombPlantTeamReward { get; set; } = 800;

	void IGameEventHandler<BombPlantedEvent>.OnGameEvent( BombPlantedEvent eventArgs )
	{
		eventArgs.Planter?.Client.GiveCash( BombPlantReward );
	}

	void IGameEventHandler<BombDefusedEvent>.OnGameEvent( BombDefusedEvent eventArgs )
	{
		eventArgs.Defuser?.Client.GiveCash( BombDefuseReward );

		foreach ( var player in GameUtils.GetPlayers( Team.Terrorist ) )
		{
			player.GiveCash( BombPlantTeamReward );
		}
	}
}
