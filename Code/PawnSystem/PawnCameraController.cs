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

	/// <summary>
	/// The boom for this camera.
	/// </summary>
	[Property] 
	public GameObject Boom { get; set; }

	[Property]
	public GameObject CameraObject { get; set; }

	public bool IsActive { get; private set; }

	public virtual void SetActive( bool isActive )
	{
		IsActive = isActive;

		if ( isActive )
		{
			CameraObject.Enabled = true;

			Camera = CameraObject.GetOrAddComponent<CameraComponent>();
			Pixelate = Camera.GetOrAddComponent<Pixelate>();
			ChromaticAberration = Camera.GetOrAddComponent<ChromaticAberration>();
			AudioListener = Camera.GetOrAddComponent<AudioListener>();
			ScreenShaker = Camera.GetOrAddComponent<ScreenShaker>();

			// Optional
			ColorAdjustments = Camera.GetComponent<ColorAdjustments>();
		}
		else
		{
			CameraObject.Enabled = false;
		}
	}
}
