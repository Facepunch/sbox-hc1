using Sandbox.Events;

namespace Facepunch;

public sealed class RoundBasedTeamScoring : Component,
	IGameEventHandler<ResetScoresEvent>
{
	[Sync( SyncFlags.FromHost )] public NetList<Team> RoundWinHistory { get; private set; } = new();

	public void AddRoundResult( Team team )
	{
		RoundWinHistory.Add( team );
	}

	void IGameEventHandler<ResetScoresEvent>.OnGameEvent( ResetScoresEvent eventArgs )
	{
		RoundWinHistory.Clear();
	}
}

/// <summary>
/// Increment a team's score when entering this state.
/// </summary>
public sealed class IncrementTeamScore : Component,
	IGameEventHandler<EnterStateEvent>
{
	[Property, Sync( SyncFlags.FromHost )]
	public Team Team { get; set; }

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		if ( Team is Team.Unassigned )
		{
			return;
		}

		var teamScoring = GameMode.Instance.Get<TeamScoring>();
		var roundBasedTeamScoring = GameMode.Instance.Get<RoundBasedTeamScoring>();

		teamScoring?.IncrementScore( Team );
		roundBasedTeamScoring?.AddRoundResult( Team );
	}
}
