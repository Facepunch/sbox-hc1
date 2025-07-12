namespace Facepunch;

public partial class PlayerPawn
{
	/// <summary>
	/// Called when the player jumps.
	/// </summary>
	[Property] public Action OnJump { get; set; }

	/// <summary>
	/// The player's box collider, so people can jump on other people.
	/// </summary>
	[Property] public BoxCollider PlayerBoxCollider { get; set; }

	[RequireComponent] public TagBinder TagBinder { get; set; }

	/// <summary>
	/// How tall are we?
	/// </summary>
	[Property, Group( "Config" )] public float Height { get; set; } = 64f;

	[Property, Group( "Fall Damage" )] public float MinimumFallVelocity { get; set; } = 500f;
	[Property, Group( "Fall Damage" )] public float MinimumFallSoundVelocity { get; set; } = 300f;
	[Property, Group( "Fall Damage" )] public float FallDamageScale { get; set; } = 0.2f;

	[Property, Group( "Sprint" )] public float SprintMovementDampening { get; set; } = 0.35f;

	/// <summary>
	/// Noclip movement speed
	/// </summary>
	[Property] public float NoclipSpeed { get; set; } = 1000f;

	public PlayerGlobals Global => GetGlobal<PlayerGlobals>();

	/// <summary>
	/// Look direction of this player. Smoothly interpolated for networked players.
	/// </summary>
	public override Angles EyeAngles
	{
		get => _smoothEyeAngles;
		set
		{
			if ( !IsProxy ) _smoothEyeAngles = value;
			_rawEyeAngles = value;
		}
	}
	[Sync] private Angles _rawEyeAngles { get; set; }
	private Angles _smoothEyeAngles;

	/// <summary>
	/// Is the player crouching?
	/// </summary>
	[Sync] public bool IsCrouching { get; set; }

	/// <summary>
	/// Is the player slow walking?
	/// </summary>
	[Sync] public bool IsSlowWalking { get; set; }

	/// <summary>
	/// Are we sprinting?
	/// </summary>
	[Sync] public bool IsSprinting { get; set; }

	/// <summary>
	/// Is the player noclipping?
	/// </summary>
	[Sync] public bool IsNoclipping { get; set; }

	/// <summary>
	/// If true, we're not allowed to move.
	/// </summary>
	[Sync( SyncFlags.FromHost )] public bool IsFrozen { get; set; }

	/// <summary>
	/// Last time this player moved or attacked.
	/// </summary>
	[Sync] public TimeSince TimeSinceLastInput { get; private set; }

	/// <summary>
	/// What's our holdtype?
	/// </summary>
	[Sync] AnimationHelper.HoldTypes CurrentHoldType { get; set; } = AnimationHelper.HoldTypes.None;

	/// <summary>
	/// How quick do we wish to go?
	/// </summary>
	private Vector3 WishVelocity { get; set; }

	/// <summary>
	/// Are we on the ground?
	/// </summary>
	public bool IsGrounded { get; set; }

	/// <summary>
	/// How quick do we wish to go (normalized)
	/// </summary>
	public Vector3 WishMove { get; set; }

	/// <summary>
	/// How much friction to apply to the aim eg if zooming
	/// </summary>
	public float AimDampening { get; set; } = 1.0f;

	/// <summary>
	/// An accessor to get the camera controller's aim ray.
	/// </summary>
	public Ray AimRay => CameraController.AimRay;

	private float _smoothEyeHeight;
	private Vector3 _previousVelocity;
	private bool _isTouchingLadder;
	private Vector3 _ladderNormal;

	[Sync] private float _eyeHeightOffset { get; set; }

	private void UpdateEyes()
	{
		if ( IsLocallyControlled )
		{
			var eyeHeightOffset = GetEyeHeightOffset();

			var target = eyeHeightOffset;
			var trace = TraceBBox( WorldPosition, WorldPosition, 0, 10f );
			if ( trace.Hit && target > _smoothEyeHeight )
			{
				// We hit something, that means we can't increase our eye height because something's in the way.
				eyeHeightOffset = _smoothEyeHeight;
				IsCrouching = true;
			}
			else
			{
				eyeHeightOffset = target;
			}

			_eyeHeightOffset = eyeHeightOffset;
		}

		if ( PlayerBoxCollider.IsValid() )
		{
			// Bit shit, but it works
			PlayerBoxCollider.Center = new( 0, 0, 32 + _smoothEyeHeight );
			PlayerBoxCollider.Scale = new( 32, 32, 64 + _smoothEyeHeight );
		}
	}

	TimeUntil TimeUntilAccelerationRecovered = 0;
	float AccelerationAddedScale = 0;

	private void ApplyAcceleration()
	{
		var relative = TimeUntilAccelerationRecovered.Fraction.Clamp( 0, 1 );
		var acceleration = GetAcceleration();

		acceleration *= (relative + AccelerationAddedScale).Clamp( 0, 1 );

		CharacterController.Acceleration = acceleration;
	}

	private void OnUpdateMovement()
	{
		var cc = CharacterController;
		CurrentHoldType = CurrentEquipment.IsValid() ? CurrentEquipment.GetHoldType() : AnimationHelper.HoldTypes.None;

		if ( !IsLocallyControlled )
		{
			_smoothEyeAngles = Angles.Lerp( _smoothEyeAngles, _rawEyeAngles, Time.Delta / Scene.NetworkRate );
		}

		// Eye input
		if ( IsPossessed && cc.IsValid() )
		{
			if ( IsLocallyControlled && HealthComponent.State == LifeState.Alive )
			{
				EyeAngles += Input.AnalogLook * AimDampening;
				EyeAngles = EyeAngles.WithPitch( EyeAngles.pitch.Clamp( -90, 90 ) );
			}

			CameraController.UpdateFromEyes( _smoothEyeHeight );
		}

		if ( Body.IsValid() )
		{
			WorldRotation = Rotation.From( 0, EyeAngles.yaw, 0 );
			Body.UpdateRotation( Rotation.FromYaw( EyeAngles.yaw ) );

			foreach ( var helper in Body.AnimationHelpers )
			{
				if ( !helper.IsValid() ) continue;

				helper.WithVelocity( cc.Velocity );
				helper.WithWishVelocity( WishVelocity );
				helper.IsGrounded = IsGrounded;
				helper.WithLook( EyeAngles.Forward );
				helper.MoveStyle = AnimationHelper.MoveStyles.Run;
				helper.DuckLevel = (MathF.Abs( _smoothEyeHeight ) / 32.0f);
				helper.HoldType = CurrentHoldType;
				helper.Handedness = CurrentEquipment.IsValid() ? CurrentEquipment.Handedness : AnimationHelper.Hand.Both;
			}
		}

		AimDampening = 1.0f;
	}

	private float GetMaxAcceleration()
	{
		if ( !CharacterController.IsOnGround ) return Global.AirMaxAcceleration;
		return Global.MaxAcceleration;
	}

	private void ApplyMovement()
	{
		var cc = CharacterController;

		CheckLadder();

		var gravity = Global.Gravity;

		if ( _isTouchingLadder )
		{
			LadderMove();
			return;
		}

		if ( cc.IsOnGround )
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
			cc.Accelerate( WishVelocity );
		}
		else
		{
			if ( !IsNoclipping )
			{
				cc.Velocity -= gravity * Time.Delta * 0.5f;
			}
			cc.Accelerate( WishVelocity.ClampLength( GetMaxAcceleration() ) );
		}

		if ( !cc.IsOnGround )
		{
			if ( !IsNoclipping )
			{
				cc.Velocity -= gravity * Time.Delta * 0.5f;
			}
		}
		else
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
		}

		if ( IsNoclipping )
		{
			var vertical = 0f;
			if ( Input.Down( "Jump" ) ) vertical = 1f;
			if ( Input.Down( "Duck" ) ) vertical = -1f;

			cc.IsOnGround = false;
			cc.Velocity = WishMove.Normal * EyeAngles.ToRotation() * NoclipSpeed;
			cc.Velocity += Vector3.Up * vertical * NoclipSpeed;
		}

		cc.ApplyFriction( GetFriction() );
		cc.Move();
	}

	private bool WantsToSprint => Input.Down( "Run" ) && !IsSlowWalking && !HasEquipmentTag( "no_sprint" ) && (WishMove.x > 0.2f || (MathF.Abs( WishMove.y ) > 0.2f && WishMove.x >= 0f));
	TimeSince TimeSinceSprintChanged { get; set; } = 100;

	private void OnSprintChanged( bool before, bool after )
	{
		TimeSinceSprintChanged = 0;
	}
	public bool HasEquipmentTag( string flag )
	{
		return CurrentEquipment.IsValid() && CurrentEquipment.HasTag( flag );
	}

	private void BuildInput()
	{
		bool wasSprinting = IsSprinting;

		IsSlowWalking = Input.Down( "Walk" ) || HasEquipmentTag( "aiming" );
		IsSprinting = WantsToSprint;

		if ( wasSprinting != IsSprinting )
		{
			OnSprintChanged( wasSprinting, IsSprinting );
		}

		IsCrouching = Input.Down( "Duck" ) && !IsNoclipping;
		IsUsing = Input.Down( "Use" );

		// Check if our current weapon has the planting tag and if so force us to crouch.
		var currentWeapon = CurrentEquipment;
		if ( currentWeapon.IsValid() && currentWeapon.Tags.Has( "planting" ) )
			IsCrouching = true;

		if ( Input.Pressed( "Noclip" ) && Game.IsEditor )
		{
			IsNoclipping = !IsNoclipping;
		}

		if ( WishMove.LengthSquared > 0.01f || Input.Down( "Attack1" ) )
		{
			TimeSinceLastInput = 0f;
		}

		if ( CharacterController.IsOnGround && !IsFrozen )
		{
			var bhop = Global.BunnyHopping;
			if ( bhop ? Input.Down( "Jump" ) : Input.Pressed( "Jump" ) )
			{
				CharacterController.Punch( Vector3.Up * Global.JumpPower * 1f );
				BroadcastPlayerJumped();
			}
		}
	}

	public SceneTraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f, float liftHead = 0.0f )
	{
		var bbox = CharacterController.BoundingBox;
		var mins = bbox.Mins;
		var maxs = bbox.Maxs;

		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		if ( liftHead > 0 )
		{
			end += Vector3.Up * liftHead;
		}

		var tr = Scene.Trace.Ray( start, end )
					.Size( mins, maxs )
					.WithoutTags( CharacterController.IgnoreLayers )
					.IgnoreGameObjectHierarchy( GameObject.Root )
					.Run();
		return tr;
	}

	/// <summary>
	/// A network message that lets other users that we've triggered a jump.
	/// </summary>
	[Rpc.Broadcast]
	public void BroadcastPlayerJumped()
	{
		foreach ( var helper in Body.AnimationHelpers )
		{
			if ( !helper.IsValid() ) continue;
			helper.TriggerJump();
		}

		OnJump?.Invoke();
	}

	public TimeSince TimeSinceGroundedChanged { get; private set; }

	private void GroundedChanged( bool wasOnGround, bool isOnGround )
	{
		if ( !IsLocallyControlled )
			return;

		TimeSinceGroundedChanged = 0;

		if ( !wasOnGround && isOnGround && Global.EnableFallDamage && !IsNoclipping )
		{
			var minimumVelocity = MinimumFallVelocity;
			var vel = MathF.Abs( _previousVelocity.z );

			if ( vel > MinimumFallSoundVelocity )
			{
				PlayFallSound();
			}
			if ( vel > minimumVelocity )
			{
				var velPastAmount = vel - minimumVelocity;

				TimeUntilAccelerationRecovered = 1f;
				AccelerationAddedScale = 0f;

				using ( Rpc.FilterInclude( Connection.Host ) )
				{
					TakeFallDamage( velPastAmount * FallDamageScale );
				}
			}
		}
	}

	[Property, Group( "Effects" )] public SoundEvent LandSound { get; set; }

	[Rpc.Broadcast]
	private void PlayFallSound()
	{
		var snd = Sound.Play( LandSound, WorldPosition );
		snd.SpacialBlend = IsViewer ? 0 : snd.SpacialBlend;
	}

	[Rpc.Broadcast]
	private void TakeFallDamage( float damage )
	{
		GameObject.TakeDamage( new DamageInfo( this, damage, null, WorldPosition, Flags: DamageFlags.FallDamage ) );
	}

	private void CheckLadder()
	{
		var cc = CharacterController;
		var wishvel = new Vector3( WishMove.x.Clamp( -1f, 1f ), WishMove.y.Clamp( -1f, 1f ), 0 );
		wishvel *= EyeAngles.WithPitch( 0 ).ToRotation();
		wishvel = wishvel.Normal;

		if ( _isTouchingLadder )
		{
			if ( Input.Pressed( "jump" ) )
			{
				cc.Velocity = _ladderNormal * 100.0f;
				_isTouchingLadder = false;
				return;

			}
			else if ( cc.GroundObject != null && _ladderNormal.Dot( wishvel ) > 0 )
			{
				_isTouchingLadder = false;
				return;
			}
		}

		const float ladderDistance = 1.0f;
		var start = WorldPosition;
		Vector3 end = start + (_isTouchingLadder ? (_ladderNormal * -1.0f) : wishvel) * ladderDistance;

		var pm = Scene.Trace.Ray( start, end )
					.Size( cc.BoundingBox.Mins, cc.BoundingBox.Maxs )
					.WithTag( "ladder" )
					.HitTriggers()
					.IgnoreGameObjectHierarchy( GameObject )
					.Run();

		_isTouchingLadder = false;

		if ( pm.Hit )
		{
			_isTouchingLadder = true;
			_ladderNormal = pm.Normal;
		}
	}

	private void LadderMove()
	{
		var cc = CharacterController;
		cc.IsOnGround = false;

		var velocity = WishVelocity;
		float normalDot = velocity.Dot( _ladderNormal );
		var cross = _ladderNormal * normalDot;
		cc.Velocity = (velocity - cross) + (-normalDot * _ladderNormal.Cross( Vector3.Up.Cross( _ladderNormal ).Normal ));
		cc.Move();
	}

	private void BuildWishInput()
	{
		WishMove = 0f;

		if ( IsFrozen )
			return;

		WishMove += Input.AnalogMove;
	}

	private void BuildWishVelocity()
	{
		WishVelocity = 0f;

		var rot = EyeAngles.WithPitch( 0f ).ToRotation();

		if ( WishMove.Length > 1f )
			WishMove = WishMove.Normal;

		var wishDirection = WishMove * rot;
		wishDirection = wishDirection.WithZ( 0 );
		WishVelocity = wishDirection * GetWishSpeed();
	}

	/// <summary>
	/// Get the current friction.
	/// </summary>
	/// <returns></returns>
	// TODO: expose to global
	private float GetFriction()
	{
		if ( !CharacterController.IsOnGround ) return 0.1f;
		if ( IsSlowWalking ) return Global.SlowWalkFriction;
		if ( IsCrouching ) return Global.CrouchingFriction;
		if ( IsSprinting ) return Global.SprintingFriction;
		return Global.WalkFriction;
	}

	private float GetAcceleration()
	{
		if ( !CharacterController.IsOnGround ) return Global.AirAcceleration;
		else if ( IsSlowWalking ) return Global.SlowWalkAcceleration;
		else if ( IsCrouching ) return Global.CrouchingAcceleration;
		else if ( IsSprinting ) return Global.SprintingAcceleration;

		return Global.BaseAcceleration;
	}

	float GetEyeHeightOffset()
	{
		if ( IsCrouching ) return -16f;
		if ( HealthComponent.State == LifeState.Dead ) return -48f;
		return 0f;
	}

	private float GetSpeedPenalty()
	{
		var wpn = CurrentEquipment;
		if ( !wpn.IsValid() ) return 0;
		return wpn.SpeedPenalty;
	}

	private float GetWishSpeed()
	{
		if ( IsSlowWalking ) return Global.SlowWalkSpeed;
		if ( IsCrouching ) return Global.CrouchingSpeed;
		if ( IsSprinting ) return Global.SprintingSpeed - (GetSpeedPenalty() * 0.5f);
		return Global.WalkSpeed - GetSpeedPenalty();
	}

	private void DebugUpdate()
	{
		DebugText.Update();
		DebugText.Write( $"Player", Color.White, 20 );
		DebugText.Write( $"Velocity: {CharacterController.Velocity}" );
		DebugText.Write( $"Speed: {CharacterController.Velocity.Length}" );
		DebugText.Spacer();
		DebugText.Write( $"Weapon Info", Color.White, 20 );
		DebugText.Write( $"Spread: {Spread}" );
	}
}
