namespace Facepunch;

public class PlanarReflectionCamera : Component
{
	private Texture _renderTexture;
	private float _aspect;
	private CameraComponent _camera;
	private GameObject _cameraGameObject;

	[Property] public ModelRenderer Plane { get; set; }
	[Property] public int Resolution { get; set; } = 128;

	protected override void OnEnabled()
	{
		GetOrCreateCamera();
		GetOrCreateRenderTexture();
	}

	protected override void OnDisabled()
	{
		_camera.Destroy();
		_renderTexture.Dispose();
	}

	private void GetOrCreateRenderTexture()
	{
		if ( _aspect == Screen.Aspect )
			return;

		_aspect = Screen.Aspect;
		_renderTexture?.Dispose();

		var targetRes = new Vector2( Resolution * _aspect, Resolution );
		_renderTexture = Texture.CreateRenderTarget().WithSize( targetRes ).WithFormat( ImageFormat.RGBA8888 ).Create();
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

		var cameraDir = Scene.Camera.WorldRotation.Forward;
		var cameraUp = Scene.Camera.WorldRotation.Up;
		var cameraPos = Scene.Camera.WorldPosition;

		cameraDir.z *= -1f;
		cameraUp.z *= -1f;

		var relCameraPos = Plane.Transform.World.PointToLocal( cameraPos );
		relCameraPos.z *= -1f;
		cameraPos = Plane.Transform.World.PointToWorld( relCameraPos );

		_camera.WorldPosition = cameraPos;
		_camera.WorldRotation = Rotation.LookAt( cameraDir, cameraUp );
		_camera.Transform.ClearInterpolation();
		_camera.Network.ClearInterpolation();

		_camera.FieldOfView = Scene.Camera.FieldOfView;
	}

	protected override void OnPreRender()
	{
		if ( !_cameraGameObject.IsValid() )
			GetOrCreateCamera();

		if ( _aspect != Screen.Aspect )
			GetOrCreateRenderTexture();

		UpdateCameraTransform();
		_camera.RenderToTexture( _renderTexture );

		Plane.SceneObject.Attributes.Set( "PlanarReflectionTexture", _renderTexture );
		Plane.SceneObject.Attributes.Set( "PlanarReflections", true );
		Plane.SceneObject.RenderLayer = SceneRenderLayer.OverlayWithDepth;
	}
}
