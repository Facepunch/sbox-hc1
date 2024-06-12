using Facepunch;
using Sandbox.Events;

public sealed class TeamDeathmatchScoring : Component,
	IGameEventHandler<KillEvent>,
	IPlayerSpawnListener,
	IGameEventHandler<PostRoundStartEvent>
{
	[RequireComponent] public TeamScoring TeamScoring { get; private set; }

	[After<FreezeTime>]
	void IGameEventHandler<PostRoundStartEvent>.OnGameEvent( PostRoundStartEvent eventArgs )
	{
		GameMode.Instance.ShowStatusText( "Deathmatch" );
	}

	void IGameEventHandler<KillEvent>.OnGameEvent( KillEvent eventArgs )
	{
		var damageInfo = eventArgs.DamageInfo;

		if ( GameUtils.GetPlayerFromComponent( damageInfo.Attacker ) is not { } killerPlayer )
			return;

		if ( GameUtils.GetPlayerFromComponent( damageInfo.Victim ) is not { } victimPlayer )
			return;

		if ( killerPlayer.IsFriendly( victimPlayer ) )
			return;

		if ( killerPlayer.TeamComponent.Team == Team.Unassigned )
			return;

		if ( victimPlayer.TeamComponent.Team == Team.Unassigned )
			return;

		TeamScoring.IncrementScore( killerPlayer.TeamComponent.Team );
	}
}
