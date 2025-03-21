using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.Events;
using System.Diagnostics.Metrics;

namespace Facepunch;

/// <summary>
/// The player's inventory.
/// </summary>
public partial class PlayerInventory : Component
{
	[RequireComponent] PlayerPawn Player { get; set; }

	/// <summary>
	/// What equipment do we have right now?
	/// </summary>
	public IEnumerable<Equipment> Equipment => Player.GetComponentsInChildren<Equipment>();

	/// <summary>
	/// Does this player have a bomb on them?
	/// </summary>
	public bool HasBomb => Equipment.Any( x => x.GetComponentInChildren<BombPlantComponent>().IsValid() );

	/// <summary>
	/// A <see cref="GameObject"/> that will hold all of our equipment.
	/// </summary>
	[Property] public GameObject WeaponGameObject { get; set; }

	/// <summary>
	/// Can we unequip the current weapon so we have no equipment out?
	/// </summary>
	[Property] public bool CanUnequipCurrentWeapon { get; set; } = false;

	/// <summary>
	/// Does this player have a defuse kit?
	/// </summary>
	public bool HasDefuseKit
	{
		get => Player.Client.Loadout.HasDefuseKit;
		set => Player.Client.Loadout.HasDefuseKit = value;
	}

	/// <summary>
	/// Gets the player's current weapon.
	/// </summary>
	private Equipment Current => Player.CurrentEquipment;
	
	public void Clear()
	{
		if ( !Networking.IsHost )
			return;
		
		foreach ( var wpn in Equipment )
		{
			wpn.GameObject.Destroy();
			wpn.Enabled = false;
		}
	}

	[Rpc.Owner( NetFlags.HostOnly )]
	public void RefillAmmo()
	{
		foreach ( var wpn in Equipment )
		{
			if ( wpn.GetComponentInChildren<AmmoComponent>() is { } ammo )
			{
				ammo.Ammo = ammo.MaxAmmo;
			}
		}
	}

	/// <summary>
	/// Try to drop the given held equipment item.
	/// </summary>
	/// <param name="weapon">Item to drop.</param>
	/// <param name="forceRemove">If we can't drop, remove it from the inventory anyway.</param>
	public void Drop( Equipment weapon, bool forceRemove = false )
	{
		using ( Rpc.FilterInclude( Connection.Host ) )
		{
			DropHost( weapon, forceRemove );
		}
	}

	[Rpc.Broadcast]
	private void DropHost( Equipment weapon, bool forceRemove )
	{
		if ( !Networking.IsHost )
			return;

		if ( !weapon.IsValid() )
			return;

		var canDrop = GameMode.Instance.Get<EquipmentDropper>() is { } dropper && dropper.CanDrop( Player, weapon );

		if ( canDrop )
		{
			var tr = Scene.Trace.Ray( new Ray( Player.AimRay.Position, Player.AimRay.Forward ), 128 )
				.IgnoreGameObjectHierarchy( GameObject.Root )
				.WithoutTags( "trigger" )
				.Run();

			var position = tr.Hit ? tr.HitPosition + tr.Normal * weapon.Resource.WorldModel.Bounds.Size.Length : Player.AimRay.Position + Player.AimRay.Forward * 32f;
			var rotation = Rotation.From( 0, Player.EyeAngles.yaw + 90, 90 );

			var baseVelocity = Player.CharacterController.Velocity;
			var droppedWeapon = DroppedEquipment.Create( weapon.Resource, position, rotation, weapon );

			if ( !tr.Hit )
			{
				droppedWeapon.Rigidbody.Velocity = baseVelocity + Player.AimRay.Forward * 200.0f + Vector3.Up * 50;
				droppedWeapon.Rigidbody.AngularVelocity = Vector3.Random * 8.0f;
			}
		}

		if ( canDrop || forceRemove )
		{
			RemoveWeapon( weapon );
		}
	}

	protected override void OnUpdate()
	{
		if ( !Player.IsLocallyControlled )
			return;

		if ( Input.Pressed( "Drop" ) && Current.IsValid() )
		{
			Drop( Current );
			return;
		}

		foreach ( var slot in Enum.GetValues<EquipmentSlot>() )
		{
			if ( slot == EquipmentSlot.Undefined )
				continue;

			if ( !Input.Pressed( $"Slot{(int)slot}" ) )
				continue;

			SwitchToSlot( slot );
			return;
		}

		var wheel = Input.MouseWheel;

		// gamepad input
		if ( Input.Pressed( "NextSlot" ) ) wheel.y = -1;
		if ( Input.Pressed( "PrevSlot" ) ) wheel.y = 1;

		if ( wheel.y == 0f ) return;

		var availableWeapons = Equipment.OrderBy( x => x.Resource.Slot ).ToList();
		if ( availableWeapons.Count == 0 )
			return;

		var currentSlot = 0;
		for ( var index = 0; index < availableWeapons.Count; index++ )
		{
			var weapon = availableWeapons[index];
			if ( !weapon.IsDeployed )
				continue;

			currentSlot = index;
			break;
		}

		var slotDelta = wheel.y > 0f ? 1 : -1;
		currentSlot += slotDelta;

		if ( currentSlot < 0 )
			currentSlot = availableWeapons.Count - 1;
		else if ( currentSlot >= availableWeapons.Count )
			currentSlot = 0;

		var weaponToSwitchTo = availableWeapons[currentSlot];
		if ( weaponToSwitchTo == Current )
			return;

		Switch( weaponToSwitchTo );
	}

	public void SwitchToBest()
	{
		if ( !Equipment.Any() )
			return;

		if ( HasInSlot( EquipmentSlot.Primary ) )
		{
			SwitchToSlot( EquipmentSlot.Primary );
			return;
		}

		if ( HasInSlot( EquipmentSlot.Secondary ) )
		{
			SwitchToSlot( EquipmentSlot.Secondary );
			return;
		}

		if ( HasInSlot( EquipmentSlot.Melee ) )
		{
			SwitchToSlot( EquipmentSlot.Melee );
			return;
		}

		Switch( Equipment.FirstOrDefault() );
	}

	public void HolsterCurrent()
	{
		Assert.True( !IsProxy || Networking.IsHost );
		Player.SetCurrentEquipment( null );
	}

	public void SwitchToSlot( EquipmentSlot slot )
	{
		Assert.True( !IsProxy || Networking.IsHost );

		var equipment = Equipment
			.Where( x => x.Resource.Slot == slot )
			.ToArray();

		if ( equipment.Length == 0 )
			return;

		if ( equipment.Length == 1 && Current == equipment[0] && CanUnequipCurrentWeapon )
		{
			HolsterCurrent();
			return;
		}

		var index = Array.IndexOf( equipment, Current );
		Switch( equipment[(index + 1) % equipment.Length] );
	}

	/// <summary>
	/// Tries to set the player's current weapon to a specific one, which has to be in the player's inventory.
	/// </summary>
	/// <param name="equipment"></param>
	public void Switch( Equipment equipment )
	{
		Assert.True( !IsProxy || Networking.IsHost );
		
		if ( !Equipment.Contains( equipment ) )
			return;
		
		Player.SetCurrentEquipment( equipment );
	}

	/// <summary>
	/// Removes the given weapon and destroys it.
	/// </summary>
	public void RemoveWeapon( Equipment equipment )
	{
		Assert.True( Networking.IsHost );
		
		if ( !Equipment.Contains( equipment ) ) return;

		if ( Current == equipment )
		{
			var otherEquipment = Equipment.Where( x => x != equipment );
			var orderedBySlot = otherEquipment.OrderBy( x => x.Resource.Slot );
			var targetWeapon = orderedBySlot.FirstOrDefault();

			if ( targetWeapon.IsValid() )
			{
				Switch( targetWeapon );
			}
		}

		equipment.GameObject.Destroy();
		equipment.Enabled = false;
	}
	
	/// <summary>
	/// Removes the given weapon (by its resource data) and destroys it.
	/// </summary>
	public void Remove( EquipmentResource resource )
	{
		var equipment = Equipment.FirstOrDefault( w => w.Resource == resource );
		if ( !equipment.IsValid() ) return;
		RemoveWeapon( equipment );
	}

	public Equipment Give( EquipmentResource resource, bool makeActive = true )
	{
		Assert.True( Networking.IsHost );

		// If we're in charge, let's make some equipment.
		if ( resource == null )
		{
			return null;
		}

		var pickupResult = CanTake( resource );

		if ( pickupResult == PickupResult.None )
			return null;

		// Don't let us have the exact same equipment
		if ( Has( resource ) )
			return null;

		if ( pickupResult == PickupResult.Swap )
		{
			var slotCurrent = Equipment.FirstOrDefault( equipment => equipment.Enabled && equipment.Resource.Slot == resource.Slot );
			if ( slotCurrent.IsValid() )
				Drop( slotCurrent, true );
		}

		if ( !resource.MainPrefab.IsValid() )
		{
			Log.Error( $"equipment doesn't have a prefab? {resource}, {resource.MainPrefab}, {resource.ViewModelPrefab}" );
			return null;
		}

		// Create the equipment prefab and put it on the GameObject.
		var gameObject = resource.MainPrefab.Clone( new CloneConfig()
		{
			Transform = new(),
			Parent = WeaponGameObject
		} );
		var component = gameObject.GetComponentInChildren<Equipment>( true );
		component.Owner = Player;
		gameObject.NetworkSpawn( Player.Network.Owner );

		if ( makeActive )
			Player.SetCurrentEquipment( component );

		if ( component.Resource.Slot == EquipmentSlot.Special )
		{
			Scene.Dispatch( new BombPickedUpEvent() );
		}

		return component;
	}

	public bool Has( EquipmentResource resource )
	{
		return Equipment.Any( weapon => weapon.Enabled && weapon.Resource == resource );
	}

	public bool HasInSlot( EquipmentSlot slot )
	{
		return Equipment.Any( weapon => weapon.Enabled && weapon.Resource.Slot == slot );
	}

	public enum PickupResult
	{
		None,
		Pickup,
		Swap
	}

	public PickupResult CanTake( EquipmentResource resource )
	{
		if ( resource.Team != Team.Unassigned
			&& resource.Team != Player.Team
			&& !resource.CanOtherTeamPickUp )
		{
			return PickupResult.None;
		}

		switch ( resource.Slot )
		{
			case EquipmentSlot.Utility:
				var can = !Has( resource );
				return can ? PickupResult.Pickup : PickupResult.Swap;

			default:
				return !HasInSlot( resource.Slot ) ? PickupResult.Pickup : PickupResult.Swap;
		}
	}

	public void TryPurchaseBuyMenuItem( string equipmentId )
	{
		using var _ = Rpc.FilterInclude( Connection.Host );
		PurchaseBuyMenuItem( equipmentId );
	}

	[Rpc.Broadcast]
	private void PurchaseBuyMenuItem( string equipmentId )
	{
		if ( !Networking.IsHost )
		{
			Log.Warning( $"Tried to purchase an buy menu item ({equipmentId}) but is not the host." );
			return;
		}

		var equipmentData = BuyMenuItem.GetById( equipmentId );

		if ( equipmentData == null )
		{
			Log.Warning( $"Attempted purchase but EquipmentData (Id: {equipmentId}) not known!" );
			return;
		}
		
		equipmentData.Purchase( Player );
	}

	public void Purchase( int resourceId )
	{
		using var _ = Rpc.FilterInclude( Connection.Host );
		PurchaseAsHost( resourceId );
	}

	[Rpc.Broadcast]
	private void PurchaseAsHost( int resourceId )
	{
		if ( !Networking.IsHost )
		{
			Log.Warning( $"Tried to purchase an inventory resource ({resourceId}) but is not the host." );
			return;
		}

		var resource = ResourceLibrary.Get<EquipmentResource>( resourceId );

		if ( resource == null )
		{
			Log.Warning( $"Attempted purchase but EquipmentResource (Id: {resource}) not known!" );
			return;
		}

		if ( Player.Client.Balance < resource.Price )
			return;

		if ( Give( resource ) is null )
			return;

		// Update the player's loadout
		Player.Client.Loadout.SetFrom( Player );

		Player.Client.Balance -= resource.Price;
	}
}
