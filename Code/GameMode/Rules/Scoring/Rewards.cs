using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Grants kill reward money.
/// </summary>
public sealed class KillRewards : Component, IGameEventHandler<KillEvent>
{
	[Property, HostSync]
	public bool AllowFriendlyFire { get; set; }

	[Property, HostSync, ShowIf( nameof(AllowFriendlyFire), false )]
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
			killerPlayer.PlayerState.GiveCash( -FriendlyFirePenalty );
		}
		else if ( damageInfo.Inflictor is Equipment weapon )
		{
			killerPlayer.PlayerState.GiveCash( weapon.Resource.KillReward );
		}
	}
}

public sealed class DefuseObjectiveRewards : Component,
	IGameEventHandler<BombPlantedEvent>,
	IGameEventHandler<BombDefusedEvent>
{
	[Property, HostSync]
	public int BombPlantReward { get; set; } = 300;

	[Property, HostSync]
	public int BombDefuseReward { get; set; } = 300;

	/// <summary>
	/// Team reward for <see cref="Team.Terrorist"/>s if the bomb was planted but the round was lost.
	/// </summary>
	[Property, HostSync]
	public int BombPlantTeamReward { get; set; } = 800;

	void IGameEventHandler<BombPlantedEvent>.OnGameEvent( BombPlantedEvent eventArgs )
	{
		eventArgs.Planter?.PlayerState.GiveCash( BombPlantReward );
	}

	void IGameEventHandler<BombDefusedEvent>.OnGameEvent( BombDefusedEvent eventArgs )
	{
		eventArgs.Defuser?.PlayerState.GiveCash( BombDefuseReward );

		foreach ( var player in GameUtils.AllPlayers.Where( x => x.Team is not null && x.Team.Tags.Has( "t" ) ) )
		{
			player.GiveCash( BombPlantTeamReward );
		}
	}
}
