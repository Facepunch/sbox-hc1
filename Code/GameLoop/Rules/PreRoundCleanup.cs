
using Sandbox.Events;

/// <summary>
/// Destroy any <see cref="GameObject"/>s marked with <see cref="DestroyBetweenRounds"/> before a new round starts.
/// </summary>
public sealed class PreRoundCleanup : Component,
	IGameEventHandler<PreRoundStartEvent>
{
	void IGameEventHandler<PreRoundStartEvent>.OnGameEvent( PreRoundStartEvent eventArgs )
	{
		foreach ( var component in Scene.GetAllComponents<DestroyBetweenRounds>() )
		{
			component.GameObject.Destroy();
		}
	}
}
