namespace Facepunch;

public class PlanarReflectionCamera : Component
{
	private Texture RenderTexture;
	[Property] public ModelRenderer Plane { get; set; }
	[RequireComponent] public CameraComponent Camera { get; set; }

	private Vector2 _screenSize;

	protected override void OnEnabled()
	{
		CreateRenderTexture();
	}

	protected override void OnDisabled()
	{
		RenderTexture.Dispose();
	}

	private void CreateRenderTexture()
	{
		_screenSize = Screen.Size;
		RenderTexture = Texture.CreateRenderTarget().WithSize( Screen.Size / 8 ).WithFormat( ImageFormat.RGBA8888 ).Create();
	}

	protected override void OnUpdate()
	{
		if ( Screen.Size != _screenSize )
		{
			RenderTexture.Dispose();
			CreateRenderTexture();
		}

		var cameraDir = Scene.Camera.Transform.Rotation.Forward;
		var cameraUp = Scene.Camera.Transform.Rotation.Up;
		var cameraPos = Scene.Camera.Transform.Position;

		cameraDir.z *= -1f;
		cameraUp.z *= -1f;

		var relCameraPos = Plane.Transform.World.PointToLocal( cameraPos );
		relCameraPos.z *= -1f;
		cameraPos = Plane.Transform.World.PointToWorld( relCameraPos );

		Camera.Transform.Position = cameraPos;
		Camera.Transform.Rotation = Rotation.LookAt( cameraDir, cameraUp );
		Camera.ZNear = 100;
		Camera.ZFar = 100000;
		Camera.FieldOfView = Scene.Camera.FieldOfView;
		Camera.BackgroundColor = Color.Black;
		Camera.ClearFlags = ClearFlags.All;

		Camera.RenderToTexture( RenderTexture );

		Plane.SceneObject.Attributes.Set( "PlanarReflectionTexture", RenderTexture );
		Plane.SceneObject.RenderLayer = SceneRenderLayer.OverlayWithDepth;
		Plane.SceneObject.Attributes.SetCombo( "F_PLANAR_REFLECTIONS", true );
	}
}
