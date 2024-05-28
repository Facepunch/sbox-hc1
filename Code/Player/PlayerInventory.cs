namespace Facepunch;

/// <summary>
/// The player's inventory.
/// </summary>
public partial class PlayerInventory : Component
{
	[RequireComponent] PlayerController Player { get; set; }

	/// <summary>
	/// What weapons do we have right now?
	/// </summary>
	[Property] public List<Weapon> Weapons { get; private set; }

	/// <summary>
	/// A <see cref="GameObject"/> that will hold all of our weapons.
	/// </summary>
	[Property] public GameObject WeaponGameObject { get; set; }

	/// <summary>
	/// Can we unequip the current weapon so we have no weapons out?
	/// </summary>
	[Property] public bool CanUnequipCurrentWeapon { get; set; } = false;

	/// <summary>
	/// Gets the player's current weapon.
	/// </summary>
	public Weapon CurrentWeapon => Player.CurrentWeapon;

	[Authority( NetPermission.HostOnly )]
	public void Clear()
	{
		foreach ( var wpn in Weapons )
		{
			wpn.GameObject.Destroy();
		}

		Weapons.Clear();
	}

	protected override void OnUpdate()
	{
		if ( !Player.IsLocallyControlled )
		{
			return;
		}

		for ( int i = 0; i < Weapons.Count; i++ )
		{
			if ( Input.Pressed( $"Slot{i + 1}" ) )
			{
				SwitchToSlot( i );
			}
		}
	}

	public void HolsterCurrent()
	{
		Player.CurrentWeapon = null;
	}

	public void SwitchToSlot( int slot )
	{
		var weapon = Weapons[slot];
		if ( !weapon.IsValid() ) return;

		if ( Player.CurrentWeapon != weapon )
		{
			SwitchWeapon( weapon );
		}
		else if ( CanUnequipCurrentWeapon )
		{
			HolsterCurrent();
		}
	}

	/// <summary>
	/// Tries to set the player's current weapon to a specific one, which has to be in the player's inventory.
	/// </summary>
	/// <param name="weapon"></param>
	public void SwitchWeapon( Weapon weapon )
	{
		if ( !Weapons.Contains( weapon ) ) return;

		Player.CurrentWeapon = weapon;
	}

	/// <summary>
	/// Removes the given weapon and destroys it.
	/// </summary>
	public void RemoveWeapon( Weapon weapon )
	{
		if (!Weapons.Contains( weapon )) return;

		Weapons.Remove( weapon );

		if ( CurrentWeapon == weapon )
		{
			SwitchWeapon( Weapons.FirstOrDefault() );
		}

		weapon.GameObject.Destroy();
	}

	[Authority( NetPermission.HostOnly )]
	private void GiveWeapon( int resourceId, bool makeActive )
	{
		var resource = ResourceLibrary.Get<WeaponDataResource>( resourceId )
			?? throw new Exception( $"Unable to find {nameof(WeaponDataResource)} with id {resourceId}." );

		GiveWeapon( resource, makeActive );
	}

	public void GiveWeapon( WeaponDataResource resource, bool makeActive = true )
	{
		if ( IsProxy )
		{
			GiveWeapon( resource.ResourceId, makeActive );
			return;
		}

		// If we're in charge, let's make some weapons.
		if ( resource == null )
		{
			Log.Warning( "A player loadout without a weapon? Nonsense." );
			return;
		}

		if ( !resource.MainPrefab.IsValid() )
		{
			Log.Error( "Weapon doesn't have a prefab?" );
			return;
		}

		// Create the weapon prefab and put it on the weapon gameobject.
		var weaponGameObject = resource.MainPrefab.Clone( new CloneConfig()
		{
			Transform = new Transform(),
			Parent = WeaponGameObject,
			StartEnabled = false,
		} );
		var weaponComponent = weaponGameObject.Components.Get<Weapon>( FindMode.EverythingInSelfAndDescendants );
		weaponGameObject.NetworkSpawn();

		weaponGameObject.Enabled = false;

		Weapons.Add( weaponComponent );

		if ( makeActive ) Player.CurrentWeapon = weaponComponent;
	}

	public bool OwnsWeapon(WeaponDataResource resource) => OwnsWeapon(resource.ResourceId);

	public bool OwnsWeapon(int resourceId)
	{
		foreach ( Weapon weapon in Weapons )
		{
			if (weapon.Resource.ResourceId == resourceId)
				return true;
		}

		return false;
	}
}
