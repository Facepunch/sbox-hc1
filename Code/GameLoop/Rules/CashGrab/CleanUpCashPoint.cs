using Sandbox.Events;

namespace Facepunch;

public partial class CleanUpCashPoint : Component, IGameEventHandler<EnterStateEvent>
{
	[Before<DisableCashPoints>]
	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		var tracker = GameMode.Instance.Get<CashPointTracker>();
		tracker.Cleanup();
	}
}
