namespace Facepunch;

[Title( "Molotov Grenade" )]
public partial class MolotovGrenade : BaseGrenade, Component.ICollisionListener
{
	[Property] public GameObject FireNodePrefab { get; set; }
	[Property] public RangedFloat FireRadiusPerNode { get; set; } = new( 16f, 150f );
	[Property] public int FireNodesToSpawn { get; set; } = 20;
	
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

		//SpreadFire( collision.Contact.Point + collision.Contact.Normal * 8f, FireNodesToSpawn, FireSpreadCount );

		var position = collision.Contact.Point + collision.Contact.Normal * 8f;
		var go = FireNodePrefab.Clone( position );
		go.NetworkSpawn();

		SpreadFireNetworked( position, Time.Now.CeilToInt() );
		
		Explode();
	}

	[Broadcast]
	void SpreadFireNetworked( Vector3 position, int seed )
	{
		Game.SetRandomSeed( seed );
		SpreadFireInSphere( position, 16f, 150f );
	}

	void SpreadFireInSphere( Vector3 position, float minRadius, float maxRadius )
	{
		for ( var i = 0; i < FireNodesToSpawn; i++ )
		{
			var angle = Game.Random.Float( 0f, 360f );
			var randomRadius = FireRadiusPerNode.GetValue();
			var x = randomRadius * MathF.Cos( angle );
			var y = randomRadius * MathF.Sin( angle );
			var targetPosition = new Vector3( position.x + x, position.y + y, position.z + 32f );
			var trace = Scene.Trace.Ray( position, targetPosition )
				.Run();
			
			trace = Scene.Trace.Ray( trace.EndPosition, trace.EndPosition + Vector3.Down * 48f )
				.Run();
			
			if ( !trace.Hit ) continue;

			var spawnPosition = trace.HitPosition + trace.Normal * 8f;
			
			var go = FireNodePrefab.Clone( spawnPosition );
			go.NetworkSpawn();
		}
	}
}
