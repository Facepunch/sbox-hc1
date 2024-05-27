using Facepunch;
using Facepunch.UI;

public sealed class RoundTimeLimit : Component, IRoundEndCondition
{
	[RequireComponent] public RoundTimer RoundTimer { get; private set; }

	[Property] public float TimeLimitSeconds { get; set; } = 120f;

	public bool ShouldRoundEnd()
	{
		return RoundTimer.TimeSeconds >= TimeLimitSeconds;
	}

	protected override void OnUpdate()
	{
		if ( GameMode.Instance.State != GameState.DuringRound ) return;

		if ( GameUtils.GetHudPanel<RoundStateDisplay>() is { } display )
		{
			display.Status = null;
			display.Time = TimeSpan.FromSeconds( TimeLimitSeconds - RoundTimer.TimeSeconds );
		}
	}
}

