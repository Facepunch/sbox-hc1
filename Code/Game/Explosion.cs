namespace Facepunch;

public static class Explosion
{
	private static float GetExplosionDamage( float radius, float distance, float damage )
	{
		var t = 1f - Math.Clamp( distance / radius, 0f, 1f );
		return damage * t * t;
	}

	[Broadcast( NetPermission.HostOnly )]
	public static void AtPoint( Vector3 point, float radius, float baseDamage, Guid attackerId = default, Guid inflictorId = default )
	{
		var scene = Game.ActiveScene;
		if ( !scene.IsValid() )
			return;

		var objectsInArea = scene.FindInPhysics( new Sphere( point, radius ) );
		var inflictor = scene.Directory.FindComponentByGuid( inflictorId );
		var inflictorRoot = inflictor?.GameObject?.Root;

		var trace = scene.Trace
			.WithoutTags( "trigger", "invis", "ragdoll" );

		if ( inflictorRoot.IsValid() )
			trace = trace.IgnoreGameObjectHierarchy( inflictorRoot );

		foreach ( var obj in objectsInArea )
		{
			if ( obj.Root.Components.Get<HealthComponent>( FindMode.EnabledInSelfAndDescendants ) is not { } hc )
				continue;

			// If the object isn't in line of sight, fuck it off
			var tr = trace.Ray( point, obj.Transform.Position ).Run();
			if ( tr.Hit && tr.GameObject.IsValid() )
			{
				if ( !obj.Root.IsDescendant( tr.GameObject ) )
					continue;
			}

			var distance = obj.Transform.Position.Distance( point );
			var damage = GetExplosionDamage( radius, distance, baseDamage );
			var direction = (obj.Transform.Position - point).Normal;
			var force = direction * distance * 50f;
			
			hc.TakeDamage( damage, point, force, attackerId, inflictorId );
		}
	}
}
