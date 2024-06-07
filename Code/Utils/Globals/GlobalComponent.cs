namespace Facepunch;

/// <summary>
/// A component that registers itself as a global.
/// </summary>
public abstract class GlobalComponent : Component
{
	protected override void OnStart()
	{
		Scene.GetSystem<GlobalSystem>().Register( this );
	}
}
