using Sandbox.Events;

namespace Facepunch;

// TODO: Tony: Restore

/// <summary>
/// Transition to another state if one team is eliminated.
/// </summary>
public sealed class TeamEliminated : Component,
	IGameEventHandler<UpdateStateEvent>
{
	/// <summary>
	/// Transition to this state when all T players are eliminated.
	/// </summary>
	[Property]
	public StateComponent TerroristsEliminatedState { get; set; }

	/// <summary>
	/// Transition to this state when all CT players are eliminated.
	/// </summary>
	[Property]
	public StateComponent CounterTerroristsEliminatedState { get; set; }

	/// <summary>
	/// Transition to this state when all players are eliminated simultaneously.
	/// </summary>
	[Property]
	public StateComponent BothTeamsEliminatedState { get; set; }

	private bool IsTeamEliminated( TeamDefinition team )
	{
		return GameUtils.GetPlayerPawns( team ).All( x => x.HealthComponent.State == LifeState.Dead );
	}

	void IGameEventHandler<UpdateStateEvent>.OnGameEvent( UpdateStateEvent eventArgs )
	{
		var teamSetupTeams = TeamSetup.Instance.Teams;

		var ctsEliminated = IsTeamEliminated( teamSetupTeams[0] );
		var tsEliminated = IsTeamEliminated( teamSetupTeams[1] );

		if ( ctsEliminated && tsEliminated && BothTeamsEliminatedState is not null )
		{
			GameMode.Instance.StateMachine.Transition( BothTeamsEliminatedState );
		}
		else if ( ctsEliminated && CounterTerroristsEliminatedState is not null )
		{
			GameMode.Instance.StateMachine.Transition( CounterTerroristsEliminatedState );
		}
		else if ( tsEliminated && TerroristsEliminatedState is not null )
		{
			GameMode.Instance.StateMachine.Transition( TerroristsEliminatedState );
		}
	}
}
