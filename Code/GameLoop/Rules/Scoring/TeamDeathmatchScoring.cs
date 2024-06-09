using Facepunch;

public sealed class TeamDeathmatchScoring : Component, IKillListener, IPlayerSpawnListener, IRoundStartListener
{
	[RequireComponent] public TeamScoring TeamScoring { get; private set; }

	void IRoundStartListener.PostRoundStart()
	{
		GameMode.Instance.ShowStatusText( "Deathmatch" );
	}

	void IKillListener.OnPlayerKilled( DamageEvent damageEvent )
	{
		if ( GameUtils.GetPlayerFromComponent( damageEvent.Attacker ) is not { } killerPlayer )
			return;

		if ( GameUtils.GetPlayerFromComponent( damageEvent.Victim ) is not { } victimPlayer )
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
