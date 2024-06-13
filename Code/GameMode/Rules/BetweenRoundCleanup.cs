using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Called on the host to clean up objects that shouldn't persist between rounds.
/// </summary>
public record BetweenRoundCleanupEvent : IGameEvent;

/// <summary>
/// Dispatches a <see cref="BetweenRoundCleanupEvent"/> when entering this state.
/// </summary>
public sealed class BetweenRoundCleanup : Component,
	IGameEventHandler<EnterStateEvent>
{
	[Early]
	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		Scene.Dispatch( new BetweenRoundCleanupEvent() );
	}
}
