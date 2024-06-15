using Sandbox.Events;

namespace Facepunch;

public partial class ActivateCashPoint : Component, IGameEventHandler<EnterStateEvent>
{
	public void OnGameEvent( EnterStateEvent eventArgs )
	{
		if ( !Networking.IsHost )
			return;

		var cashPoint = GameMode.Instance.Get<CashPointTracker>().Current;
		cashPoint.Activate();
	}
}
