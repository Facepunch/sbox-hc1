using Facepunch.UI;
using Facepunch;
using System.Threading.Tasks;

/// <summary>
/// Wait for enough players to connect before starting, or start anyway if we waited too long.
/// </summary>
public sealed class WaitForPlayers : Component, IGameStartListener
{
	[DeveloperCommand( "Pause Game Start", "Pause / resume timer before game starts." )]
	public static void Toggle()
	{
		var inst = GameMode.Instance?.Components.GetInDescendantsOrSelf<WaitForPlayers>();

		if ( inst != null )
		{
			inst.IsPostponed = !inst.IsPostponed;
		}
	}

	[Property, HostSync]
	public float DurationSeconds { get; set; } = 60f;

	[Property, HostSync]
	public float MinDurationSeconds { get; set; } = 5f;

	[Property, HostSync]
	public int MinPlayerCount { get; set; } = 2;

	[HostSync]
	public bool IsPostponed { get; set; }

	[HostSync]
	public float StartTime { get; set; }

	public float Remaining => DurationSeconds - Time.Now + StartTime;

	async Task IGameStartListener.OnGameStart()
	{
		StartTime = Time.Now;

		while ( GameUtils.AllPlayers.Count() < MinPlayerCount && Remaining > 0f )
		{
			if ( IsPostponed )
			{
				StartTime = Time.Now;
			}

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
