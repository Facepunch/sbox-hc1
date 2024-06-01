namespace Facepunch;

/// <summary>
/// An ammo container. It holds ammo for a weapon.
/// </summary>
public partial class AmmoComponent : Component
{
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
