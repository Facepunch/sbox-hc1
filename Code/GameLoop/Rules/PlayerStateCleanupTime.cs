using Sandbox.Events;

namespace Facepunch;

public partial class PlayerStateCleanupTime : Component, IGameEventHandler<EnterStateEvent>
{
	[Property] public float CleanupTime { get; set; } = 120f;

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		PlayerState.DisconnectCleanupTime = CleanupTime;
	}
}
