
using Facepunch;

public sealed class TeamDeathmatchScoring : Component, IKillListener
{
	[RequireComponent] public TeamScoring TeamScoring { get; private set; }

	public void OnPlayerKilled( Component killer, Component victim, float damage, Vector3 position, Vector3 force,
		Component inflictor = null, bool isHeadshot = false )
	{
		if ( GameUtils.GetPlayerFromComponent( killer ) is not { } killerPlayer )
		{
			return;
		}

		if ( GameUtils.GetPlayerFromComponent( victim ) is not { } victimPlayer )
		{
			return;
		}


	}
}
