
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

	[HostSync] public bool IsPlanting { get; private set; }

	/// <summary>
	/// Hold long since we started planting.
	/// </summary>
	[HostSync] private TimeSince TimeSincePlantStart { get; set; }

	/// <summary>
	/// Hold long since we aborted planting.
	/// </summary>
	[HostSync] private TimeSince TimeSincePlantCancel { get; set; }

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

		// Delay checks
		if ( TimeSincePlantCancel < ResetTime )
			return false;

		if ( !Weapon.PlayerController.Zones.Any( x => x.Components.Get<BombSite>() is not null ) )
			return false;

		return true;
	}

	[Broadcast]
	private void StartPlant()
	{
		if ( Networking.IsHost )
		{
			TimeSincePlantStart = 0f;
			IsPlanting = true;
		}
	}

	private void FinishPlant()
	{
		PlantBombOnHost( Weapon.PlayerController.Transform.Position, Rotation.FromYaw( Random.Shared.NextSingle() * 360f ) );
	}

	[Broadcast]
	private void PlantBombOnHost( Vector3 position, Rotation rotation )
	{
		if ( !Networking.IsHost ) return;
		
		Weapon.PlayerController.Inventory.RemoveWeapon( Weapon );
		
		if ( PlantedObjectPrefab is null ) return;
		
		var planted = PlantedObjectPrefab.Clone( position, rotation );
		
		// If the host leaves, we want to make the new host have authority over the bomb.
		planted.Network.SetOrphanedMode( NetworkOrphaned.ClearOwner );
		planted.NetworkSpawn();
	}
	
	[Broadcast]
	private void CancelPlant()
	{
		if ( Networking.IsHost )
		{
			IsPlanting = false;
			TimeSincePlantCancel = 0f;
		}
	}

	protected override void OnFixedUpdate()
	{
		if ( Networking.IsHost )
		{
			if ( IsPlanting && CanPlant() )
			{
				if ( TimeSincePlantStart > PlantTime )
				{
					FinishPlant();
				}
			}
			else if ( IsPlanting )
			{
				CancelPlant();
			}
		}
		
		base.OnFixedUpdate();
	}

	protected override void OnFunctionExecute()
	{
		
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

			if ( PlantingBeepSound is not null )
			{
				Sound.Play( PlantingBeepSound, Weapon.GameObject.Transform.Position );
			}
		}
	}
}
