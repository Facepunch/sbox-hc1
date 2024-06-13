using Sandbox.Events;

namespace Facepunch;

public record TeamScoreIncrementedEvent( Team Team, int Score ) : IGameEvent;

/// <summary>
/// Keeps track of a score for each team.
/// </summary>
public sealed class TeamScoring : Component,
	IGameEventHandler<ResetScoresEvent>,
	IGameEventHandler<TeamsSwappedEvent>
{
	[HostSync]
	public NetDictionary<Team, int> Scores { get; private set; } = new();

	public int MyTeamScore => Scores.GetValueOrDefault( GameUtils.LocalPlayer?.GameObject.GetTeam() ?? Team.Unassigned );
	public int OpposingTeamScore => Scores.GetValueOrDefault( GameUtils.LocalPlayer?.GameObject.GetTeam().GetOpponents() ?? Team.Unassigned );

	void IGameEventHandler<ResetScoresEvent>.OnGameEvent( ResetScoresEvent eventArgs )
	{
		Scores.Clear();
	}

	public void IncrementScore( Team team, int amount = 1 )
	{
		var score = Scores.GetValueOrDefault( team ) + amount;

		Scores[team] = score;

		Scene.Dispatch( new TeamScoreIncrementedEvent( team, score ) );
	}

	public void Flip()
	{
		var ctScores = Scores.GetValueOrDefault( Team.CounterTerrorist );
		var tScores = Scores.GetValueOrDefault( Team.Terrorist );

		Scores[Team.Terrorist] = ctScores;
		Scores[Team.CounterTerrorist] = tScores;
	}

	void IGameEventHandler<TeamsSwappedEvent>.OnGameEvent( TeamsSwappedEvent eventArgs )
	{
		Flip();
	}
}
