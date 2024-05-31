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
		GameMode.Instance.HideTimer();

		return Task.DelaySeconds( DurationSeconds );
	}
}
