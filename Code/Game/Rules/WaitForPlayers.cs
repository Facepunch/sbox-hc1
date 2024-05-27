using Facepunch.UI;
using Facepunch;
using System.Threading.Tasks;

internal class WaitForPlayers : Component, IGameStartListener
{
	[Property, Sync]
	public float DurationSeconds { get; set; } = 60f;

	[Property, Sync]
	public float MinDurationSeconds { get; set; } = 5f;

	[Property, Sync]
	public int MinPlayerCount { get; set; } = 2;

	[Sync]
	public float StartTime { get; set; }

	public float Remaining => DurationSeconds - Time.Now + StartTime;

	async Task IGameStartListener.OnGameStart()
	{
		StartTime = Time.Now;

		while ( GameUtils.AllPlayers.Count() < MinPlayerCount && Remaining > 0f)
		{
			await Task.DelaySeconds( 1f );
		}

		if ( Remaining > MinDurationSeconds )
		{
			StartTime = Time.Now - DurationSeconds + MinDurationSeconds;
		}

		if ( Remaining > 0f )
		{
			await Task.DelaySeconds( Remaining );
		}
	}

	protected override void OnUpdate()
	{
		if ( GameMode.Instance.State != GameState.PreGame )
		{
			return;
		}

		if ( GameUtils.GetHudPanel<RoundStateDisplay>() is { } display )
		{
			display.Status = "Waiting for players...";
			display.Time = TimeSpan.FromSeconds( Remaining + 1f );
		}
	}
}
