using Sandbox.Events;

namespace Facepunch;

public partial class ClientCleanupTime : Component, IGameEventHandler<EnterStateEvent>
{
	[Property] public float CleanupTime { get; set; } = 120f;

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		Client.DisconnectCleanupTime = CleanupTime;
	}
}
