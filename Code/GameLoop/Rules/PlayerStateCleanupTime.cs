using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Lets a gamemode define how long we will leave a client in a session before removing it.
/// </summary>
public partial class ClientCleanupTime : Component, IGameEventHandler<EnterStateEvent>
{
	[Property] public float CleanupTime { get; set; } = 120f;

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		Client.DisconnectCleanupTime = CleanupTime;
	}
}
