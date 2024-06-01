namespace Facepunch;

/// <summary>
/// A weapon function. This can be anything that controls a weapon. Aiming, recoil, sway, shooting..
/// </summary>
[Icon( "track_changes" )]
public abstract class WeaponFunction : Component
{
	/// <summary>
	/// The weapon. It's going to be a component on the same <see cref="GameObject"/>.
	/// </summary>
	protected Weapon Weapon { get; set; }

	protected void BindTag( string tag, Func<bool> predicate ) => Weapon.BindTag( tag, predicate );

	protected override void OnAwake()
	{
		Weapon = Components.Get<Weapon>( FindMode.EverythingInSelfAndAncestors );
		base.OnAwake();
	}
}
