namespace Facepunch;

public partial class DroppedWeapon : Component
{
	[Property] public WeaponData Resource { get; set; }

	public static DroppedWeapon Create( WeaponData resource, Vector3 positon, Rotation rotation = default )
	{
		var go = new GameObject();
		go.Transform.Position = positon;
		go.Transform.Rotation = rotation;

		var droppedWeapon = go.Components.Create<DroppedWeapon>();
		droppedWeapon.Resource = resource;

		var renderer = go.Components.Create<SkinnedModelRenderer>();
		renderer.Model = resource.WorldModel;

		var collider = go.Components.Create<BoxCollider>();
		collider.Scale = new( 4, 4 );

		var rigidbody = go.Components.Create<Rigidbody>();

		return droppedWeapon;
	}
}
