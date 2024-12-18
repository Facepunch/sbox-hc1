using Sandbox.Events;

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
	//[Property] public BoxCollider PlayerBoxCollider { get; set; }

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
			if (!IsProxy) _smoothEyeAngles = value;
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
	/// Are we on the ground?
	/// </summary>
	public bool IsGrounded { get; set; }

	/// <summary>
	/// How quick do we wish to go (normalized)
	/// </summary>
	public Vector3 WishMove { get; private set; }

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
	private Vector3 _jumpPosition;
	
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
	}

	private void OnUpdateMovement()
	{
		var cc = PlayerController;
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

		{

			if ( Body.IsValid() )
			{
				Body.WorldRotation = Rotation.FromYaw( EyeAngles.yaw );
			}

			if ( AnimationHelper.IsValid() )
			{
				AnimationHelper.WithVelocity( cc.Velocity );
				AnimationHelper.WithWishVelocity( cc.WishVelocity );
				AnimationHelper.IsGrounded = IsGrounded;
				AnimationHelper.WithLook( EyeAngles.Forward, 1, 1, 1.0f );
				AnimationHelper.MoveStyle = AnimationHelper.MoveStyles.Run;
				AnimationHelper.DuckLevel = (MathF.Abs( _smoothEyeHeight ) / 32.0f);
				AnimationHelper.HoldType = CurrentHoldType;
				AnimationHelper.Handedness = CurrentEquipment.IsValid() ? CurrentEquipment.Handedness : AnimationHelper.Hand.Both;
				AnimationHelper.AimBodyWeight = 0.1f;

				var skidding = 0.0f;

				if ( cc.WishVelocity.IsNearlyZero( 0.1f ) ) skidding = cc.Velocity.Length.Remap( 0, 1000, 0, 1 );

				AnimationHelper.SkidAmount = skidding;
			}
		}

		AimDampening = 1.0f;
	}

    private bool WantsToSprint => Input.Down( "Run" ) && !IsSlowWalking && !HasEquipmentTag( "no_sprint" ) && WishMove.x > 0.2f;
	TimeSince TimeSinceSprintChanged { get; set; } = 100;

	private void OnSprintChanged( bool before, bool after )
	{
		TimeSinceSprintChanged = 0;
	}

	public bool HasEquipmentTag( string tag )
	{
		return CurrentEquipment.IsValid() && CurrentEquipment.Tags.Has( tag );
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

		if ( PlayerController.IsOnGround && !IsFrozen )
		{
			var bhop = Global.BunnyHopping;
			if ( bhop ? Input.Down( "Jump" ) : Input.Pressed( "Jump" ) )
			{
				PlayerController.Jump( Vector3.Up * Global.JumpPower );
				BroadcastPlayerJumped();
			}
		}
	}

	public SceneTraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f, float liftHead = 0.0f )
	{
		return PlayerController.TraceBody( start, end );
	}

	/// <summary>
	/// A network message that lets other users that we've triggered a jump.
	/// </summary>
	[Rpc.Broadcast]
	public void BroadcastPlayerJumped()
	{
		AnimationHelper?.TriggerJump();
		OnJump?.Invoke();
	}

	public TimeSince TimeSinceGroundedChanged { get; private set; }

	private void GroundedChanged( bool wasOnGround, bool isOnGround )
	{
		if ( !IsLocallyControlled )
			return;

		TimeSinceGroundedChanged = 0;

		if ( wasOnGround && !isOnGround )
		{
			_jumpPosition = WorldPosition;
		}

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

	private void BuildWishInput()
	{
		WishMove = 0f;

		if ( IsFrozen )
			return;

		WishMove += Input.AnalogMove;
	}

	private void BuildWishVelocity()
	{
		var cc = PlayerController;
		cc.WishVelocity = 0f;

		var rot = EyeAngles.WithPitch( 0f ).ToRotation();
		
		if ( WishMove.Length > 1f )
			WishMove = WishMove.Normal;

		cc.WishVelocity = cc.Mode.UpdateMove( rot, WishMove );
	}

	float GetEyeHeightOffset()
	{
		if ( IsCrouching ) return -16f;
		if ( HealthComponent.State == LifeState.Dead ) return -48f;
		return 0f;
	}

	float GetSpeedPenalty()
	{
		var wpn = CurrentEquipment;
		if ( !wpn.IsValid() ) return 0;
		return wpn.SpeedPenalty;
	}

	public float GetWishSpeed()
	{
		if ( IsSlowWalking ) return Global.SlowWalkSpeed;
		if ( IsCrouching ) return Global.CrouchingSpeed;
		if ( IsSprinting ) return Global.SprintingSpeed - ( GetSpeedPenalty() );
		return Global.WalkSpeed - GetSpeedPenalty();
	}

	/// <summary>
	/// Get the current friction.
	/// </summary>
	/// <returns></returns>
	public float GetFriction()
	{
		if ( !PlayerController.IsOnGround ) return 0.1f;
		if ( IsSlowWalking ) return Global.SlowWalkFriction;
		if ( IsCrouching ) return Global.CrouchingFriction;
		if ( IsSprinting ) return Global.SprintingFriction;
		return Global.WalkFriction;
	}

	private void DebugUpdate()
	{
		DebugText.Update();
		DebugText.Write( $"Player", Color.White, 20 );
		DebugText.Write( $"Velocity: {PlayerController.Velocity}" );
		DebugText.Write( $"Speed: {PlayerController.Velocity.Length}" );
		DebugText.Write( $"Speed Penalty: {GetSpeedPenalty()}" );
		DebugText.Spacer();
		DebugText.Write( $"Weapon Info", Color.White, 20 );
		DebugText.Write( $"Spread: {Spread}" );
	}
}
