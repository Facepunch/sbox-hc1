using Facepunch.UI;
using Sandbox.Events;
using Sandbox.Utility;
using System.Collections.Generic;

namespace Facepunch;

public record TeamScoreIncrementedEvent( TeamDefinition Team, int Score ) : IGameEvent;

/// <summary>
/// Keeps track of a score for each team.
/// </summary>
public sealed class TeamScoring : Component,
	IGameEventHandler<ResetScoresEvent>,
	IGameEventHandler<TeamsSwappedEvent>
{
	[HostSync]
	public NetDictionary<TeamDefinition, int> Scores { get; private set; } = new();

	/// <summary>
	/// What should we set the initial scores to?
	/// </summary>
	[Property] public int InitialScores { get; set; } = 0;

	/// <summary>
	/// You can define a scoring format for the scores, say you want to format in currency.
	/// </summary>
	[HostSync] public string ScoreFormat { get; set; } = "";
	[HostSync] public string ScorePrefix { get; set; } = "";

	public int MyTeamScore => Scores.GetValueOrDefault( PlayerState.Local.Team );
	public int OpposingTeamScore => Scores.GetValueOrDefault( PlayerState.Local.Team.GetOpponents() );

	public string MyTeamScoreFormatted => $"{ScorePrefix}{MyTeamScore.ToString( ScoreFormat )}";
	public string OpposingTeamScoreFormatted => $"{ScorePrefix}{OpposingTeamScore.ToString( ScoreFormat )}";

	void IGameEventHandler<ResetScoresEvent>.OnGameEvent( ResetScoresEvent eventArgs )
	{
		Scores.Clear();

		SetInitial();
	}

	public TeamDefinition GetHighest()
	{
		var highest = Scores.OrderByDescending( kv => kv.Value ).First();
		return highest.Key;
	}

	public void SetInitial()
	{
		if ( InitialScores != 0 )
		{
			foreach ( var team in GameMode.Instance.Get<TeamSetup>().Teams )
			{
				Scores[team] = InitialScores;
			}
		}
	}

	public string GetFormattedScore( TeamDefinition team )
	{
		return Scores.GetValueOrDefault( team, 0 ).ToString( ScoreFormat );
	}

	public void IncrementScore( TeamDefinition team, int amount = 1 )
	{
		var score = Scores.GetValueOrDefault( team ) + amount;

		Scores[team] = score;

		Scene.Dispatch( new TeamScoreIncrementedEvent( team, score ) );
	}

	public void Flip()
	{
		// TODO: Tony: This needs to be restored

		//var ctScores = Scores.GetValueOrDefault( Team.CounterTerrorist );
		//var tScores = Scores.GetValueOrDefault( Team.Terrorist );

		//Scores[Team.Terrorist] = ctScores;
		//Scores[Team.CounterTerrorist] = tScores;
	}

	void IGameEventHandler<TeamsSwappedEvent>.OnGameEvent( TeamsSwappedEvent eventArgs )
	{
		Flip();
	}

	protected override void OnStart()
	{
		SetInitial();
	}
}

/// <summary>
/// Transitions to specified states if one team's score is higher than the other's.
/// </summary>
public sealed class CompareTeamScores : Component,
	IGameEventHandler<EnterStateEvent>,
	IGameEventHandler<UpdateStateEvent>
{
	/// <summary>
	/// Minimum difference between scores to trigger a transition.
	/// </summary>
	[Property]
	public int MinMargin { get; set; } = 1;

	[Property]
	public List<TeamBasedState> VictoryStates { get; set; }

	private void CheckScores()
	{
		var teamScoring = GameMode.Instance.Get<TeamScoring>( true );
		var teamSetupTeams = TeamSetup.Instance.Teams;

		if ( TeamSetup.Instance.Teams.Count() != 2 )
		{
			Log.Warning( "TeamScoring doesn't work for count above 2 right now" );
			return;
		}

		var firstScore = teamScoring.Scores.GetValueOrDefault( teamSetupTeams[0] );
		var secondScore = teamScoring.Scores.GetValueOrDefault( teamSetupTeams[1] );

		if ( secondScore >= firstScore + MinMargin )
		{
			var x = VictoryStates.FirstOrDefault( x => x.Team == teamSetupTeams[0] );
			GameMode.Instance.StateMachine.Transition( x.State );
		}
		else if ( firstScore >= secondScore + MinMargin )
		{
			var x = VictoryStates.FirstOrDefault( x => x.Team == teamSetupTeams[1] );
			GameMode.Instance.StateMachine.Transition( x.State );
		}
	}

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		CheckScores();
	}

	void IGameEventHandler<UpdateStateEvent>.OnGameEvent( UpdateStateEvent eventArgs )
	{
		CheckScores();
	}
}
