using Facepunch;
using Sandbox.Events;

[Icon( "yard" )]
[Title( "Bomb Planting" ), Group( "Weapon Components" )]
public partial class DefuseC4 : WeaponInputAction
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

	[Sync( SyncFlags.FromHost )] public bool IsPlanting { get; private set; }

	/// <summary>
	/// Hold long since we started planting.
	/// </summary>
	[Sync( SyncFlags.FromHost )] public TimeSince TimeSincePlantStart { get; private set; }

	public float Progress => Math.Clamp( TimeSincePlantStart / PlantTime, 0f, 1f );

	/// <summary>
	/// Hold long since we aborted planting.
	/// </summary>
	[Sync( SyncFlags.FromHost )] public TimeSince TimeSincePlantCancel { get; private set; }

	private TimeSince TimeSinceBeep { get; set; }

	protected override void OnEnabled()
	{
		BindTag( "planting", () => IsPlanting );
	}

	private BombSite CurrentBombSite => Equipment.IsValid() && Equipment.Owner.IsValid() ? Equipment.Owner.GetZone<BombSite>() : null;

	/// <summary>
	/// Can we plant right now?
	/// </summary>
	public bool CanPlant()
	{
		if ( !Equipment.IsValid() ) return false;
		if ( !Equipment.Owner.IsValid() ) return false;

		if ( !Equipment.Owner.IsGrounded )
			return false;

		if ( !CurrentBombSite.IsValid() )
			return false;

		return true;
	}

	[Rpc.Broadcast]
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
		PlantBombOnHost( position ?? Equipment.Owner.WorldPosition, Rotation.FromYaw( Random.Shared.NextSingle() * 360f ), ignorePlanter );
	}

	[Rpc.Broadcast]
	private void PlantBombOnHost( Vector3 position, Rotation rotation, bool ignorePlanter )
	{
		if ( Equipment.IsValid() && Equipment.Owner.IsValid() && Equipment.Owner.BodyRenderer.IsValid() )
			Equipment.Owner.BodyRenderer.Set( "b_planting_bomb", false );

		if ( !Networking.IsHost )
			return;

		var player = Equipment.Owner;

		// Shouldn't happen, but if the owner is gone - disregard plant anyway
		if ( !player.IsValid() )
			return;

		player.Inventory.RemoveWeapon( Equipment );
		player.IsFrozen = false;

		if ( !PlantedObjectPrefab.IsValid() )
			return;

		var planted = PlantedObjectPrefab.Clone( position, rotation );

		// If the host leaves, we want to make the new host have authority over the bomb.
		planted.Network.SetOrphanedMode( NetworkOrphaned.ClearOwner );
		planted.NetworkSpawn();

		Scene.Dispatch( new BombPlantedEvent( ignorePlanter ? null : player, planted, CurrentBombSite ) );
	}

	[Rpc.Broadcast]
	private void CancelPlant()
	{
		if ( !Equipment.IsValid() )
			return;

		if ( Networking.IsHost )
		{
			IsPlanting = false;
			TimeSincePlantCancel = 0f;

			if ( Equipment.Owner.IsValid() )
			{
				Equipment.Owner.IsFrozen = false;
			}
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
				Sound.Play( PlantingBeepSound, Equipment.GameObject.WorldPosition );
			}
		}
	}

	protected override void OnEquipmentHolstered()
	{
		if ( Equipment.Owner.IsValid() && Equipment.Owner.BodyRenderer.IsValid() )
		{
			Equipment.Owner.BodyRenderer.Set( "b_planting_bomb", false );
		}
	}
}
