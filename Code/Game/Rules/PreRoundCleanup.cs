
/// <summary>
/// Destroy any <see cref="GameObject"/>s marked with <see cref="DestroyBetweenRounds"/> before a new round starts.
/// </summary>
public sealed class PreRoundCleanup : Component, IRoundStartListener
{
	void IRoundStartListener.PreRoundStart()
	{
		foreach ( var component in Scene.GetAllComponents<DestroyBetweenRounds>() )
		{
			component.GameObject.Destroy();
		}
	}
}
