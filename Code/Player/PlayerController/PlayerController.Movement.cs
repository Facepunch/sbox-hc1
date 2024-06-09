namespace Facepunch;

public partial class PlayerController
{
	/// <summary>
	/// Called when the player jumps.
	/// </summary>
	[Property] public Action OnJump { get; set; }

	/// <summary>
	/// The player's box collider, so people can jump on other people.
	/// </summary>
	[Property] public BoxCollider PlayerBoxCollider { get; set; }

	/// <summary>
	/// How tall are we?
	/// </summary>
	[Property, Group( "Config" )] public float Height { get; set; } = 64f;

	/// <summary>
	/// How quickly does the player move by default?
	/// </summary>
	[Property, Group( "Config" )] public float WalkSpeed { get; set; } = 125f;

	/// <summary>
	/// How much friction does the player have?
	/// </summary>
	[Property, Group( "Friction" )] public float BaseFriction { get; set; } = 4.0f;
	[Property, Group( "Friction" )] public float SlowWalkFriction { get; set; } = 4.0f;
	[Property, Group( "Friction" )] public float CrouchingFriction { get; set; } = 4.0f;

	/// <summary>
	/// Noclip movement speed
	/// </summary>
	[Property] public float NoclipSpeed { get; set; } = 1000f;

	/// <summary>
	/// Where are we looking?
	/// </summary>
	[Sync] public Angles EyeAngles { get; set; }

	/// <summary>
	/// Is the player crouching?
	/// </summary>
	[Sync] public bool IsCrouching { get; set; }

	/// <summary>
	/// Is the player slow walking?
	/// </summary>
	[Sync] public bool IsSlowWalking { get; set; }

	/// <summary>
	/// Is the player noclipping?
	/// </summary>
	[Sync] public bool IsNoclipping { get; set; }

	/// <summary>
	/// If true, we're not allowed to move.
	/// </summary>
	[HostSync] public bool IsFrozen { get; set; }

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
	private bool _isTouchingLadder;
	private Vector3 _ladderNormal;

	private void OnUpdateMovement()
	{
		var cc = CharacterController;
		CurrentHoldType = CurrentWeapon.IsValid() ? CurrentWeapon.GetHoldType() : AnimationHelper.HoldTypes.None;

		// Eye input
		if ( (this as IPawn).IsPossessed && cc.IsValid() )
		{
			var eyeHeightOffset = GetEyeHeightOffset();

			var target = eyeHeightOffset;
			var trace = TraceBBox( Transform.Position, Transform.Position, 0, 10f );
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

			_smoothEyeHeight = _smoothEyeHeight.LerpTo( eyeHeightOffset, Time.Delta * 10f );
			CharacterController.Height = Height + _smoothEyeHeight;

			if ( PlayerBoxCollider.IsValid() )
			{
				// Bit shit, but it works
				PlayerBoxCollider.Center = new( 0, 0, 32 + _smoothEyeHeight );
				PlayerBoxCollider.Scale = new( 32, 32, 64 + _smoothEyeHeight );
			}

			if ( IsLocallyControlled )
			{
				EyeAngles += Input.AnalogLook * AimDampening;
				EyeAngles = EyeAngles.WithPitch( EyeAngles.pitch.Clamp( -90, 90 ) );
			}

			CameraController.UpdateFromEyes( _smoothEyeHeight );
		}
		else
		{
			CameraController.SetActive( false );
		}

		var rotateDifference = 0f;

		if ( Body.IsValid() )
		{
			var targetAngle = new Angles( 0, EyeAngles.yaw, 0 ).ToRotation();

			rotateDifference = Body.Transform.Rotation.Distance( targetAngle );

			if ( rotateDifference > 50.0f || (cc != null && cc.Velocity.Length > 10.0f) )
			{
				Body.Transform.Rotation = Rotation.Lerp( Body.Transform.Rotation, targetAngle, Time.Delta * 10.0f );
			}
		}

		var wasGrounded = IsGrounded;
		IsGrounded = cc.IsOnGround;

		if ( IsGrounded != wasGrounded )
		{
			GroundedChanged( wasGrounded, IsGrounded );
		}

		if ( AnimationHelper.IsValid() )
		{
			AnimationHelper.WithVelocity( cc.Velocity );
			AnimationHelper.WithWishVelocity( WishVelocity );
			AnimationHelper.IsGrounded = IsGrounded;
			AnimationHelper.FootShuffle = rotateDifference;
			AnimationHelper.WithLook( EyeAngles.Forward, 1, 1, 1.0f );
			AnimationHelper.MoveStyle = AnimationHelper.MoveStyles.Run;
			AnimationHelper.DuckLevel = IsCrouching ? 100 : 0;
			AnimationHelper.HoldType = CurrentHoldType;
		}

		AimDampening = 1.0f;
	}

	private float GetMaxAcceleration()
	{
		var global = GetGlobal<PlayerGlobals>();
		if ( !IsGrounded ) return global.AirMaxAcceleration;
		return global.MaxAcceleration;
	}

	private void ApplyMovement()
	{
		var cc = CharacterController;

		CheckLadder();

		var gravity = GetGlobal<PlayerGlobals>().Gravity;

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
			cc.IsOnGround = false;
			cc.Velocity = WishMove.Normal * EyeAngles.ToRotation() * NoclipSpeed;
		}

		cc.ApplyFriction( GetFriction() );
		cc.Move();
	}

	private void BuildInput()
	{
		if ( InMenu )
			return;

		IsSlowWalking = Input.Down( "Run" );
		IsCrouching = Input.Down( "Duck" );
		IsUsing = Input.Down( "Use" );

		// Check if our current weapon has the planting tag and if so force us to crouch.
		var currentWeapon = CurrentWeapon;
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

		if ( CharacterController.IsOnGround && !IsFrozen && !InMenu )
		{
			var bhop = GetGlobal<PlayerGlobals>().BunnyHopping;
			if ( bhop ? Input.Down( "Jump" ) : Input.Pressed( "Jump" ) )
			{
				CharacterController.Punch( Vector3.Up * GetGlobal<PlayerGlobals>().JumpPower * 1f );
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
	[Broadcast]
	public void BroadcastPlayerJumped()
	{
		AnimationHelper?.TriggerJump();
		OnJump?.Invoke();
	}

	private void GroundedChanged( bool wasOnGround, bool isOnGround )
	{
		if ( wasOnGround && !isOnGround )
		{
			_jumpPosition = Transform.Position;
		}

		if ( !wasOnGround && isOnGround && GetGlobal<PlayerGlobals>().EnableFallDamage )
		{
			var vel = _previousVelocity.z;
			var jumpFromPos = _jumpPosition;
			var positionNow = Transform.Position;

			// jumped up somehow?
			if ( positionNow.z >= jumpFromPos.z ) return;

			var fallDamageScale = 125;
			var zDist = MathF.Abs( positionNow.z - jumpFromPos.z );

			var scale = zDist.LerpInverse( 0, 500f, true );
			if ( scale < 0.35f ) return;

			GameObject.TakeDamage( DamageEvent.From( this, fallDamageScale * scale, null, Transform.Position ) with { Tags = "fall_damage" } );
		}
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
		var start = Transform.Position;
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

		if ( IsFrozen || InMenu )
			return;

		WishMove += Input.AnalogMove;
	}

	private void BuildWishVelocity()
	{
		WishVelocity = 0f;

		var rot = EyeAngles.WithPitch( 0f ).ToRotation();

		var wishDirection = WishMove.Normal * rot;
		wishDirection = wishDirection.WithZ( 0 );

		WishVelocity = wishDirection * GetWishSpeed();
	}

	/// <summary>
	/// Get the current friction.
	/// </summary>
	/// <returns></returns>
	private float GetFriction()
	{
		if ( !IsGrounded ) return 0.1f;
		if ( IsSlowWalking ) return SlowWalkFriction;
		if ( IsCrouching ) return CrouchingFriction;
		return BaseFriction;
	}

	private void ApplyAcceleration()
	{
		var global = GetGlobal<PlayerGlobals>();

		if ( !IsGrounded ) CharacterController.Acceleration = global.AirAcceleration;
		else if ( IsSlowWalking ) CharacterController.Acceleration = global.SlowWalkAcceleration;
		else if ( IsCrouching ) CharacterController.Acceleration = global.CrouchingAcceleration;
		else
			CharacterController.Acceleration = global.BaseAcceleration;
	}

	float GetEyeHeightOffset()
	{
		if ( IsCrouching ) return -32f;
		if ( HealthComponent.State == LifeState.Dead ) return -48f;
		return 0f;
	}

	private float GetWalkSpeed()
	{
		var spd = WalkSpeed;
		var wpn = CurrentWeapon;
		if ( !wpn.IsValid() ) return spd;
		return spd - wpn.SpeedPenalty;
	}

	private float GetWishSpeed()
	{
		if ( IsSlowWalking ) return 100f;
		if ( IsCrouching ) return 100f;
		return GetWalkSpeed();
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
