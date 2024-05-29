
using Facepunch;

/// <summary>
/// End the round if one team is eliminated.
/// </summary>
public sealed class TeamEliminated : Component, IRoundStartListener, IRoundEndCondition
{
	private bool _bothTeamsHadPlayers;

	/// <summary>
	/// Don't end the round if this team is eliminated.
	/// </summary>
	[Property, HostSync]
	public Team IgnoreTeam { get; set; }

	void IRoundStartListener.PostRoundStart()
	{
		_bothTeamsHadPlayers = GameUtils.GetPlayers( Team.CounterTerrorist ).Any()
			&& GameUtils.GetPlayers( Team.Terrorist ).Any();
	}

	private bool IsTeamEliminated( Team team )
	{
		return GameUtils.GetPlayers( team ).All( x => x.HealthComponent.State == LifeState.Dead );
	}

	public bool ShouldRoundEnd()
	{
		if ( !_bothTeamsHadPlayers && !GameUtils.InactivePlayers.Any() )
		{
			// Let you test stuff in single player
			return false;
		}

		return IgnoreTeam != Team.CounterTerrorist && IsTeamEliminated( Team.CounterTerrorist )
			|| IgnoreTeam != Team.Terrorist && IsTeamEliminated( Team.Terrorist );
	}
}
