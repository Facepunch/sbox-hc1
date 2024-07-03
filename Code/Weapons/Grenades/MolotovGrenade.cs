namespace Facepunch;

[Title( "Molotov Grenade" )]
public partial class MolotovGrenade : BaseGrenade, Component.ICollisionListener
{
	void ICollisionListener.OnCollisionStart( Collision collision )
	{
		if ( !Networking.IsHost )
			return;

		Explode();
	}
}
