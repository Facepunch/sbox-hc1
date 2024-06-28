using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Transition to another state if one team is eliminated.
/// </summary>
public sealed class TeamEliminated : Component,
	IGameEventHandler<EnterStateEvent>,
	IGameEventHandler<UpdateStateEvent>
{
	private bool _bothTeamsHadPlayers;

	/// <summary>
	/// Transition to this state when all <see cref="Team.Terrorist"/> players are eliminated.
	/// </summary>
	[Property]
	public StateComponent TerroristsEliminatedState { get; set; }

	/// <summary>
	/// Transition to this state when all <see cref="Team.CounterTerrorist"/> players are eliminated.
	/// </summary>
	[Property]
	public StateComponent CounterTerroristsEliminatedState { get; set; }

	/// <summary>
	/// Transition to this state when all players are eliminated simultaneously.
	/// </summary>
	[Property]
	public StateComponent BothTeamsEliminatedState { get; set; }

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		_bothTeamsHadPlayers = GameUtils.GetPlayerPawns( Team.CounterTerrorist ).Any()
			&& GameUtils.GetPlayerPawns( Team.Terrorist ).Any();
	}

	private bool IsTeamEliminated( Team team )
	{
		return GameUtils.GetPlayerPawns( team ).All( x => x.HealthComponent.State == LifeState.Dead );
	}

	void IGameEventHandler<UpdateStateEvent>.OnGameEvent( UpdateStateEvent eventArgs )
	{
		if ( !_bothTeamsHadPlayers && !GameUtils.GetPlayerStates( Team.Unassigned ).Any() )
		{
			// Let you test stuff in single player
			return;
		}

		var ctsEliminated = IsTeamEliminated( Team.CounterTerrorist );
		var tsEliminated = IsTeamEliminated( Team.Terrorist );

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
