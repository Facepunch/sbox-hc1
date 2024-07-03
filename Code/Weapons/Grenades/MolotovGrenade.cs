namespace Facepunch;

[Title( "Molotov Grenade" )]
public partial class MolotovGrenade : BaseGrenade, Component.ICollisionListener
{
	void ICollisionListener.OnCollisionStart( Collision collision )
	{
		if ( !Networking.IsHost )
			return;
		
		var isValidCollision = collision.Other.Collider is MapCollider;

		if ( !isValidCollision && collision.Other.GameObject.Tags.HasAny( "world", "solid" ) )
			isValidCollision = true;
		
		if ( !isValidCollision )
			return;

		Explode();
	}
}
