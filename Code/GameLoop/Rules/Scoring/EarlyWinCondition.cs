using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Transition to the given state when one team's score reaches the given value.
/// </summary>
public sealed class TeamEarlyWinCondition : Component,
	IGameEventHandler<RoundCounterIncrementedEvent>,
	IGameEventHandler<TeamScoreIncrementedEvent>
{
	/// <summary>
	/// Transition when either team reaches this score.
	/// </summary>
	[Property, HostSync]
	public int TargetScore { get; set; } = 16;

	/// <summary>
	/// Transition to this state when <see cref="Team.Terrorist"/> reaches the target score.
	/// </summary>
	[Property]
	public StateComponent TerroristVictoryState { get; set; }

	/// <summary>
	/// Transition to this state when <see cref="Team.CounterTerrorist"/> reaches the target score.
	/// </summary>
	[Property]
	public StateComponent CounterTerroristVictoryState { get; set; }

	[Property] public bool MatchPoint { get; set; } = true;

	private TeamScoring TeamScoring => GameMode.Instance.Get<TeamScoring>( true );

	private int GetWonRounds( Team team )
	{
		return TeamScoring.Scores.GetValueOrDefault( team );
	}

	void IGameEventHandler<RoundCounterIncrementedEvent>.OnGameEvent( RoundCounterIncrementedEvent eventArgs )
	{
		if ( !MatchPoint )
			return;

		if ( GetWonRounds( Team.Terrorist ) == TargetScore - 1 || GetWonRounds( Team.CounterTerrorist ) == TargetScore - 1 )
		{
			Facepunch.UI.Toast.Instance.Show( "Match Point", Facepunch.UI.ToastType.Generic );
		}
	}

	void IGameEventHandler<TeamScoreIncrementedEvent>.OnGameEvent( TeamScoreIncrementedEvent eventArgs )
	{
		if ( GetWonRounds( Team.Terrorist ) == TargetScore && TerroristVictoryState is not null )
		{
			GameMode.Instance.StateMachine.Transition( TerroristVictoryState );
		}
		else if ( GetWonRounds( Team.CounterTerrorist ) == TargetScore && CounterTerroristVictoryState is not null )
		{
			GameMode.Instance.StateMachine.Transition( CounterTerroristVictoryState );
		}
	}
}
