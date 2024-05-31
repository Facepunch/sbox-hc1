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
	/// Does this player have a bomb on them?
	/// </summary>
	public bool HasBomb => Weapons.Any( x => x.GetFunction<PlantFunction>() != null );

	/// <summary>
	/// A <see cref="GameObject"/> that will hold all of our weapons.
	/// </summary>
	[Property] public GameObject WeaponGameObject { get; set; }

	/// <summary>
	/// Can we unequip the current weapon so we have no weapons out?
	/// </summary>
	[Property] public bool CanUnequipCurrentWeapon { get; set; } = false;

	/// <summary>
	/// Players current cash balance
	/// </summary>
	[HostSync] public int Balance { get; private set; } = 999999;

	/// <summary>
	/// Does this player have a defuse kit?
	/// </summary>
	[HostSync]
	public bool HasDefuseKit { get; set; }

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

	[Broadcast]
	public void DropWeapon( Guid weaponId )
	{
		var weapon = Scene.Directory.FindComponentByGuid( weaponId ) as Weapon;
		if ( !weapon.IsValid() )
			return;

		if ( weapon.Resource.Slot == WeaponSlot.Melee )
			return;

		if ( !Networking.IsHost )
			return;

		var tr = Scene.Trace.Ray( new Ray( Player.AimRay.Position, Player.AimRay.Forward ), 128 )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "trigger" )
			.Run();

		var position = tr.Hit ? tr.HitPosition + tr.Normal * weapon.Resource.WorldModel.Bounds.Size.Length : Player.AimRay.Position + Player.AimRay.Forward * 32f;
		var rotation = Rotation.From( 0, Player.EyeAngles.yaw + 90, 90 );

		var baseVelocity = Player.CharacterController.Velocity;
		var droppedWeapon = DroppedWeapon.Create( weapon.Resource, position, rotation );

		if ( !tr.Hit )
		{
			droppedWeapon.Rigidbody.Velocity = baseVelocity + Player.AimRay.Forward * 200.0f + Vector3.Up * 50;
			droppedWeapon.Rigidbody.AngularVelocity = Vector3.Random * 8.0f;
		}

		droppedWeapon.GameObject.NetworkSpawn();

		RemoveWeapon( weapon );
	}

	protected override void OnUpdate()
	{
		if ( !Player.IsLocallyControlled )
			return;

		if ( Input.Pressed( "Drop" ) )
		{
			DropWeapon( CurrentWeapon.Id );
		}

		foreach ( var slot in Enum.GetValues<WeaponSlot>() )
		{
			if ( slot == WeaponSlot.Undefined )
				continue;

			if ( Input.Pressed( $"Slot{(int)slot}" ) )
			{
				SwitchToSlot( slot );
			}
		}
	}

	public void SwitchToBestWeapon()
	{
		if ( !Weapons.Any() )
		{
			return;
		}

		if ( HasWeapon( WeaponSlot.Primary ) )
		{
			SwitchToSlot( WeaponSlot.Primary );
			return;
		}

		if ( HasWeapon( WeaponSlot.Secondary ) )
		{
			SwitchToSlot( WeaponSlot.Secondary );
			return;
		}

		if ( HasWeapon( WeaponSlot.Melee ) )
		{
			SwitchToSlot( WeaponSlot.Melee );
			return;
		}

		SwitchWeapon( Weapons.FirstOrDefault() );
	}

	public void HolsterCurrent()
	{
		Assert.True( !IsProxy || Networking.IsHost );
		Player.SetCurrentWeapon( null );
	}

	public void SwitchToSlot( WeaponSlot slot )
	{
		Assert.True( !IsProxy || Networking.IsHost );

		var weapons = Weapons
			.Where( x => x.Resource.Slot == slot )
			.ToArray();

		if ( weapons.Length == 0 )
		{
			return;
		}

		if ( weapons.Length == 1 && CurrentWeapon == weapons[0] && CanUnequipCurrentWeapon )
		{
			HolsterCurrent();
			return;
		}

		var index = Array.IndexOf( weapons, CurrentWeapon );

		SwitchWeapon( weapons[(index + 1) % weapons.Length] );
	}

	/// <summary>
	/// Tries to set the player's current weapon to a specific one, which has to be in the player's inventory.
	/// </summary>
	/// <param name="weapon"></param>
	public void SwitchWeapon( Weapon weapon )
	{
		Assert.True( !IsProxy || Networking.IsHost );
		
		if ( !Weapons.Contains( weapon ) )
			return;
		
		Player.SetCurrentWeapon( weapon );
	}

	/// <summary>
	/// Removes the given weapon and destroys it.
	/// </summary>
	public void RemoveWeapon( Weapon weapon )
	{
		Assert.True( Networking.IsHost );
		
		if ( !Weapons.Contains( weapon ) ) return;

		if ( CurrentWeapon == weapon )
		{
			var otherWeapons = Weapons.Where( x => x != weapon );
			var orderedBySlot = otherWeapons.OrderBy( x => x.Resource.Slot );
			var targetWeapon = orderedBySlot.FirstOrDefault();

			if ( targetWeapon.IsValid() )
				SwitchWeapon( targetWeapon );
		}

		weapon.GameObject.Destroy();
		weapon.Enabled = false;
	}
	
	/// <summary>
	/// Removes the given weapon (by its resource data) and destroys it.
	/// </summary>
	public void RemoveWeapon( WeaponData resource )
	{
		var weapon = Weapons.FirstOrDefault( w => w.Resource == resource );
		if ( !weapon.IsValid() ) return;
		RemoveWeapon( weapon );
	}

	public Weapon GiveWeapon( WeaponData resource, bool makeActive = true )
	{
		Assert.True( Networking.IsHost );

		// If we're in charge, let's make some weapons.
		if ( resource == null )
		{
			Log.Warning( "A player loadout without a weapon? Nonsense." );
			return null;
		}

		if ( !CanTakeWeapon( resource ) )
			return null;

		// Don't let us have the exact same weapon
		if ( HasWeapon( resource ) )
			return null;

		var slotCurrent = Weapons.FirstOrDefault( weapon => weapon.Enabled && weapon.Resource.Slot == resource.Slot );
		if ( slotCurrent.IsValid() )
			DropWeapon( slotCurrent.Id );

		if ( !resource.MainPrefab.IsValid() )
		{
			Log.Error( $"Weapon doesn't have a prefab? {resource}, {resource.MainPrefab}, {resource.ViewModelPrefab}" );
			return null;
		}

		// Create the weapon prefab and put it on the weapon GameObject.
		var weaponGameObject = resource.MainPrefab.Clone( new CloneConfig()
		{
			Transform = new(),
			Parent = WeaponGameObject
		} );
		var weaponComponent = weaponGameObject.Components.Get<Weapon>( FindMode.EverythingInSelfAndDescendants );
		weaponGameObject.NetworkSpawn( Player.Network.OwnerConnection );

		if ( makeActive )
			Player.SetCurrentWeapon( weaponComponent );
		
		Log.Info( $"Spawned weapon {weaponGameObject} for {Player}" );

		return weaponComponent;
	}

	public bool HasWeapon( WeaponData resource )
	{
		return Weapons.Any( weapon => weapon.Enabled && weapon.Resource == resource );
	}

	public bool HasWeapon( WeaponSlot slot )
	{
		return Weapons.Any( weapon => weapon.Enabled && weapon.Resource.Slot == slot );
	}

	public bool CanTakeWeapon( WeaponData resource )
	{
		if ( resource.Team != Team.Unassigned
			&& resource.Team != Player.TeamComponent.Team
			&& !resource.CanOtherTeamPickUp )
		{
			return false;
		}

		switch ( resource.Slot )
		{
			case WeaponSlot.Utility:
				// TODO: grenade limits
				return true;

			default:
				return true;
		}
	}

	public void SetCash( int amount )
	{
		using var _ = Rpc.FilterInclude( Connection.Host );
		SetCashHost( amount );
	}

	[Broadcast]
	private void SetCashHost( int amount )
	{
		Assert.True( Networking.IsHost );
		Balance = Math.Clamp( amount, 0, GameMode.Instance.MaxBalance );
	}

	public void GiveCash( int amount )
	{
		using var _ = Rpc.FilterInclude( Connection.Host );
		GiveCashHost( amount );
	}

	[Broadcast]
	private void GiveCashHost( int amount )
	{
		Assert.True( Networking.IsHost );
		Balance = Math.Clamp( Balance + amount, 0, GameMode.Instance.MaxBalance );
	}

	public void BuyWeapon( int resourceId )
	{
		using var _ = Rpc.FilterInclude( Connection.Host );
		BuyWeaponHost( resourceId );
	}

	[Broadcast]
	private void BuyWeaponHost( int resourceId )
	{
		Assert.True( Networking.IsHost );

		var weaponData = ResourceLibrary.Get<WeaponData>(resourceId);

		if ( weaponData == null )
		{
			Log.Warning( $"Attempted purchase but WeaponData (Id: {weaponData}) not known!" );
			return;
		}

		if ( Balance < weaponData.Price )
			return;

		if ( GiveWeapon( weaponData ) is null )
			return;

		Balance -= weaponData.Price;
	}

	public void BuyEquipment( string equipmentId )
	{
		using var _ = Rpc.FilterInclude( Connection.Host );
		BuyEquipmentHost( equipmentId );
	}

	[Broadcast]
	private void BuyEquipmentHost( string equipmentId )
	{
		Assert.True( Networking.IsHost );

		var equipmentData = EquipmentData.GetById( equipmentId );

		if ( equipmentData == null )
		{
			Log.Warning( $"Attempted purchase but EquipmentData (Id: {equipmentId}) not known!" );
			return;
		}
		
		equipmentData.Purchase( Player );
	}
}
