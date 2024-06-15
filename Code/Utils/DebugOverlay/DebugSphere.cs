namespace Facepunch;

public partial class DebugSphere : Component
{
	[Property] public float Size { get; set; } = 8f;
	[Property] public Color Color { get; set; } = Color.Red;

	protected override void OnUpdate()
	{
		Gizmo.Draw.Color = Color;
		Gizmo.Draw.LineSphere( Transform.Position, Size );
	}
}
