/// <summary>
/// C4 can be planted anywhere within colliders inside this object.
/// </summary>
public sealed partial class BombSite : Component
{
	/// <summary>
	/// The zone defining the bounds of this bomb site.
	/// </summary>
	[RequireComponent]
	public Zone Zone { get; private set; }

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

	protected override void OnValidate()
	{
		base.OnValidate();

		Zone.Color = Color.Red;
		Zone.DisplayName = $"Bomb Site {(char)('A' + Index)}";
	}

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
		if ( !Gizmo.IsSelected )
		{
			return;
		}

		if ( !Facepunch.Preferences.ShowVolumes )
		{
			return;
		}

		Gizmo.Transform = global::Transform.Zero;

		Gizmo.Draw.Color = Gizmo.Colors.Red;
		Gizmo.Draw.LineSphere( WorldPosition, MaxLethalDistance );

		Gizmo.Draw.Color = Gizmo.Colors.Green;
		Gizmo.Draw.LineSphere( WorldPosition, DamageFalloffDistance );
	}
}
