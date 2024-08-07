using Sandbox.Events;

namespace Facepunch;

public partial class ResetCapturePoints : Component,
	IGameEventHandler<EnterStateEvent>
{
	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		Scene.Dispatch( new ResetCapturePointsEvent() );
	}
}
