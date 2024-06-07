using Facepunch;

/// <summary>
/// End the round after a fixed period of time.
/// </summary>
public sealed class RoundTimeLimit : Component, IRoundStartListener, IRoundEndCondition
{
	[Property, HostSync] public float TimeLimitSeconds { get; set; } = 120f;
	[Property, HostSync] public Team DefaultWinningTeam { get; set; }

	[HostSync] public float StartTime { get; set; }

	void IRoundStartListener.PostRoundStart()
	{
		StartTime = Time.Now;

		GameMode.Instance.ShowCountDownTimer( StartTime, TimeLimitSeconds );
	}

	public bool ShouldRoundEnd()
	{
		if ( Time.Now < StartTime + TimeLimitSeconds )
		{
			return false;
		}

		if ( GameMode.Instance.Components.GetInDescendantsOrSelf<RoundBasedTeamScoring>() is { } scoring )
		{
			scoring.RoundWinner = DefaultWinningTeam;
		}

		return true;
	}
}
