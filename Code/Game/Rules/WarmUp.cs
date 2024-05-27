using Facepunch.UI;
using Facepunch;
using System.Threading.Tasks;

internal class WarmUp : Component, IGameStartListener
{
	[Property, Sync]
	public float DurationSeconds { get; set; }

	[Sync]
	public float StartTime { get; set; }

	Task IGameStartListener.OnGameStart()
	{
		StartTime = Time.Now;

		return Task.DelaySeconds( DurationSeconds );
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
			display.Time = TimeSpan.FromSeconds( DurationSeconds - Time.Now + StartTime + 1f );
		}
	}
}
