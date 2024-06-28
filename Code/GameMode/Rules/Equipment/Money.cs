using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Starting money for new players.
/// </summary>
public sealed class DefaultBalance : Component,
	IGameEventHandler<TeamAssignedEvent>
{
	[Property]
	public int Value { get; set; } = 800;

	void IGameEventHandler<TeamAssignedEvent>.OnGameEvent( TeamAssignedEvent eventArgs )
	{
		eventArgs.Player.SetCash( Value );
	}
}

/// <summary>
/// Reset player cash to the given amount.
/// </summary>
public sealed class ResetBalance : Component,
	IGameEventHandler<EnterStateEvent>
{
	[Property]
	public int Value { get; set; } = 800;

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		foreach ( var player in GameUtils.PlayerPawns )
		{
			player.PlayerState.SetCash( Value );
		}
	}
}

/// <summary>
/// Event dispatched when a team is granted income.
/// The income value can be modified by event handlers.
/// </summary>
public record TeamIncomeEvent( Team Team ) : IGameEvent
{
	public int Value { get; set; }
}

/// <summary>
/// Give the specified team the given amount of income.
/// </summary>
public sealed class GiveTeamIncome : Component,
	IGameEventHandler<EnterStateEvent>
{
	[Property]
	public Team Team { get; set; }

	[Property]
	public int Value { get; set; } = 3250;

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		var incomeEventArgs = new TeamIncomeEvent( Team ) { Value = Value };

		Scene.Dispatch( incomeEventArgs );

		foreach ( var player in GameUtils.GetPlayerStates( Team ) )
		{
			player.GiveCash( incomeEventArgs.Value );
		}
	}
}

public sealed class LossBonus : Component,
	IGameEventHandler<TeamIncomeEvent>,
	IGameEventHandler<ResetScoresEvent>,
	IGameEventHandler<TeamsSwappedEvent>,
	IGameEventHandler<TeamScoreIncrementedEvent>
{
	[Property]
	public int MaxRounds { get; set; } = 4;

	[Property]
	public int ValuePerRound { get; set; } = 500;

	[HostSync]
	public NetDictionary<Team, int> CurrentLossBonus { get; private set; } = new();

	[HostSync]
	public Team LastWinningTeam { get; private set; }

	void IGameEventHandler<TeamIncomeEvent>.OnGameEvent( TeamIncomeEvent eventArgs )
	{
		if ( eventArgs.Team == LastWinningTeam )
		{
			return;
		}

		var rounds = CurrentLossBonus.GetValueOrDefault( eventArgs.Team );

		eventArgs.Value += rounds * ValuePerRound;

		CurrentLossBonus[eventArgs.Team] = Math.Min( MaxRounds, CurrentLossBonus.GetValueOrDefault( eventArgs.Team ) + 1 );
	}

	void IGameEventHandler<ResetScoresEvent>.OnGameEvent( ResetScoresEvent eventArgs )
	{
		CurrentLossBonus.Clear();
		LastWinningTeam = Team.Unassigned;
	}

	void IGameEventHandler<TeamsSwappedEvent>.OnGameEvent( TeamsSwappedEvent eventArgs )
	{
		CurrentLossBonus.Clear();
		LastWinningTeam = Team.Unassigned;
	}

	void IGameEventHandler<TeamScoreIncrementedEvent>.OnGameEvent( TeamScoreIncrementedEvent eventArgs )
	{
		LastWinningTeam = eventArgs.Team;
		CurrentLossBonus[eventArgs.Team] = Math.Max( 0, CurrentLossBonus.GetValueOrDefault( eventArgs.Team ) - 1 );
	}
}
