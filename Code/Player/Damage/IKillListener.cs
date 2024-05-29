namespace Facepunch;

/// <summary>
/// A listener put on a component that listens to damage.
/// </summary>
public interface IKillListener
{
	/// <summary>
	/// Called when something dies
	/// </summary>
	/// <param name="damage"></param>
	/// <param name="position"></param>
	/// <param name="force"></param>
	/// <param name="killer"></param>
	/// <param name="victim"></param>
	public void OnPlayerKilled( Component killer, Component victim, float damage, Vector3 position, Vector3 force );
}
