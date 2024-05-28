using Sandbox.Diagnostics;

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
	public IEnumerable<Weapon> Weapons => WeaponGameObject.Components.GetAll<Weapon>( FindMode.EverythingInSelfAndDescendants );

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
	
	public void Clear()
	{
		Assert.True( Networking.IsHost );
		
		foreach ( var wpn in Weapons )
		{
			wpn.GameObject.Destroy();
			wpn.Enabled = false;
		}
	}

	protected override void OnUpdate()
	{
		if ( !Player.IsLocallyControlled )
			return;

		for ( int i = 0; i < Weapons.Count(); i++ )
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
		Assert.False( IsProxy );
		
		var weapon = Weapons.ElementAt( slot );
		if ( !weapon.IsValid() ) return;

		if ( Player.CurrentWeapon != weapon )
			SwitchWeapon( weapon );
		else if ( CanUnequipCurrentWeapon )
			HolsterCurrent();
	}

	/// <summary>
	/// Tries to set the player's current weapon to a specific one, which has to be in the player's inventory.
	/// </summary>
	/// <param name="weapon"></param>
	public void SwitchWeapon( Weapon weapon )
	{
		if ( !Weapons.Contains( weapon ) )
			return;

		if ( Networking.IsHost )
		{
			Player.ChangeCurrentWeapon( weapon.Id );
			return;
		}
		
		Player.CurrentWeapon = weapon;
	}

	/// <summary>
	/// Removes the given weapon and destroys it.
	/// </summary>
	public void RemoveWeapon( Weapon weapon )
	{
		Assert.True( Networking.IsHost );
		
		if ( !Weapons.Contains( weapon ) ) return;

		if ( CurrentWeapon == weapon )
			SwitchWeapon( Weapons.FirstOrDefault( x => x != weapon ) );

		weapon.GameObject.Destroy();
	}

	public void GiveWeapon( WeaponDataResource resource, bool makeActive = true )
	{
		Assert.True( Networking.IsHost );
		
		// Can't have the same weapon twice
		if ( HasWeapon( resource ) ) return;

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
			Transform = new(),
			Parent = WeaponGameObject,
			StartEnabled = false,
		} );
		var weaponComponent = weaponGameObject.Components.Get<Weapon>( FindMode.EverythingInSelfAndDescendants );
		weaponGameObject.NetworkSpawn( Player.Network.OwnerConnection );
		weaponGameObject.Enabled = false;

		if ( makeActive )
		{
			Player.ChangeCurrentWeapon( weaponComponent.Id );
		}

		Log.Info( $"Spawned weapon {weaponGameObject} for {Player}" );
	}

	public bool HasWeapon( WeaponDataResource resource )
	{
		return Weapons.Any( weapon => weapon.Enabled && weapon.Resource == resource );
	}
}
