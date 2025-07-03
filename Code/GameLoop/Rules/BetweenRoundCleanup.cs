using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Called on the host to clean up objects that shouldn't persist between rounds.
/// </summary>
public interface IRoundCleanup : ISceneEvent<IRoundCleanup>
{
	/// <summary>
	/// Called when the round cleanup is dispatched.
	/// </summary>
	void OnRoundCleanup();
}

/// <summary>
/// Dispatches a <see cref="IRoundCleanup"/> when entering this state.
/// </summary>
public sealed class BetweenRoundCleanup : Component,
	IGameEventHandler<EnterStateEvent>
{
	[Early]
	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		Dispatch();
	}

	[Rpc.Broadcast( NetFlags.HostOnly )]
	public void Dispatch()
	{
		Scene.RunEvent<IRoundCleanup>( x => x.OnRoundCleanup() );
	}
}
