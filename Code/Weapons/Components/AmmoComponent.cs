namespace Facepunch;

/// <summary>
/// Hardcoded ammo types, no need making any kind of data struct for this.
/// </summary>
public enum AmmoType
{
	Pistol,
	Handcannon,
	SMG,
	Shotgun,
	AssaultRifle,

	/// <summary>
	/// Special ammo, grenade launchers, RPGs..
	/// </summary>
	Special
}

/// <summary>
/// An ammo container. It holds ammo for a weapon.
/// </summary>
[Title( "Ammo" ), Group( "Weapon Components" )]
public partial class AmmoComponent : Component, IDroppedWeaponState<AmmoComponent>
{
	[Property] 
	public AmmoType AmmoType { get; set; }

	/// <summary>
	/// How much ammo are we holding?
	/// </summary>
	[Property, Sync] public int Ammo { get; set; } = 0;

	[Property] public int MaxAmmo { get; set; } = 30;

	/// <summary>
	/// Do we have any ammo?
	/// </summary>
	[Property] public bool HasAmmo => Ammo > 0;

	/// <summary>
	/// Is this container full?
	/// </summary>
	public bool IsFull => Ammo == MaxAmmo;
}
