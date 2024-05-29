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

		while ( Time.Now < StartTime + DurationSeconds )
		{
			await Task.FixedUpdate();
		}
	}

	protected override void OnUpdate()
	{
		if ( GameMode.Instance.State != GameState.PreRound )
		{
			return;
		}

		if ( GameUtils.GetHudPanel<RoundStateDisplay>() is { } display )
		{
			display.Status = "Round starts in...";
			display.Time = TimeSpan.FromSeconds( DurationSeconds - Time.Now + StartTime + 1f );
		}
	}
}
