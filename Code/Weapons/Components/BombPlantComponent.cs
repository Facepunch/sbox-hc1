using Facepunch;
using Sandbox.Events;

[Icon( "yard" )]
[Title( "Bomb Planting" ), Group( "Weapon Components" )]
public partial class BombPlantComponent : InputWeaponComponent, 
	IGameEventHandler<EquipmentHolsteredEvent>
{
	/// <summary>
	/// How long the input must be held to plant.
	/// </summary>
	[Property, Category( "Config" )]
	public float PlantTime { get; set; } = 3.2f;

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
	[HostSync] public TimeSince TimeSincePlantStart { get; private set; }

	public float Progress => Math.Clamp( TimeSincePlantStart / PlantTime, 0f, 1f );

	/// <summary>
	/// Hold long since we aborted planting.
	/// </summary>
	[HostSync] public TimeSince TimeSincePlantCancel { get; private set; }

	private TimeSince TimeSinceBeep { get; set; }

	protected override void OnEnabled()
	{
		BindTag( "planting", () => IsPlanting );
	}

	private BombSite CurrentBombSite => Equipment.Owner.GetZone<BombSite>();

	/// <summary>
	/// Can we plant right now?
	/// </summary>
	public bool CanPlant()
	{
		if ( !Equipment.Owner.IsGrounded )
			return false;

		if ( CurrentBombSite is null )
			return false;

		return true;
	}

	[Broadcast]
	private void BroadcastPlant()
	{
		if ( Networking.IsHost )
		{
			TimeSincePlantStart = 0f;
			IsPlanting = true;

			var player = Equipment.Owner;
			if ( player.IsValid() )
			{
				player.IsFrozen = true;
			}
		}

		if ( !Equipment.IsValid() )
			return;

		if ( !Equipment.Owner.IsValid() )
			return;

		if ( Equipment.Owner.BodyRenderer.IsValid() )
			Equipment.Owner.BodyRenderer.Set( "b_planting_bomb", true );
	}

	private void StartPlant()
	{
		// Send the radio sound to everyone
		if ( Equipment.Owner.IsLocallyControlled )
		{
			RadioSounds.Play( Equipment.Owner.Team, "Hidden", "Planting bomb" );
		}

		BroadcastPlant();
	}

	public void FinishPlant( Vector3? position = null, bool ignorePlanter = false )
	{
		PlantBombOnHost( position ?? Equipment.Owner.Transform.Position, Rotation.FromYaw( Random.Shared.NextSingle() * 360f ), ignorePlanter );
	}

	[Broadcast]
	private void PlantBombOnHost( Vector3 position, Rotation rotation, bool ignorePlanter )
	{
		if ( Equipment.Owner?.BodyRenderer is { IsValid: true } bodyRenderer )
			bodyRenderer.Set( "b_planting_bomb", false );

		if ( !Networking.IsHost )
			return;

		var player = Equipment.Owner;

		player.Inventory.RemoveWeapon( Equipment );
		player.IsFrozen = false;
		
		if ( PlantedObjectPrefab is null ) return;
		
		var planted = PlantedObjectPrefab.Clone( position, rotation );
		
		// If the host leaves, we want to make the new host have authority over the bomb.
		planted.Network.SetOrphanedMode( NetworkOrphaned.ClearOwner );
		planted.NetworkSpawn();

		Scene.Dispatch( new BombPlantedEvent( ignorePlanter ? null : player, planted, CurrentBombSite ) );
	}
	
	[Broadcast]
	private void CancelPlant()
	{
		if ( Networking.IsHost )
		{
			IsPlanting = false;
			TimeSincePlantCancel = 0f;

			var player = Equipment.Owner;
			player.IsFrozen = false;
		}

		if ( Equipment.Owner.IsValid() && Equipment.Owner.BodyRenderer.IsValid() )
		{
			Equipment.Owner.BodyRenderer.Set( "b_planting_bomb", false );
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

	protected override void OnInput()
	{
		
	}

	protected override void OnInputDown()
	{
		if ( CanPlant() )
		{
			StartPlant();
		}
	}

	protected override void OnInputUp()
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
		if ( TimeSinceBeep > PlantingBeepFrequency.Evaluate( Progress ) )
		{
			TimeSinceBeep = 0f;

			if ( PlantingBeepSound is not null )
			{
				Sound.Play( PlantingBeepSound, Equipment.GameObject.Transform.Position );
			}
		}
	}

	void IGameEventHandler<EquipmentHolsteredEvent>.OnGameEvent( EquipmentHolsteredEvent eventArgs )
	{
		if ( Equipment.Owner.IsValid() && Equipment.Owner.BodyRenderer.IsValid() )
		{
			Equipment.Owner.BodyRenderer.Set( "b_planting_bomb", false );
		}
	}
}
