
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

	protected override void DrawGizmos()
	{
		Gizmo.Draw.Color = Gizmo.Colors.Red.WithAlpha( Gizmo.IsSelected ? 0.5f : 0.25f );

		foreach ( var collider in Components.GetAll<BoxCollider>() )
		{
			Gizmo.Transform = collider.Transform.World;

			Gizmo.Draw.SolidBox( BBox.FromPositionAndSize( collider.Center, collider.Scale ) );
		}
	}
}
