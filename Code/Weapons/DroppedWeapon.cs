namespace Facepunch;

public partial class DroppedWeapon : Component, IUse
{
	[Property] public WeaponData Resource { get; set; }

	public Rigidbody Rigidbody { get; private set; }

	public static DroppedWeapon Create( WeaponData resource, Vector3 positon, Rotation rotation = default )
	{
		var go = new GameObject();
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
		return !player.Inventory.HasWeapon( Resource );
	}

	public bool OnUse( PlayerController player )
	{
		TryPickup( player.Id );
		return false;
	}

	[Broadcast]
	private void TryPickup( Guid pickerId )
	{
		if ( !Networking.IsHost )
			return;

		var player = Scene.Directory.FindComponentByGuid( pickerId ) as PlayerController;
		if ( !player.IsValid() )
			return;
		
		player.Inventory.GiveWeapon( Resource );
		GameObject.Destroy();
	}
}
