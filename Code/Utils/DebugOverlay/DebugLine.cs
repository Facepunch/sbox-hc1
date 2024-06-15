namespace Facepunch;

public partial class DebugLine : Component
{
	[Property] public Vector3 StartPos { get; set; }
	[Property] public Vector3 EndPos { get; set; }
	[Property] public Color Color { get; set; }

	protected override void OnUpdate()
	{
		Gizmo.Transform = new Transform();

		Gizmo.Draw.Color = Color;
		Gizmo.Draw.Line( StartPos, EndPos );
	}
}
