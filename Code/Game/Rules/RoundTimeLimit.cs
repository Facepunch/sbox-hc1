using Facepunch;
using Facepunch.UI;

/// <summary>
/// End the round after a fixed period of time.
/// </summary>
public sealed class RoundTimeLimit : Component, IRoundStartListener, IRoundEndCondition
{
	[Property, Sync] public float TimeLimitSeconds { get; set; } = 120f;

	[HostSync] public float StartTime { get; set; }

	void IRoundStartListener.PostRoundStart()
	{
		StartTime = Time.Now;

		GameMode.Instance.ShowCountDownTimer( StartTime, TimeLimitSeconds );
	}

	public bool ShouldRoundEnd()
	{
		return Time.Now >= StartTime + TimeLimitSeconds;
	}
}
