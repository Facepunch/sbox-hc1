using Facepunch;
using Facepunch.UI;

partial class GameMode
{
	private enum DisplayedTimerMode
	{
		None,
		CountUp,
		CountDown
	}

	public string DisplayedStatus { get; private set; }

	public TimeSpan? DisplayedTime => _timerMode switch
	{
		DisplayedTimerMode.CountUp => TimeSpan.FromSeconds( Math.Clamp( Time.Now - _timerStart, 0f, _timerDuration ) ),
		DisplayedTimerMode.CountDown => TimeSpan.FromSeconds( Math.Clamp( _timerStart + _timerDuration - Time.Now, 0f, _timerDuration ) ),
		_ => null
	};

	private DisplayedTimerMode _timerMode;
	private float _timerStart;
	private float _timerDuration;

	[Broadcast( NetPermission.HostOnly )]
	public void ShowCountUpTimer( float startTime )
	{
		_timerMode = DisplayedTimerMode.CountUp;
		_timerStart = startTime;
		_timerDuration = float.PositiveInfinity;
	}

	[Broadcast( NetPermission.HostOnly )]
	public void ShowCountDownTimer( float startTime, float duration )
	{
		_timerMode = DisplayedTimerMode.CountDown;
		_timerStart = startTime;
		_timerDuration = duration;
	}

	[Broadcast( NetPermission.HostOnly )]
	public void HideTimer()
	{
		_timerMode = DisplayedTimerMode.None;
		_timerStart = 0f;
		_timerDuration = 0f;
	}

	[Broadcast( NetPermission.HostOnly )]
	public void ShowStatusText( string value )
	{
		DisplayedStatus = value;
	}

	[Broadcast( NetPermission.HostOnly )]
	public void ShowStatusText( Team team, string value )
	{
		if ( (GameUtils.Viewer?.TeamComponent.Team ?? Team.Unassigned) != team )
		{
			return;
		}

		DisplayedStatus = value;
	}

	[Broadcast( NetPermission.HostOnly )]
	public void HideStatusText()
	{
		DisplayedStatus = null;
	}

	[Broadcast( NetPermission.HostOnly )]
	public void ShowToast( string value, ToastType type = ToastType.Generic )
	{
		Toast.Instance.Show( value, type );
	}
}
