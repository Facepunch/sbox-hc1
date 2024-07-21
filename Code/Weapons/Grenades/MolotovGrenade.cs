using System.Threading.Tasks;

namespace Facepunch;

[Title( "Molotov Grenade" )]
public partial class MolotovGrenade : BaseGrenade, Component.ICollisionListener
{
	[Property] public GameObject FireNodePrefab { get; set; }
	[Property] public float FireRadius { get; set; } = 200f;
	[Property] public int FireNodesToSpawn { get; set; } = 20;
	[Property] public RangedFloat SpreadDelay { get; set; } = new( 0.1f, 0.2f );
	
	private bool IsSpreadingFire { get; set; }
	
	void ICollisionListener.OnCollisionStart( Collision collision )
	{
		if ( !Networking.IsHost || IsSpreadingFire )
			return;
		
		var isValidCollision = collision.Other.Collider is MapCollider;

		if ( !isValidCollision && collision.Other.GameObject.Tags.HasAny( "world", "solid" ) )
			isValidCollision = true;
		
		if ( !isValidCollision )
			return;
		
		var dot = collision.Contact.Normal.Dot( Vector3.Up );

		// Did we pretty much land on flat surface?
		if ( dot > -0.5f ) return;
		
		var position = collision.Contact.Point + collision.Contact.Normal * 8f;
		SpreadFireNetworked( position, Time.Now.CeilToInt() );
	}

	GameObject CreateFireNode( Vector3 position )
	{
		var node = FireNodePrefab.Clone( position, Rotation.Identity );
		var areaDamage = node.Components.Get<AreaDamage>();
		if ( areaDamage.IsValid() )
			areaDamage.Attacker = Player;

		return node;
	}

	[Broadcast]
	async void SpreadFireNetworked( Vector3 position, int seed )
	{
		if ( PrefabOnExplode.IsValid() )
			PrefabOnExplode.Clone( position );

		CreateFireNode( position );

		await SpreadFireInSphere( new( seed ), position );
		GameObject.Destroy();
	}

	async Task SpreadFireInSphere( Random rnd, Vector3 position )
	{
		IsSpreadingFire = true;

		var sunflower = Sunflower( FireNodesToSpawn, 2 );

		foreach ( var v in sunflower )
		{
			var targetPosition = new Vector3( position.x + v.x * FireRadius, position.y + v.y * FireRadius, position.z + 32f );
			var trace = Scene.Trace.Ray( position, targetPosition )
				.Run();
			
			trace = Scene.Trace.Ray( trace.EndPosition, trace.EndPosition + Vector3.Down * 48f )
				.Run();
			
			if ( !trace.Hit ) continue;

			var spawnPosition = trace.HitPosition + trace.Normal * 8f;
			CreateFireNode( spawnPosition );
			
			await Task.DelaySeconds( rnd.Float( SpreadDelay.RangeValue.x, SpreadDelay.RangeValue.y ) );
		}
	}
	
	List<Vector2> Sunflower( int n, float alpha = 0, bool geodesic = false )
	{
		var phi = (1 + MathF.Sqrt( 5 )) / 2;
		var stride = 360f * phi;
		
		float radius( float k, float d, float b )
		{
			return k > d - b ? 1 : MathF.Sqrt( k - 0.5f ) / MathF.Sqrt( d - (b + 1) / 2 );
		}
    
		var b = (int)(alpha * MathF.Sqrt( n ) );
		var points = new List<Vector2>();
		
		for ( var k = 0; k < n; k++ )
		{
			var r = radius( k, n, b );
			var theta = geodesic ? k * 360f * phi : k * stride;
			var x = !float.IsNaN( r * MathF.Cos( theta ) ) ? r * MathF.Cos( theta ) : 0;
			var y = !float.IsNaN( r * MathF.Sin( theta ) ) ? r * MathF.Sin( theta ) : 0;
			points.Add( new(x, y) );
		}
		
		return points;
	}
	
	protected override bool CanExplode()
	{
		return !IsSpreadingFire;
	}
}
