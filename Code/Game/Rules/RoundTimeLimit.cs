public sealed class RoundTimeLimit : Component, IRoundEndCondition
{
	[RequireComponent] public RoundTimer RoundTimer { get; private set; }

	[Property] public float TimeLimitSeconds { get; set; } = 120f;

	public bool ShouldRoundEnd()
	{
		return RoundTimer.SinceRoundStart >= TimeLimitSeconds;
	}
}

