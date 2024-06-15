using Sandbox.Events;

namespace Facepunch;

public partial class SetCashPoint : Component, IGameEventHandler<EnterStateEvent>
{
	public void OnGameEvent( EnterStateEvent eventArgs )
	{
		var tracker = GameMode.Instance.Get<CashPointTracker>();
		var cashPoints = CashPoint.All;

		var selected = Game.Random.FromList<CashPoint>( cashPoints.Where( x => x != tracker.Last ).ToList() );


		// Turn off all the cash points
		foreach ( var item in cashPoints )
		{
			item.GameObject.Enabled = false;
		}

		// Turn on our selected cash point
		{
			tracker.Last = tracker.Current;
			tracker.Current = selected;
			selected.GameObject.Enabled = true;
		}
	}
}
