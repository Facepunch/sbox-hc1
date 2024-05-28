
using Facepunch;

[Icon( "yard" )]
public partial class PlantFunction : InputActionWeaponFunction
{
	/// <summary>
	/// How long the input must be held to plant.
	/// </summary>
	[Property, Category( "Config" )]
	public float PlantTime { get; set; } = 3.2f;

	/// <summary>
	/// If you cancel planting, how long until you can plant again.
	/// </summary>
	[Property, Category( "Config" )]
	public float ResetTime { get; set; } = 0.5f;

	[Property, Category( "Config" )]
	public GameObject PlantedObjectPrefab { get; set; }

	/// <summary>
	/// TODO: this will get tied into an animation.
	/// </summary>
	[Property, Category( "Effects" )]
	public SoundEvent PlantingBeepSound { get; set; }

	[Property, Category( "Effects" )]
	public Curve PlantingBeepFrequency { get; set; } = new Curve( new Curve.Frame( 0f, 1f ), new Curve.Frame( 1f, 0.25f ) );

	[Sync]
	public bool IsPlanting { get; private set; }

	/// <summary>
	/// Hold long since we started planting.
	/// </summary>
	[Sync]
	private TimeSince TimeSincePlantStart { get; set; }

	/// <summary>
	/// Hold long since we aborted planting.
	/// </summary>
	[Sync]
	private TimeSince TimeSincePlantCancel { get; set; }

	private TimeSince TimeSinceBeep { get; set; }

	protected override void OnEnabled()
	{
		BindTag( "planting", () => IsPlanting );
	}

	/// <summary>
	/// Can we plant right now?
	/// </summary>
	public bool CanPlant()
	{
		if ( !Weapon.PlayerController.IsGrounded )
			return false;

		// TODO: bomb site

		// Delay checks
		if ( TimeSincePlantCancel < ResetTime )
		{
			return false;
		}

		return true;
	}

	private void StartPlant()
	{
		IsPlanting = true;
		TimeSincePlantStart = 0f;
	}

	private void Plant()
	{
		if ( TimeSincePlantStart > PlantTime )
		{
			FinishPlant();
		}
	}

	private void FinishPlant()
	{
		Weapon.PlayerController.Inventory.RemoveWeapon( Weapon );

		if ( PlantedObjectPrefab is null )
		{
			return;
		}

		var planted = PlantedObjectPrefab.Clone( Weapon.PlayerController.Transform.Position, Rotation.FromYaw( Random.Shared.NextSingle() * 360f ) );

		planted.NetworkSpawn( Connection.Host );
	}

	private void CancelPlant()
	{
		IsPlanting = false;
		TimeSincePlantCancel = 0f;
	}

	protected override void OnFunctionExecute()
	{
		if ( IsPlanting && CanPlant() )
		{
			Plant();
		}
		else if ( IsPlanting )
		{
			CancelPlant();
		}
	}

	protected override void OnFunctionDown()
	{
		if ( CanPlant() )
		{
			StartPlant();
		}
	}

	protected override void OnFunctionUp()
	{
		CancelPlant();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !IsPlanting )
		{
			TimeSinceBeep = 0f;
			return;
		}

		// TODO: this will get tied into an animation

		var t = Math.Clamp( TimeSincePlantStart / PlantTime, 0f, 1f );

		if ( TimeSinceBeep > PlantingBeepFrequency.Evaluate( t ) )
		{
			TimeSinceBeep = 0f;

			Log.Info( $"Beep {PlantingBeepSound}" );

			if ( PlantingBeepSound is not null )
			{
				Sound.Play( PlantingBeepSound, Weapon.GameObject.Transform.Position );
			}
		}
	}
}
