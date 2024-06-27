namespace Facepunch;

public sealed partial class PlayerPawn : Pawn
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
	/// A reference to the animation helper (normally on the Body GameObject)
	/// </summary>
	[Property] public AnimationHelper AnimationHelper { get; set; }

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
	/// A reference to the View Model's camera. This will be disabled by the View Model.
	/// </summary>
	[Property] public CameraComponent ViewModelCamera { get; set; }

	/// <summary>
	/// Handles the player's outfit
	/// </summary>
	[Property] public PlayerOutfitter Outfitter { get; set; }

	/// <summary>
	/// Get a quick reference to the real Camera GameObject.
	/// </summary>
	public GameObject CameraGameObject => CameraController.Camera.GameObject;

	/// <summary>
	/// Finds the first <see cref="SkinnedModelRenderer"/> on <see cref="Body"/>
	/// </summary>
	public SkinnedModelRenderer BodyRenderer => Body.Components.Get<SkinnedModelRenderer>();

	/// <summary>
	/// Pawn
	/// </summary>
	// CameraComponent Pawn.Camera => CameraController.Camera;

	public override CameraComponent Camera => CameraController.Camera;

	protected override void OnStart()
	{
		if ( !IsProxy && !PlayerState.IsBot )
		{
			// Set this as our local player and possess it.
			PlayerState.Possess( this );
		}

		// TODO: expose these parameters please
		TagBinder.BindTag( "no_shooting", () => IsSprinting || TimeSinceSprintChanged < 0.25f || TimeSinceWeaponDeployed < 0.66f );
		TagBinder.BindTag( "no_aiming", () => IsSprinting || TimeSinceSprintChanged < 0.25f || TimeSinceGroundedChanged < 0.25f );

		GameObject.Name = $"Player ({DisplayName})";
	}

	public SceneTraceResult CachedEyeTrace { get; private set; }

	protected override void OnUpdate()
	{
		OnUpdateMovement();

		CrouchAmount = CrouchAmount.LerpTo( IsCrouching ? 1 : 0, Time.Delta * CrouchLerpSpeed() );
		_smoothEyeHeight = _smoothEyeHeight.LerpTo( _eyeHeightOffset * (IsCrouching ? CrouchAmount : 1), Time.Delta * 10f );
		CharacterController.Height = Height + _smoothEyeHeight;

		if ( IsLocallyControlled )
		{
			DebugUpdate();
		}
	}

	protected override void OnFixedUpdate()
	{
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
				.WithoutTags( "invis", "ragdoll", "movement" )
				.UseHitboxes()
				.Run();
		}

		if ( HealthComponent.State != LifeState.Alive )
		{
			return;
		}

		if ( Networking.IsHost && PlayerState.IsBot )
		{
			if ( BotFollowHostInput )
			{
				BuildWishInput();
				BuildWishVelocity();
			}

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

		UpdateUse();
		BuildWishInput();
		BuildWishVelocity();
		BuildInput();

		UpdateRecoilAndSpread();
		ApplyAcceleration();
		ApplyMovement();
	}
}
