using Facepunch;
using System.Threading.Tasks;

/// <summary>
/// Wait for enough players to connect before starting, or start anyway if we waited too long.
/// </summary>
public sealed class WaitForPlayers : Component, IGameStartListener
{
	[DeveloperCommand( "Pause Game Start", "Pause / resume timer before game starts." )]
	public static void DevToggle()
	{
		GameMode.Instance
			?.Components.GetInDescendantsOrSelf<WaitForPlayers>()
			?.Toggle();
	}

	[Property, HostSync]
	public float DurationSeconds { get; set; } = 60f;

	[Property, HostSync]
	public float GameStartDelaySeconds { get; set; } = 5f;

	[Property, HostSync]
	public int MinPlayerCount { get; set; } = 2;

	[HostSync]
	public bool IsPostponed { get; set; }

	[HostSync]
	public float StartTime { get; set; }

	public float Remaining => DurationSeconds - Time.Now + StartTime;

	async Task IGameStartListener.OnGameStart()
	{
		Restart();

		while ( IsPostponed || GameUtils.AllPlayers.Count() < MinPlayerCount && Remaining > 0f )
		{
			await Task.DelaySeconds( 1f );
		}

		GameMode.Instance.ShowStatusText( "Starting..." );
		GameMode.Instance.ShowCountDownTimer( Time.Now, GameStartDelaySeconds );

		await Task.DelaySeconds( GameStartDelaySeconds );
	}

	private void Toggle()
	{
		if ( IsPostponed ) Restart();
		else Postpone();
	}

	private void Postpone()
	{
		IsPostponed = true;

		GameMode.Instance.ShowStatusText( "Paused" );
		GameMode.Instance.HideTimer();
	}

	private void Restart()
	{
		StartTime = Time.Now;
		IsPostponed = false;

		GameMode.Instance.ShowStatusText( "Waiting..." );
		GameMode.Instance.ShowCountDownTimer( StartTime, DurationSeconds );
	}
}
