namespace Facepunch;

public class PlanarReflectionCamera : Component
{
	private Texture RenderTexture;
	[Property] public ModelRenderer Plane { get; set; }
	private CameraComponent _camera;
	private GameObject _cameraGameObject;

	private Vector2 _screenSize;

	protected override void OnEnabled()
	{
		GetOrCreateCamera();
		CreateRenderTexture();
	}

	protected override void OnDisabled()
	{
		_camera.Destroy();
		RenderTexture.Dispose();
	}

	private void CreateRenderTexture()
	{
		_screenSize = Screen.Size;
		RenderTexture = Texture.CreateRenderTarget().WithSize( Screen.Size / 8 ).WithFormat( ImageFormat.RGBA8888 ).Create();
	}

	void GetOrCreateCamera()
	{
		if ( _cameraGameObject.IsValid() )
			_cameraGameObject.Destroy();

		if ( _camera.IsValid() )
			_camera.Destroy();

		_cameraGameObject = new GameObject();
		_cameraGameObject.Parent = GameObject;
		_cameraGameObject.Flags = GameObjectFlags.NotSaved | GameObjectFlags.Hidden;

		_camera = _cameraGameObject.Components.Create<CameraComponent>();
		_camera.Flags = ComponentFlags.NotSaved;
		_camera.IsMainCamera = false;
		_camera.ZNear = 100;
		_camera.ZFar = 100000;
		_camera.ClearFlags = ClearFlags.All;
	}

	private void UpdateCameraTransform()
	{
		if ( !_cameraGameObject.IsValid() )
			GetOrCreateCamera();

		var cameraDir = Scene.Camera.Transform.Rotation.Forward;
		var cameraUp = Scene.Camera.Transform.Rotation.Up;
		var cameraPos = Scene.Camera.Transform.Position;

		cameraDir.z *= -1f;
		cameraUp.z *= -1f;

		var relCameraPos = Plane.Transform.World.PointToLocal( cameraPos );
		relCameraPos.z *= -1f;
		cameraPos = Plane.Transform.World.PointToWorld( relCameraPos );

		_camera.Transform.Position = cameraPos;
		_camera.Transform.Rotation = Rotation.LookAt( cameraDir, cameraUp );
		_camera.Transform.ClearInterpolation();
		_camera.Network.ClearInterpolation();

		_camera.FieldOfView = Scene.Camera.FieldOfView;
	}

	protected override void OnPreRender()
	{
		if ( !_cameraGameObject.IsValid() )
			GetOrCreateCamera();

		if ( Screen.Size != _screenSize )
		{
			RenderTexture.Dispose();
			CreateRenderTexture();
		}

		UpdateCameraTransform();
		_camera.RenderToTexture( RenderTexture );

		Plane.SceneObject.Attributes.Set( "PlanarReflectionTexture", RenderTexture );
		Plane.SceneObject.Attributes.Set( "PlanarReflections", true );
		Plane.SceneObject.RenderLayer = SceneRenderLayer.OverlayWithDepth;
	}
}
