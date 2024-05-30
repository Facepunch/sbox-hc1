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
			if ( obj.Root.Components.Get<HealthComponent>( FindMode.EnabledInSelfAndDescendants ) is { } hc )
			{
				var damage = GetExplosionDamage( radius, obj.Transform.Position.Distance( point ), baseDamage );
				// TODO: force
				Vector3 force = 0;
				hc.TakeDamage( damage, point, force, attackerId, inflictorId, false );
			}
		}
	}
}
