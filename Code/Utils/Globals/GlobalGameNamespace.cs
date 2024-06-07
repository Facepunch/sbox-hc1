namespace Facepunch;

/// <summary>
/// It's nice to be able to access a global anywhere.
/// </summary>
public static class GlobalGameNamespace
{
	/// <summary>
	/// Fetch a global.
	/// </summary>
	public static T GetGlobal<T>() where T : GlobalComponent
	{
		return Game.ActiveScene.GetSystem<GlobalSystem>().Get<T>();
	}
}
