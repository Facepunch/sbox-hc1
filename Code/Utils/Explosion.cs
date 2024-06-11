namespace Facepunch;

public static class Explosion
{	
	public static void AtPoint( Vector3 point, float radius, float baseDamage, Guid attackerId = default, Guid inflictorId = default, Curve falloff = default )
	{
		if ( falloff.Frames.Count == 0 )
		{
			falloff = new Curve( new Curve.Frame( 1.0f, 1.0f ), new Curve.Frame( 0.0f, 0.0f ) );
		}

		var scene = Game.ActiveScene;
		if ( !scene.IsValid() ) return;

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
			var damage = baseDamage * falloff.Evaluate( distance / radius );
			var direction = (obj.Transform.Position - point).Normal;
			var force = direction * distance * 50f;
			
			hc.TakeDamage( damage, point, force, attackerId, inflictorId );
		}
	}
}
