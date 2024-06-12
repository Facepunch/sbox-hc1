
using Facepunch;
using Sandbox.Events;

/// <summary>
/// End the round if one team is eliminated.
/// </summary>
public sealed class TeamEliminated : Component,
	IGameEventHandler<PostRoundStartEvent>,
	IGameEventHandler<DuringRoundEvent>
{
	[RequireComponent] public RoundBasedTeamScoring Scoring { get; private set; }

	private bool _bothTeamsHadPlayers;

	/// <summary>
	/// Don't end the round if this team is eliminated.
	/// </summary>
	[Property, HostSync]
	public Team IgnoreTeam { get; set; }

	void IGameEventHandler<PostRoundStartEvent>.OnGameEvent( PostRoundStartEvent eventArgs )
	{
		_bothTeamsHadPlayers = GameUtils.GetPlayers( Team.CounterTerrorist ).Any()
			&& GameUtils.GetPlayers( Team.Terrorist ).Any();
	}

	private bool IsTeamEliminated( Team team )
	{
		return GameUtils.GetPlayers( team ).All( x => x.HealthComponent.State == LifeState.Dead );
	}

	private static Team GetRoundWinner( bool ctsEliminated, bool tsEliminated )
	{
		return ctsEliminated && tsEliminated ? Team.Unassigned : ctsEliminated ? Team.Terrorist : Team.CounterTerrorist;
	}

	void IGameEventHandler<DuringRoundEvent>.OnGameEvent( DuringRoundEvent eventArgs )
	{
		if ( !_bothTeamsHadPlayers && !GameUtils.InactivePlayers.Any() )
		{
			// Let you test stuff in single player
			return;
		}

		var ctsEliminated = IgnoreTeam != Team.CounterTerrorist && IsTeamEliminated( Team.CounterTerrorist );
		var tsEliminated = IgnoreTeam != Team.Terrorist && IsTeamEliminated( Team.Terrorist );

		if ( !ctsEliminated && !tsEliminated )
		{
			return;
		}

		Scoring.RoundWinner = GetRoundWinner( ctsEliminated, tsEliminated );

		GameMode.Instance.EndRound();
	}
}
