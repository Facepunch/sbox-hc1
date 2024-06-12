namespace Facepunch;

/// <summary>
/// A weapon component. This can be anything that controls a weapon. Aiming, recoil, sway, shooting..
/// </summary>
[Icon( "track_changes" )]
public abstract class WeaponComponent : Component
{
	/// <summary>
	/// The weapon.
	/// </summary>
	protected Weapon Weapon { get; set; }

	/// <summary>
	/// The player.
	/// </summary>
	protected PlayerController Player { get; set; }

	protected void BindTag( string tag, Func<bool> predicate ) => Weapon.BindTag( tag, predicate );

	protected override void OnAwake()
	{
		// Cache the weapon on awake
		Weapon = Components.Get<Weapon>( FindMode.EverythingInSelfAndAncestors );
		Player = Weapon.PlayerController;

		base.OnAwake();
	}
}
