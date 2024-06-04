
using Facepunch;

public sealed class TeamDeathmatchScoring : Component, IKillListener, IPlayerSpawnListener, IRoundStartListener
{
	[RequireComponent] public TeamScoring TeamScoring { get; private set; }

	void IRoundStartListener.PostRoundStart()
	{
		GameMode.Instance.ShowStatusText( "Deathmatch" );
	}

	void IKillListener.OnPlayerKilled( Component killer, Component victim, float damage, Vector3 position, Vector3 force,
		Component inflictor = null, string hitbox = "" )
	{
		if ( GameUtils.GetPlayerFromComponent( killer ) is not { } killerPlayer )
		{
			return;
		}

		if ( GameUtils.GetPlayerFromComponent( victim ) is not { } victimPlayer )
		{
			return;
		}

		if ( killerPlayer.TeamComponent.Team == victimPlayer.TeamComponent.Team )
		{
			return;
		}

		if ( killerPlayer.TeamComponent.Team == Team.Unassigned )
		{
			return;
		}

		if ( victimPlayer.TeamComponent.Team == Team.Unassigned )
		{
			return;
		}

		TeamScoring.IncrementScore( killerPlayer.TeamComponent.Team );
	}
}
