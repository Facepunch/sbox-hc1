using Facepunch.UI;
using Sandbox.Diagnostics;

namespace Facepunch;
partial class GameMode
{
	private enum DisplayedTimerMode
	{
		None,
		CountUp,
		CountDown,
		StateCountDown
	}

	public string DisplayedStatus => TeamStatusText.GetValueOrDefault( GameUtils.LocalPlayer?.Team ?? Team.Unassigned );

	public TimeSpan? DisplayedTime => TimerMode switch
	{
		DisplayedTimerMode.CountUp => TimeSpan.FromSeconds( Math.Clamp( Time.Now - TimerStart, 0f, TimerDuration ) ),
		DisplayedTimerMode.CountDown => TimeSpan.FromSeconds( Math.Clamp( TimerStart + TimerDuration - Time.Now + 1f, 0f, TimerDuration ) ),
		DisplayedTimerMode.StateCountDown => TimeSpan.FromSeconds( Math.Max( StateMachine.NextStateTime - Time.Now + 1f, 0f ) ),
		_ => null
	};

	[HostSync]
	private NetDictionary<Team, string> TeamStatusText { get; set; } = new();

	[HostSync] private DisplayedTimerMode TimerMode { get; set; }
	[HostSync] private float TimerStart { get; set; }
	[HostSync] private float TimerDuration { get; set; }

	public void ShowCountUpTimer( float startTime )
	{
		Assert.True( Networking.IsHost );

		TimerMode = DisplayedTimerMode.CountUp;
		TimerStart = startTime;
		TimerDuration = float.PositiveInfinity;
	}

	public void ShowStateCountDownTimer()
	{
		Assert.True( Networking.IsHost );

		TimerMode = DisplayedTimerMode.StateCountDown;
		TimerStart = 0f;
		TimerDuration = 0f;
	}

	public void ShowCountDownTimer( float startTime, float duration )
	{
		Assert.True( Networking.IsHost );

		TimerMode = DisplayedTimerMode.CountDown;
		TimerStart = startTime;
		TimerDuration = duration;
	}

	public void HideTimer()
	{
		Assert.True( Networking.IsHost );

		TimerMode = DisplayedTimerMode.None;
		TimerStart = 0f;
		TimerDuration = 0f;
	}

	public void ShowStatusText( string value )
	{
		Assert.True( Networking.IsHost );

		foreach ( var team in Enum.GetValues<Team>() )
		{
			ShowStatusText( team, value );
		}
	}

	public void ShowStatusText( Team team, string value )
	{
		Assert.True( Networking.IsHost );

		TeamStatusText[team] = value;
	}

	public void HideStatusText()
	{
		Assert.True( Networking.IsHost );

		TeamStatusText.Clear();
	}

	public void HideStatusText( Team team )
	{
		Assert.True( Networking.IsHost );

		TeamStatusText.Remove( team );
	}

	[Broadcast( NetPermission.HostOnly )]
	public void ShowToast( string value, ToastType type = ToastType.Generic, float duration = 5f )
	{
		Toast.Instance?.Show( value, type, duration );
	}

	[Broadcast( NetPermission.HostOnly )]
	public void HideToast()
	{
		Toast.Instance?.Show( "", default, 0f );
	}
}
