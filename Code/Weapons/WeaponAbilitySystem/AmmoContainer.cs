namespace Facepunch;

/// <summary>
/// An ammo container. It holds ammo for a weapon.
/// </summary>
public partial class AmmoContainer : Component
{
	private int ammo = 0;

	/// <summary>
	/// How much ammo are we holding?
	/// </summary>
	[Property] public int Ammo
	{
		get => ammo;
		set
		{
			ammo = value;
		}
	}

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
