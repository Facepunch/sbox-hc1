using Sandbox.Events;

namespace Facepunch;

public partial class SetCashPoint : Component, IGameEventHandler<EnterStateEvent>
{
	[Late]
	public void OnGameEvent( EnterStateEvent eventArgs )
	{
		if ( !Networking.IsHost )
			return;

		var tracker = GameMode.Instance.Get<CashPointTracker>();
		var cashPoints = tracker.All;

		var selected = Game.Random.FromList( cashPoints.ToList() );

		// Turn off all the cash points
		foreach ( var item in cashPoints )
		{
			if ( item == selected )
			{
				item.SetSpawning();
			}
			else
			{
				item.Deactivate();
			}
		}

		// Turn on our selected cash point
		{
			tracker.Last = tracker.Current;
			tracker.Current = selected;
		}
	}
}
