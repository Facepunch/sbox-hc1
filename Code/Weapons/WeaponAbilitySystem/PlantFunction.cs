
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

	public bool IsPlanting { get; private set; }

	/// <summary>
	/// Hold long since we started planting.
	/// </summary>
	private TimeSince TimeSincePlantStart { get; set; }

	/// <summary>
	/// Hold long since we aborted planting.
	/// </summary>
	private TimeSince TimeSincePlantCancel { get; set; }

	protected override void OnEnabled()
	{
		BindTag( "planting", () => IsPlanting );
	}

	/// <summary>
	/// Can we plant right now?
	/// </summary>
	public bool CanPlant()
	{
		// Player
		if ( Weapon.PlayerController.HasTag( "sprint" ) )
			return false;

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
		Log.Info( $"StartPlant" );

		IsPlanting = true;
		TimeSincePlantStart = 0f;
	}

	private void Plant()
	{
		Log.Info( $"Plant" );

		if ( TimeSincePlantStart > PlantTime )
		{
			FinishPlant();
		}
	}

	private void FinishPlant()
	{
		Weapon.PlayerController.Inventory.RemoveWeapon( Weapon );
	}

	private void CancelPlant()
	{
		Log.Info( $"CancelPlant" );

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
		Log.Info( $"OnFunctionDown" );

		if ( CanPlant() )
		{
			StartPlant();
		}
	}

	protected override void OnFunctionUp()
	{
		Log.Info( $"OnFunctionUp" );

		CancelPlant();
	}
}
