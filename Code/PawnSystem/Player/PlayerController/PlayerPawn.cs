namespace Facepunch;

public sealed partial class PlayerPawn : Pawn, IDescription, IAreaDamageReceiver
{
	/// <summary>
	/// The player's body
	/// </summary>
	[Property] public PlayerBody Body { get; set; }

	/// <summary>
	/// A reference to the player's head (the GameObject)
	/// </summary>
	[Property] public GameObject Head { get; set; }

	/// <summary>
	/// The current character controller for this player.
	/// </summary>
	[RequireComponent] public CharacterController CharacterController { get; set; }

	/// <summary>
	/// The current camera controller for this player.
	/// </summary>
	[RequireComponent] public CameraController CameraController { get; set; }

	/// <summary>
	/// The outline effect for this player.
	/// </summary>
	[RequireComponent] public HighlightOutline Outline { get; set; }

	/// <summary>
	/// The spotter for this player.
	/// </summary>
	[RequireComponent] public Spotter Spotter { get; set; }

	/// <summary>
	/// The spottable for this player.
	/// </summary>
	[RequireComponent] public Spottable Spottable { get; set; }

	/// <summary>
	/// Where are weapons on the player?
	/// </summary>
	[Property]
	public GameObject HoldGameObject { get; set; }

	/// <summary>
	/// Get a quick reference to the real Camera GameObject.
	/// </summary>
	public GameObject CameraGameObject
	{
		get
		{
			if ( !CameraController.IsValid() )
				return null;

			if ( !CameraController.Camera.IsValid() )
				return null;

			return CameraController.Camera.GameObject;
		}
	}

	/// <summary>
	/// Finds the first <see cref="SkinnedModelRenderer"/> on <see cref="Body"/>
	/// </summary>
	public SkinnedModelRenderer BodyRenderer => Body?.Components?.Get<SkinnedModelRenderer>();

	// IDescription
	string IDescription.DisplayName => DisplayName;
	Color IDescription.Color => Team.GetColor();

	// IAreaDamageReceiver
	void IAreaDamageReceiver.ApplyAreaDamage( AreaDamage component )
	{
		var dmg = new DamageInfo( component.Attacker, component.Damage, component.Inflictor, component.WorldPosition,
			Flags: component.DamageFlags );

		HealthComponent.TakeDamage( dmg );
	}

	/// <summary>
	/// Pawn
	/// </summary>
	// CameraComponent Pawn.Camera => CameraController.Camera;

	public override CameraComponent Camera => CameraController.Camera;

	public override string NameType => "Player";

	protected override void OnStart()
	{
		// TODO: expose these parameters please
		TagBinder.BindTag( "no_shooting", () => IsSprinting || TimeSinceSprintChanged < 0.25f || TimeSinceWeaponDeployed < 0.66f );
		TagBinder.BindTag( "no_aiming", () => IsSprinting || TimeSinceSprintChanged < 0.25f || TimeSinceGroundedChanged < 0.25f );

		GameObject.Name = $"Player ({DisplayName})";

		CameraController.SetActive( IsViewer );
	}

	public SceneTraceResult CachedEyeTrace { get; private set; }

	protected override void OnUpdate()
	{
		if ( HealthComponent.State == LifeState.Dead )
		{
			UpdateDead();
		}

		OnUpdateMovement();

		_smoothEyeHeight = _smoothEyeHeight.LerpTo( _eyeHeightOffset * (IsCrouching ? 1 : 0), Time.Delta * 10f );
		CharacterController.Height = Height + _smoothEyeHeight;

		if ( IsLocallyControlled )
		{
			DebugUpdate();
		}
	}

	private float DeathcamSkipTime => 5f;
	private float DeathcamIgnoreInputTime => 1f;

	// deathcam
	private void UpdateDead()
	{
		if ( !IsLocallyControlled )
			return;

		if ( !Client.IsValid() )
			return;

		if ( Client.LastDamageInfo is null )
			return;

		var killer = Client.GetLastKiller();

		if ( killer.IsValid() )
		{
			EyeAngles = Rotation.LookAt( killer.WorldPosition - WorldPosition, Vector3.Up );
		}

		if ( ((Input.Pressed( "attack1" ) || Input.Pressed( "attack2" )) && !Client.IsRespawning) || Client.IsBot || Client.LastDamageInfo.TimeSinceEvent > DeathcamSkipTime )
		{
			// Don't let players immediately switch
			if ( Client.LastDamageInfo.TimeSinceEvent < DeathcamIgnoreInputTime ) return;

			GameObject.Destroy();
			return;
		}
	}

	protected override void OnFixedUpdate()
	{
		if ( !Client.IsValid() )
			return;

		var cc = CharacterController;
		if ( !cc.IsValid() ) return;

		var wasGrounded = IsGrounded;
		IsGrounded = cc.IsOnGround;

		if ( IsGrounded != wasGrounded )
		{
			GroundedChanged( wasGrounded, IsGrounded );
		}

		UpdateEyes();
		UpdateZones();
		UpdateOutline();

		if ( IsViewer )
		{
			CachedEyeTrace = Scene.Trace.Ray( AimRay, 100000f )
				.IgnoreGameObjectHierarchy( GameObject )
				.WithoutTags( "ragdoll", "movement" )
				.UseHitboxes()
				.Run();
		}

		if ( HealthComponent.State != LifeState.Alive )
		{
			return;
		}

		if ( Networking.IsHost && Client.IsBot )
		{
			// BuildWishVelocity();

			// If we're a bot call these so they don't float in the air.
			ApplyAcceleration();
			ApplyMovement();
			return;
		}

		if ( !IsLocallyControlled )
		{
			return;
		}

		_previousVelocity = cc.Velocity;

		UpdatePlayArea();
		UpdateUse();
		BuildWishInput();
		BuildWishVelocity();
		BuildInput();

		UpdateRecoilAndSpread();
		ApplyAcceleration();

		if ( IsInVehicle )
		{
			ApplyVehicle();
		}
		else
		{
			ApplyMovement();
		}
	}

	[Sync( SyncFlags.FromHost )] public bool InPlayArea { get; set; } = true;
	[Sync( SyncFlags.FromHost )] public RealTimeUntil TimeUntilPlayAreaKill { get; set; } = 10f;
	[Property] public float OutOfPlayAreaKillTime { get; set; } = 5f;

	void UpdatePlayArea()
	{
		if ( !Networking.IsHost ) return;

		// Don't have any play areas, dont bother.
		if ( Scene.GetSystem<PlayAreaSystem>().Count < 1 )
			return;

		var playArea = GetZone<PlayArea>();
		if ( !playArea.IsValid() )
		{
			if ( InPlayArea )
			{
				Log.Info( $"No longer in play area, {OutOfPlayAreaKillTime}" );
				// not in the play area, kill them soon
				InPlayArea = false;
				TimeUntilPlayAreaKill = OutOfPlayAreaKillTime;
			}
		}
		else if ( !InPlayArea )
		{
			InPlayArea = true;
		}

		if ( !InPlayArea && TimeUntilPlayAreaKill )
		{
			HealthComponent.TakeDamage( new DamageInfo( this, 999, Hitbox: HitboxTags.Chest ) );
		}
	}
}
