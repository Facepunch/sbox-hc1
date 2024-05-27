namespace Gunfight;

/// <summary>
/// A weapon function. This can be anything that controls a weapon. Aiming, recoil, sway, shooting..
/// </summary>
[Icon( "track_changes" )]
public abstract class WeaponFunction : Component
{
	/// <summary>
	/// Find the weapon, it's going to be a component on the same GameObject.
	/// </summary>
	public Weapon Weapon => Components.Get<Weapon>( FindMode.EverythingInSelfAndAncestors );

	protected void BindTag( string tag, Func<bool> predicate ) => Weapon.BindTag( tag, predicate );
}
