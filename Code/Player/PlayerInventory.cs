namespace Gunfight;

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
	/// Temporary: A weapon set that we'll give the player by default.
	/// </summary>
	[Property] public List<WeaponDataResource> DefaultWeapons { get; set; }

	/// <summary>
	/// Gets the player's current weapon.
	/// </summary>
	public Weapon CurrentWeapon => Player.CurrentWeapon;

	public void Clear()
	{
		foreach ( var wpn in Weapons )
		{
			wpn.GameObject.Destroy();
		}

		Weapons.Clear();
	}

	public void Setup()
	{
		for ( int i = 0; i < DefaultWeapons.Count; i++ )
		{
			GiveWeapon( DefaultWeapons[i], i == 0 );
		}
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
		else
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

	public void GiveWeapon( WeaponDataResource resource, bool makeActive = true )
	{
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

	protected override void OnStart()
	{
		Setup();
	}
}
