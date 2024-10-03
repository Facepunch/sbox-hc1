namespace Facepunch.Gunsmith;

/// <summary>
/// The manager for gunsmith, we'll place this in the gunsmith scene and handle state.
/// </summary>
public partial class GunsmithSystem : Component
{
	[Property]
	public EquipmentResource Equipment { get; set; }

	[Property]
	public SkinnedModelRenderer Renderer { get; set; }

	[Property]
	public GunsmithController Controller { get; set; }

	public GunsmithWeapon Weapon => Controller.Weapon;

	/// <summary>
	/// Are we in FPS mode?
	/// </summary>
	public bool FPS { get; private set; } = false;

	/// <summary>
	/// Sets the FPS mode
	/// </summary>
	/// <param name="fps"></param>
	public void SetFirstPerson( bool fps )
	{
		FPS = fps;
		Renderer.UseAnimGraph = fps;
		Weapon.ViewModel.Arms.Enabled = fps;

		if ( fps )
		{
			Renderer.GameObject.SetParent( Controller.CameraComponent.GameObject, false );
		}
		else
		{
			Renderer.GameObject.SetParent( GameObject, false );

		}

		Renderer.LocalPosition = 0;
	}

	[Button( "Toggle FPS" )]
	public void Toggle()
	{
		SetFirstPerson( !FPS );
	}

	protected override void OnStart()
	{
		var inst = Equipment.ViewModelPrefab.Clone( new CloneConfig()
		{
			Name = "Runtime Weapon",
			Parent = GameObject,
			StartEnabled = true,
			Transform = new()
		} );

		var weapon = inst.GetComponent<GunsmithWeapon>();

		// Turn off the viewmodel' arms in gunsmith, we do not need it.
		weapon.ViewModel.Arms.Enabled = false;

		Renderer = inst.GetComponent<SkinnedModelRenderer>();
		Renderer.UseAnimGraph = false;
		Controller.Weapon = weapon;
	}
}
