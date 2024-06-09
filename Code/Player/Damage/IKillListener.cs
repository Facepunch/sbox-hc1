namespace Facepunch;

/// <summary>
/// A listener put on a component that listens to damage.
/// </summary>
public interface IKillListener
{
	/// <summary>
	/// Called when something dies
	/// </summary>
	/// <param name="damageEvent"></param>
	public void OnPlayerKilled( DamageEvent damageEvent );
}
