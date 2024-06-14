using Sandbox.Events;

namespace Facepunch;

public partial class DroppedEquipment : Component, IUse, Component.ICollisionListener
{
	[Property] public EquipmentResource Resource { get; set; }

	public Rigidbody Rigidbody { get; private set; }

	public static DroppedEquipment Create( EquipmentResource resource, Vector3 positon, Rotation? rotation = null, Equipment heldWeapon = null )
	{
		var go = new GameObject();
		go.Transform.Position = positon;
		go.Transform.Rotation = rotation ?? Rotation.Identity;
		go.Name = resource.Name;
		go.Tags.Add( "pickup" );

		var droppedWeapon = go.Components.Create<DroppedEquipment>();
		droppedWeapon.Resource = resource;

		var renderer = go.Components.Create<SkinnedModelRenderer>();
		renderer.Model = resource.WorldModel;

		var collider = go.Components.Create<BoxCollider>();
		collider.Scale = new( 8, 2, 8 );

		droppedWeapon.Rigidbody = go.Components.Create<Rigidbody>();

		go.Components.Create<DestroyBetweenRounds>();

		if ( resource.Slot == EquipmentSlot.Special )
		{
			Game.ActiveScene.Dispatch( new BombDroppedEvent() );

			Spottable spottable = go.Components.Get<Spottable>();
			spottable.Team = Team.Terrorist;
		}

		if ( heldWeapon is not null )
		{
			foreach ( var state in heldWeapon.Components.GetAll<IDroppedWeaponState>() )
			{
				state.CopyToDroppedWeapon( droppedWeapon );
			}
		}

		return droppedWeapon;
	}

	public bool CanUse( PlayerController player )
	{
		return player.Inventory.CanTake( Resource ) != PlayerInventory.PickupResult.None;
	}

	private bool _isUsed;

	public void OnUse( PlayerController player )
	{
		if ( _isUsed ) return;
		_isUsed = true;

		var weapon = player.Inventory.Give( Resource );

		foreach ( var state in weapon.Components.GetAll<IDroppedWeaponState>() )
		{
			state.CopyFromDroppedWeapon( this );
		}

		GameObject.Destroy();
	}

	void ICollisionListener.OnCollisionStart( Collision collision )
	{
		if ( !Networking.IsHost ) return;

		// Conna: this is longer than Daenerys Targaryen's full title.
		if ( collision.Other.GameObject.Root.Components.Get<PlayerController>( FindMode.EnabledInSelfAndDescendants ) is { } player )
		{
			// Don't pickup weapons if we're dead.
			if ( player.HealthComponent.State != LifeState.Alive )
				return;

			// If we last respawned less than 2 seconds ago then don't pickup. This is because
			// we need to give a chance for the owner to update its position. I want to add a way
			// to specify that Transform can be changed on non-owner too (prediction.)
			if ( player.TimeSinceLastRespawn < 2f )
				return;
			
			if ( player.Inventory.CanTake( Resource ) != PlayerInventory.PickupResult.Pickup )
				return;

			// Don't auto-pickup if we already have a weapon in this slot.
			if ( player.Inventory.HasInSlot( Resource.Slot ) )
				return;

			OnUse( player );
		}
	}
}
