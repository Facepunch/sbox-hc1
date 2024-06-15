using Sandbox.Events;

namespace Facepunch;

public partial class ActivateCashPoint : Component, IGameEventHandler<EnterStateEvent>
{
	public void OnGameEvent( EnterStateEvent eventArgs )
	{
		var cashPoint = GameMode.Instance.Get<CashPointTracker>().Current;
		cashPoint.Activate();
	}
}
