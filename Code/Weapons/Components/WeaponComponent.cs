namespace Facepunch;

/// <summary>
/// A weapon component. This can be anything that controls a weapon. Aiming, recoil, sway, shooting..
/// </summary>
[Icon( "track_changes" )]
public abstract class WeaponComponent : Component
{
	/// <summary>
	/// The weapon. It's going to be a component on the same <see cref="GameObject"/>.
	/// </summary>
	protected Weapon Weapon { get; set; }

	protected void BindTag( string tag, Func<bool> predicate ) => Weapon.BindTag( tag, predicate );

	protected override void OnAwake()
	{
		// Cache the weapon on awake
		Weapon = Components.Get<Weapon>( FindMode.EverythingInSelfAndAncestors );

		base.OnAwake();
	}
}
