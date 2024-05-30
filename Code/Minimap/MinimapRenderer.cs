namespace Facepunch;

public partial class MinimapRenderer : Component, Component.ExecuteInEditor
{
	[Property, Group( "Rendering" )] public Vector2 ExportResolution { get; set; } = new( 2048, 2048 );
	[Property, Group( "Rendering" )] public int Size { get; set; }

	private GameObject CameraGameObject;
	public CameraComponent Camera { get; set; }

	public Vector2 RemapCoords( Vector3 worldPos )
	{
		// Define the center position and half size of the minimap area
		var origin = Transform.Position;
		var halfSize = Size * 0.5f;

		// Calculate the bounding box min and max coordinates
		var minX = origin.x - halfSize;
		var maxX = origin.x + halfSize;
		var minY = origin.y - halfSize;
		var maxY = origin.y + halfSize;

		// Normalize the world position within the bounding box
		var normalizedX = (worldPos.x - minX) / (maxX - minX);
		var normalizedY = (worldPos.y - minY) / (maxY - minY);

		// Map normalized coordinates to the minimap image resolution
		var minimapX = 1 - normalizedX;
		var minimapY = 1 - normalizedY; // Invert Y to match top-left origin

		// Return the mapped coordinates
		return new Vector2(minimapY, minimapX);
	}

	protected override void DrawGizmos()
	{
		Gizmo.Draw.Color = Color.Blue.WithAlpha( 0.8f );
		Gizmo.Draw.LineBBox( BBox.FromPositionAndSize( Vector3.Zero, Size ) );

		if ( Gizmo.IsSelected )
		{
			Gizmo.Draw.Color = Color.Blue.WithAlpha( 0.3f );
			Gizmo.Draw.SolidBox( BBox.FromPositionAndSize( Vector3.Zero, Size ) );
		}
    }

	void GetOrCreateCamera()
	{
        CameraGameObject?.Destroy();
        Camera?.Destroy();

        CameraGameObject = new GameObject();
		CameraGameObject.Parent = GameObject;
		CameraGameObject.Flags = GameObjectFlags.NotSaved | GameObjectFlags.Hidden;

		Camera = CameraGameObject.Components.Create<CameraComponent>();
		Camera.Flags = ComponentFlags.NotSaved;
		Camera.Orthographic = true;
		Camera.OrthographicHeight = Size;
    }

	protected override void OnUpdate()
	{
        if ( Game.IsPlaying )
		{
			CameraGameObject?.Destroy();
			return;
		}

		if ( !CameraGameObject.IsValid() )
		{
			GetOrCreateCamera();
        }

        Camera.Transform.Position = Transform.Position + Vector3.Up * Size;
        Camera.Transform.Rotation = Rotation.From( 90, 0, 0 );
        Camera.OrthographicHeight = Size;
	}
}
