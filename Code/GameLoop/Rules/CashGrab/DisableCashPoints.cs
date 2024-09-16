using Sandbox.Events;

namespace Facepunch;

public partial class DisableCashPoints : Component, IGameEventHandler<EnterStateEvent>
{
	[Late]
	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		if ( !Networking.IsHost )
			return;

		var tracker = GameMode.Instance.Get<CashPointTracker>();

		// Turn off the cash points
		foreach ( var cashPoint in tracker.All )
		{
			cashPoint.Deactivate();
		}
	}
}
