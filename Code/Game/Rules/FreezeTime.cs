using Facepunch.UI;
using Facepunch;
using System.Threading.Tasks;

/// <summary>
/// Keep the players frozen for a few seconds at the start of each round.
/// </summary>
public sealed class FreezeTime : Component, IRoundStartListener
{
	[Property, Sync]
	public float DurationSeconds { get; set; } = 15f;

	[Sync]
	public float StartTime { get; set; }

	async Task IRoundStartListener.OnRoundStart()
	{
		StartTime = Time.Now;

		GameMode.Instance.ShowCountDownTimer( StartTime, DurationSeconds );
		GameMode.Instance.ShowStatusText( "Round starts in..." );

		while ( Time.Now < StartTime + DurationSeconds )
		{
			await Task.FixedUpdate();
		}

		GameMode.Instance.HideStatusText();
	}
}
