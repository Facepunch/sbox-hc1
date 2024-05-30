namespace Facepunch;

/// <summary>
/// A listener put on a component that listens to damage.
/// </summary>
public interface IDamageListener
{
	/// <summary>
	/// Called when YOU inflict damage on something
	/// </summary>
	/// <param name="damage"></param>
	/// <param name="position"></param>
	/// <param name="force"></param>
	/// <param name="target"></param>
	/// <param name="isHeadshot"></param>
	public void OnDamageGiven( float damage, Vector3 position, Vector3 force, Component target, bool isHeadshot );

	/// <summary>
	/// Called when YOU take damage from something
	/// </summary>
	/// <param name="damage"></param>
	/// <param name="position"></param>
	/// <param name="force"></param>
	/// <param name="attacker"></param>
	/// <param name="isHeadshot"></param>
	public void OnDamageTaken( float damage, Vector3 position, Vector3 force, Component attacker, bool isHeadshot );
}
