using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Facepunch;

public partial class PlayerController : Component, IPawn, IRespawnable
{
	/// <summary>
	/// Sync the player's steamid
	/// </summary>
	[Sync] public ulong SteamId { get; set; }

	/// <summary>
	/// The player's body
	/// </summary>
	[Property] public PlayerBody Body { get; set; }

	/// <summary>
	/// A reference to the player's head (the GameObject)
	/// </summary>
	[Property] public GameObject Head { get; set; }

	/// <summary>
	/// A reference to the animation helper (normally on the Body GameObject)
	/// </summary>
	[Property] public AnimationHelper AnimationHelper { get; set; }

	/// <summary>
	/// The current gravity. Make this a gamerule thing later?
	/// </summary>
	[Property, Group( "Config" )] public Vector3 Gravity { get; set; } = new Vector3( 0, 0, 800 );

	/// <summary>
	/// The current character controller for this player.
	/// </summary>
	[RequireComponent] public CharacterController CharacterController { get; set; }

	/// <summary>
	/// The current camera controller for this player.
	/// </summary>
	[RequireComponent] public CameraController CameraController { get; set; }

	/// <summary>
	/// A reference to the View Model's camera. This will be disabled by the View Model.
	/// </summary>
	[Property] public CameraComponent ViewModelCamera { get; set; }

	/// <summary>
	/// A <see cref="GameObject"/> that will hold our ViewModel.
	/// </summary>
	[Property] public GameObject ViewModelGameObject { get; set; }

	/// <summary>
	/// Get a quick reference to the real Camera GameObject.
	/// </summary>
	public GameObject CameraGameObject => CameraController.Camera.GameObject;

	/// <summary>
	/// Finds the first <see cref="SkinnedModelRenderer"/> on <see cref="Body"/>
	/// </summary>
	public SkinnedModelRenderer BodyRenderer => Body.Components.Get<SkinnedModelRenderer>();

	/// <summary>
	/// An accessor to get the camera controller's aim ray.
	/// </summary>
	public Ray AimRay => CameraController.AimRay;

	// TODO: move this into something that isn't on the player, this should be on an animator being fed info like the weapon
	[Sync] AnimationHelper.HoldTypes CurrentHoldType { get; set; } = AnimationHelper.HoldTypes.None;

	/// <summary>
	/// GameObject with the player's HUD. We'll only turn it on if we're the local connection.
	/// </summary>
	[Property] public GameObject HUDGameObject { get; set; }

	/// <summary>
	/// Called when the player jumps.
	/// </summary>
	[Property] public Action OnJump { get; set; }

	/// <summary>
	/// How quickly does the player move by default?
	/// </summary>
	[Property, Group( "Config" )] public float WalkSpeed { get; set; } = 125f;

	/// <summary>
	/// How powerful is the player's jump?
	/// </summary>
	[Property, Group( "Config" )] public float JumpPower { get; set; } = 320f;

	/// <summary>
	/// How much friction does the player have?
	/// </summary>
	[Property, Group( "Config" )] public float BaseFriction { get; set; } = 4.0f;

	/// <summary>
	/// The player's box collider, so people can jump on other people.
	/// </summary>
	[Property] public BoxCollider PlayerBoxCollider { get; set; }

    /// <summary>
    /// A shorthand accessor to say if we're controlling this player.
    /// </summary>
    public bool IsLocallyControlled
	{
		get
		{
			return ( this as IPawn ).IsPossessed && !IsProxy;
		}
	}

	/// <summary>
	/// If true, we're not allowed to move.
	/// </summary>
	public bool IsFrozen => GameMode.Instance.State is GameState.PreRound;

	/// <summary>
	/// If true, we can automatically respawn.
	/// </summary>
	public bool CanRespawn => GameMode.Instance.State is GameState.PreGame;

	private Weapon currentWeapon;
	/// <summary>
	/// What weapon are we using?
	/// </summary>
	[Property, JsonIgnore, ReadOnly] public Weapon CurrentWeapon
	{
		get => currentWeapon;
		set
		{
			var prev = currentWeapon;

			currentWeapon = value;
			WeaponChanged( prev, currentWeapon );
		}
	}

	private void ClearViewModel( Weapon weapon = null )
	{
		if ( weapon.IsValid() )
		{
			weapon?.ClearViewModel( this );
		}
	}

	private void CreateViewModel( Weapon weapon = null )
	{
		if ( weapon.IsValid() )
		{
			weapon.CreateViewModel( this );
		}
	}

	public void WeaponChanged( Weapon oldWeapon, Weapon newWeapon )
	{
		if ( oldWeapon.IsValid() )
		{
			// Set old weapon as inactive
			oldWeapon.GameObject.Enabled = false;

			ClearViewModel( oldWeapon );
		}

		if ( !newWeapon.IsValid() )
			return;

		// Set new weapon as active
		newWeapon.GameObject.Enabled = true;

		if ( newWeapon.IsValid() )
		{
			// Move the weapon to the hand
			Body.MoveWeapon( newWeapon );
		}

		if ( IsLocallyControlled )
		{
			ClearViewModel( oldWeapon );

			if ( newWeapon.IsValid() )
				CreateViewModel( newWeapon );
		}
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();

		// Turn off the HUD by default, we'll opt in
		HUDGameObject.Enabled = false;
	}

	// Properties used only in this component.
	Vector3 WishVelocity;
	[Sync] public Angles EyeAngles { get; set; }

	public bool IsGrounded { get; set; }

	protected float GetEyeHeightOffset()
	{
		if ( CurrentEyeHeightOverride is not null ) return CurrentEyeHeightOverride.Value;
		return 0f;
	}

	float SmoothEyeHeight = 0f;

	protected override void OnAwake()
	{
		baseAcceleration = CharacterController.Acceleration;
	}

	protected override void OnUpdate()
	{
		var cc = CharacterController;

		if ( CurrentWeapon.IsValid() )
		{
			CurrentHoldType = CurrentWeapon.GetHoldType();
		}
		else
		{
			CurrentHoldType = AnimationHelper.HoldTypes.None;
		}

		// Eye input
		if ( IsLocallyControlled && cc != null )
		{
			// TODO: Move this eye height stuff to the camera? Not sure.
			var eyeHeightOffset = GetEyeHeightOffset();
			SmoothEyeHeight = SmoothEyeHeight.LerpTo( eyeHeightOffset, Time.Delta * 10f );

			if ( PlayerBoxCollider.IsValid() )
			{
				// Bit shit, but it works
				PlayerBoxCollider.Center = new(0, 0, 32 + SmoothEyeHeight);
				PlayerBoxCollider.Scale = new(32, 32, 64 + SmoothEyeHeight);
			}

			EyeAngles += Input.AnalogLook;
			EyeAngles = EyeAngles.WithPitch( EyeAngles.pitch.Clamp( -90, 90 ) );

			CameraController.UpdateFromEyes( SmoothEyeHeight );
		}
		else
		{
			CameraController.SetActive( false );
		}

		float rotateDifference = 0;

		// rotate body to look angles
		if ( Body is not null )
		{
			var targetAngle = new Angles( 0, EyeAngles.yaw, 0 ).ToRotation();

			rotateDifference = Body.Transform.Rotation.Distance( targetAngle );

			if ( rotateDifference > 50.0f || ( cc != null && cc.Velocity.Length > 10.0f ) )
			{
				Body.Transform.Rotation = Rotation.Lerp( Body.Transform.Rotation, targetAngle, Time.Delta * 10.0f );
			}
		}

		var wasGrounded = IsGrounded;
		IsGrounded = cc.IsOnGround;

		if ( wasGrounded != IsGrounded )
		{
			GroundedChanged();
		}

		if ( AnimationHelper is not null && cc is not null )
		{
			AnimationHelper.WithVelocity( cc.Velocity );
			AnimationHelper.WithWishVelocity( WishVelocity );
			AnimationHelper.IsGrounded = IsGrounded;
			AnimationHelper.FootShuffle = rotateDifference;
			AnimationHelper.WithLook( EyeAngles.Forward, 1, 1, 1.0f );
			AnimationHelper.MoveStyle = HasTag( "sprint" ) ? AnimationHelper.MoveStyles.Run : AnimationHelper.MoveStyles.Walk;
			AnimationHelper.DuckLevel = HasTag( "crouch" ) ? 100 : 0;
			AnimationHelper.HoldType = CurrentHoldType;
			AnimationHelper.SkidAmount = HasTag( "slide" ) ? 1 : 0;
		}
	}

	private void GroundedChanged()
	{
		var nowOffGround = IsGrounded == false;
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

	/// <summary>
	/// Get the current friction.
	/// </summary>
	/// <returns></returns>
	private float GetFriction()
	{
		if ( !CharacterController.IsOnGround ) return 0.1f;
		if ( CurrentFrictionOverride is not null ) return CurrentFrictionOverride.Value;

		return BaseFriction;
	}

	private float baseAcceleration = 10;
	private void ApplyAccceleration()
	{
		if ( CurrentAccelerationOverride is not null )
		{
			CharacterController.Acceleration = CurrentAccelerationOverride.Value;
		}
		else
		{
			CharacterController.Acceleration = baseAcceleration;
		}
	}

	protected override void OnFixedUpdate()
	{
		var cc = CharacterController;
		if ( cc == null )
			return;

		if ( IsLocallyControlled )
		{
			BuildWishInput();
			OnUpdateMechanics();
			BuildWishVelocity();

			if ( cc.IsOnGround && !IsFrozen && Input.Down( "Jump" ) )
			{
				float flGroundFactor = 1.0f;

				cc.Punch( Vector3.Up * JumpPower * flGroundFactor );

				BroadcastPlayerJumped();
			}
		}
		else
		{
			ProxyUpdateMechanics();
		}

		ApplyAccceleration();

		if ( cc.IsOnGround )
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
			cc.Accelerate( WishVelocity );
		}
		else
		{
			cc.Velocity -= Gravity * Time.Delta * 0.5f;
			cc.Accelerate( WishVelocity.ClampLength( 50 ) );
		}

		cc.ApplyFriction( GetFriction() );
		cc.Move();

		if ( !cc.IsOnGround )
		{
			cc.Velocity -= Gravity * Time.Delta * 0.5f;
		}
		else
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
		}
	}

	protected float GetWishSpeed()
	{
		if ( CurrentSpeedOverride is not null ) return CurrentSpeedOverride.Value;

		// Default speed
		return WalkSpeed;
	}

	public Vector3 WishMove;

	public void BuildWishInput()
	{
		WishMove = 0;

		if ( !IsLocallyControlled || IsFrozen ) return;

		WishMove += Input.AnalogMove;
	}

	public void BuildWishVelocity()
	{
		WishVelocity = 0;
		
		var rot = EyeAngles.WithPitch( 0f ).ToRotation();

		var wishDirection = WishMove.Normal * rot;
		wishDirection = wishDirection.WithZ( 0 );

		WishVelocity = wishDirection * GetWishSpeed();
	}
}
