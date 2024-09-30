namespace Facepunch;

public partial class PawnCameraController : Component
{
	// These components are cached.
	public CameraComponent Camera { get; set; }
	public AudioListener AudioListener { get; set; }
	public ColorAdjustments ColorAdjustments { get; set; }
	public ScreenShaker ScreenShaker { get; set; }
	public ChromaticAberration ChromaticAberration { get; set; }
	public Pixelate Pixelate { get; set; }

	public Pawn Pawn => GameObject.Root.GetComponentInChildren<Pawn>( true );

	/// <summary>
	/// The default player camera prefab.
	/// </summary>
	[Property] public GameObject DefaultPlayerCameraPrefab { get; set; }

	/// <summary>
	/// The boom for this camera.
	/// </summary>
	[Property] public GameObject Boom { get; set; }

	/// <summary>
	/// See <see cref="DefaultPlayerCameraPrefab"/>, this is the instance of this.
	/// </summary>
	public GameObject PlayerCameraGameObject { get; set; }

	public bool IsActive { get; private set; }

	private GameObject GetOrCreateCameraObject()
	{
		// I don't really get how this can happen.
		if ( !Scene.IsValid() )
		{
			return null;
		}

		var component = Scene.GetAllComponents<PlayerCameraOverride>().FirstOrDefault();

		var config = new CloneConfig()
		{
			StartEnabled = true,
			Parent = Boom,
			Transform = new Transform()
		};

		if ( component.IsValid() )
			return component.Prefab.Clone( config );

		return DefaultPlayerCameraPrefab?.Clone( config );
	}

	public virtual void SetActive( bool isActive )
	{
		IsActive = isActive;

		if ( PlayerCameraGameObject.IsValid() )
			PlayerCameraGameObject.Destroy();

		if ( isActive )
		{
			PlayerCameraGameObject = GetOrCreateCameraObject();

			if ( !PlayerCameraGameObject.IsValid() )
			{
				Log.Warning( "Couldn't make camera??" );
				return;
			}

			Camera = PlayerCameraGameObject.GetOrAddComponent<CameraComponent>();
			Pixelate = PlayerCameraGameObject.GetOrAddComponent<Pixelate>();
			ChromaticAberration = PlayerCameraGameObject.GetOrAddComponent<ChromaticAberration>();
			AudioListener = PlayerCameraGameObject.GetOrAddComponent<AudioListener>();
			ScreenShaker = PlayerCameraGameObject.GetOrAddComponent<ScreenShaker>();

			// Optional
			ColorAdjustments = PlayerCameraGameObject.GetComponent<ColorAdjustments>();
		}
	}
}
