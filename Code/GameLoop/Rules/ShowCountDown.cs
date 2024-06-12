using Sandbox.Events;

namespace Facepunch.GameRules;

/// <summary>
/// Shows status text and a countdown based on the duration of the current state.
/// </summary>
public sealed class ShowCountDown : Component,
	IGameEventHandler<EnterStateEventArgs>,
	IGameEventHandler<LeaveStateEventArgs>
{
	[Property]
	public string StatusText { get; set; }

	void IGameEventHandler<EnterStateEventArgs>.OnGameEvent( EnterStateEventArgs eventArgs )
	{
		if ( !string.IsNullOrEmpty( StatusText ) )
		{
			GameMode.Instance.ShowStatusText( StatusText );
		}

		GameMode.Instance.ShowCountDownTimer( Time.Now, eventArgs.State.DefaultDuration );
	}

	void IGameEventHandler<LeaveStateEventArgs>.OnGameEvent( LeaveStateEventArgs eventArgs )
	{
		if ( !string.IsNullOrEmpty( StatusText ) )
		{
			GameMode.Instance.HideStatusText();
		}

		GameMode.Instance.HideTimer();
	}
}
