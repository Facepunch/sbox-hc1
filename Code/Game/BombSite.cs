
/// <summary>
/// C4 can be planted anywhere within colliders inside this object.
/// </summary>
public sealed class BombSite : Component
{
	/// <summary>
	/// Which bomb site is this (A: 0, B: 1, maybe more!).
	/// </summary>
	[Property]
	public int Index { get; set; }

	/// <summary>
	/// Damage done at the epicenter of bombs planted here.
	/// </summary>
	[Property]
	public float MaxBombDamage { get; set; } = 500f;

	/// <summary>
	/// Furthest distance at which bombs planted here do damage.
	/// </summary>
	[Property]
	public float DamageFalloffDistance { get; set; } = 1750f;

	/// <summary>
	/// How much damage does a bomb planted here do at a given distance?
	/// </summary>
	public float GetExplosionDamage( float distance )
	{
		var t = 1f - Math.Clamp( distance / DamageFalloffDistance, 0f, 1f );

		// Let's square it, so it falls off sharply near the bomb,
		// but has a relatively long tail

		return MaxBombDamage * t * t;
	}

	/// <summary>
	/// Maximum distance that a bomb planted here will do 100 damage.
	/// </summary>
	public float MaxLethalDistance
	{
		get
		{
			if ( MaxBombDamage < 100f )
			{
				return 0f;
			}

			var t = 1f - MathF.Sqrt( 100f / MaxBombDamage );

			return t * DamageFalloffDistance;
		}
	}

	protected override void DrawGizmos()
	{
		Gizmo.Draw.Color = Gizmo.Colors.Red.WithAlpha( Gizmo.IsSelected ? 0.5f : 0.25f );

		foreach ( var collider in Components.GetAll<BoxCollider>() )
		{
			Gizmo.Transform = collider.Transform.World;

			Gizmo.Draw.SolidBox( BBox.FromPositionAndSize( collider.Center, collider.Scale ) );
		}

		if ( !Gizmo.IsSelected )
		{
			return;
		}

		Gizmo.Transform = global::Transform.Zero;

		Gizmo.Draw.Color = Gizmo.Colors.Red;
		Gizmo.Draw.LineSphere( Transform.Position, MaxLethalDistance );

		Gizmo.Draw.Color = Gizmo.Colors.Green;
		Gizmo.Draw.LineSphere( Transform.Position, DamageFalloffDistance );
	}
}
