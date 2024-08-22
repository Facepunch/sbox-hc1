using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Respawn players after a delay.
/// </summary>
public sealed class PlayerAutoRespawner : Respawner,
	IGameEventHandler<UpdateStateEvent>
{
}
