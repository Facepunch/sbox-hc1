using Facepunch;
using Sandbox.Events;

/// <summary>
/// End the round after a fixed period of time.
/// </summary>
public sealed class RoundTimeLimit : Component,
	IGameEventHandler<PostRoundStartEvent>,
	IGameEventHandler<PreRoundEndEvent>,
	IGameEventHandler<DuringRoundEvent>
{
	[Property, HostSync] public float TimeLimitSeconds { get; set; } = 120f;
	[Property, HostSync] public Team DefaultWinningTeam { get; set; }

	[HostSync] public float StartTime { get; set; }

	void IGameEventHandler<PostRoundStartEvent>.OnGameEvent( PostRoundStartEvent eventArgs )
	{
		StartTime = Time.Now;

		GameMode.Instance.ShowCountDownTimer( StartTime, TimeLimitSeconds );
	}

	void IGameEventHandler<PreRoundEndEvent>.OnGameEvent( PreRoundEndEvent eventArgs )
	{
		GameMode.Instance.HideTimer();
	}

	void IGameEventHandler<DuringRoundEvent>.OnGameEvent( DuringRoundEvent eventArgs )
	{
		if ( Time.Now < StartTime + TimeLimitSeconds )
		{
			return;
		}

		if ( GameMode.Instance.Get<RoundBasedTeamScoring>() is { } scoring )
		{
			scoring.RoundWinner = DefaultWinningTeam;
		}

		GameMode.Instance.EndRound();
	}
}
