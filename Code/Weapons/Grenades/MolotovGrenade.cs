namespace Facepunch;

[Title( "Molotov Grenade" )]
public partial class MolotovGrenade : BaseGrenade, Component.ICollisionListener
{
	[Property] public GameObject AreaOfEffectPrefab { get; set; }
	
	void ICollisionListener.OnCollisionStart( Collision collision )
	{
		if ( !Networking.IsHost )
			return;
		
		var isValidCollision = collision.Other.Collider is MapCollider;

		if ( !isValidCollision && collision.Other.GameObject.Tags.HasAny( "world", "solid" ) )
			isValidCollision = true;
		
		if ( !isValidCollision )
			return;
		
		var dot = collision.Contact.Normal.Dot( Vector3.Up );

		// Did we pretty much land on flat surface?
		if ( dot > -0.5f ) return;
		
		var go = AreaOfEffectPrefab.Clone( Transform.Position );
		go.NetworkSpawn();
		Explode();
	}
}
