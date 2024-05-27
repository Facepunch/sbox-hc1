using System.Threading.Tasks;
using Facepunch;
using Facepunch.UI;

/// <summary>
/// Wait a bit before starting the next round.
/// </summary>
public class NextRoundDelay : Component, IRoundEndListener
{
	[Property, Sync]
	public float DurationSeconds { get; set; } = 5f;

	Task IRoundEndListener.OnRoundEnd()
	{
		return Task.DelaySeconds( DurationSeconds );
	}

	protected override void OnUpdate()
	{
		if ( GameMode.Instance.State != GameState.PostRound )
		{
			return;
		}

		if ( GameUtils.GetHudPanel<RoundStateDisplay>() is { } display )
		{
			display.Time = null;
			display.Status = "Round Over!";
		}
	}
}
