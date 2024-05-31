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
		var objectsInArea = Game.ActiveScene.FindInPhysics( new Sphere( point, radius ) );

		foreach ( var obj in objectsInArea )
		{
			if ( obj.Root.Components.Get<HealthComponent>( FindMode.EnabledInSelfAndDescendants ) is not { } hc )
				continue;

			var distance = obj.Transform.Position.Distance( point );
			var damage = GetExplosionDamage( radius, distance, baseDamage );
			var direction = (obj.Transform.Position - point).Normal;
			var force = direction * distance * 50f;
			Log.Info( force );
			hc.TakeDamage( damage, point, force, attackerId, inflictorId );
		}
	}
}
