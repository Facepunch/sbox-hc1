namespace Facepunch;

public partial class DroppedWeapon : Component, IUse, Component.ICollisionListener
{
	[Property] public WeaponData Resource { get; set; }

	public Rigidbody Rigidbody { get; private set; }

	public static DroppedWeapon Create( WeaponData resource, Vector3 positon, Rotation rotation = default )
	{
		var go = new GameObject();
		go.Tags.Set( "no_player", true );
		go.Transform.Position = positon;
		go.Transform.Rotation = rotation;
		go.Name = resource.Name;

		var droppedWeapon = go.Components.Create<DroppedWeapon>();
		droppedWeapon.Resource = resource;

		var renderer = go.Components.Create<SkinnedModelRenderer>();
		renderer.Model = resource.WorldModel;

		var collider = go.Components.Create<BoxCollider>();
		collider.Scale = new( 8, 2, 8 );

		droppedWeapon.Rigidbody = go.Components.Create<Rigidbody>();

		go.Components.Create<DestroyBetweenRounds>();

		return droppedWeapon;
	}

	public bool CanUse( PlayerController player )
	{
		return player.Inventory.CanTakeWeapon( Resource );
	}

	private bool _isUsed;

	public void OnUse( PlayerController player )
	{
		if ( _isUsed ) return;
		_isUsed = true;

		player.Inventory.GiveWeapon( Resource );
		GameObject.Destroy();
	}

	void ICollisionListener.OnCollisionStart( Collision collision )
	{
		if ( !Networking.IsHost ) return;
		
		// Conna: this is longer than Daenerys Targaryen's full title.
		if ( collision.Other.GameObject.Root.Components.Get<PlayerController>( FindMode.EnabledInSelfAndDescendants ) is { } player )
		{
			if ( !player.Inventory.CanTakeWeapon( Resource ) )
				return;

			// Don't auto-pickup if we already have a weapon in this slot.
			if ( player.Inventory.HasWeapon( Resource.Slot ) )
				return;

			OnUse( player );
		}
	}
}
