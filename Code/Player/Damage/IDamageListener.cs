namespace Facepunch;

/// <summary>
/// A listener put on a component that listens to damage.
/// </summary>
public interface IDamageListener
{
	/// <summary>
	/// Called when YOU inflict damage on something
	/// </summary>
	public void OnDamageGiven( DamageEvent damageEvent );

	/// <summary>
	/// Called when YOU take damage from something
	/// </summary>
	public void OnDamageTaken( DamageEvent damageEvent );
}
