
namespace Facepunch;

/// <summary>
/// A weapon's viewmodel. It's responsibility is to listen to events from a weapon.
/// It should only exist on the client for the currently possessed pawn.
/// </summary>
public partial class ViewModel : Component, IEquipment
{
	/// <summary>
	/// A reference to the <see cref="Equipment"/> we want to listen to.
	/// </summary>
	public Equipment Equipment { get; set; }

	/// <summary>
	/// A reference to the viewmodel's arms.
	/// </summary>
	[Property, Group( "Components" )] public SkinnedModelRenderer Arms { get; set; }

	/// <summary>
	/// Is this a throwable?
	/// </summary>
	[Property, Group( "Configuration" )] public bool IsThrowable { get; set; }

	/// <summary>
	/// Looks up the tree to find the player controller.
	/// </summary>
	PlayerPawn Owner => Equipment.IsValid() ? Equipment.Owner : null;

	[Property, Group( "GameObjects" )] public GameObject Muzzle { get; set; }
	[Property, Group( "GameObjects" )] public GameObject EjectionPort { get; set; }

	[Property, Group( "Components" )] public SkinnedModelRenderer ModelRenderer { get; set; }
	[Property, Range( 0, 1 ), Group( "Configuration" )] public float IronsightsFireScale { get; set; } = 0.2f;
	[Property, Group( "Configuration" )] public bool UseMovementInertia { get; set; } = true;

	private float YawInertiaScale => 2f;
	private float PitchInertiaScale => 2f;
	private bool activateInertia = false;
	private float lastPitch;
	private float lastYaw;
	private float YawInertia;
	private float PitchInertia;

	IEnumerable<IViewModelOffset> Offsets => Equipment.GetComponentsInChildren<IViewModelOffset>();

	protected override void OnAwake()
	{
		ModelRenderer?.Set( "b_deploy_skip", true );
	}

	protected override void OnStart()
	{
		if ( IsThrowable )
			ModelRenderer?.Set( "throwable_type", (int)ThrowableType );

		// Somehow?
		if ( Owner.IsValid() )
			Owner.OnJump += OnPlayerJumped;

		// Somehow this can happen?
		if ( !Equipment.IsValid() )
			return;

		if ( Equipment.GetComponentInChildren<ShootWeaponComponent>() is { } shoot )
		{
			OnFireMode( shoot.CurrentFireMode );
		}
	}

	public void DisableArms()
	{
		Arms.Enabled = false;
	}

	void OnPlayerJumped()
	{
		ModelRenderer?.Set( "b_jump", true );
	}

	void ApplyAnimationTransform()
	{
		if ( !ModelRenderer.IsValid() ) return;
		if ( !ModelRenderer.Enabled ) return;

		var bone = ModelRenderer.SceneModel.GetBoneLocalTransform( "camera" );
		var camera = Equipment.Owner.CameraGameObject;

		var scale = GameSettingsSystem.Current.ViewBob / 100f;

		camera.Transform.LocalPosition += bone.Position * scale;
		camera.Transform.LocalRotation *= bone.Rotation * scale;
	}

	void ApplyOffsets()
	{
		foreach ( var offset in Offsets )
		{
			// Log.Info( $"Offsetting by {offset.PositionOffset}" );
			localPosition += offset.PositionOffset;
			localRotation *= offset.AngleOffset.ToRotation();
		}
	}

	void ApplyInertia()
	{
		var camera = Equipment.Owner.CameraGameObject;
		var inRot = camera.Transform.Rotation;

		// Need to fetch data from the camera for the first frame
		if ( !activateInertia )
		{
			lastPitch = inRot.Pitch();
			lastYaw = inRot.Yaw();
			YawInertia = 0;
			PitchInertia = 0;
			activateInertia = true;
		}

		var newPitch = camera.Transform.Rotation.Pitch();
		var newYaw = camera.Transform.Rotation.Yaw();

		PitchInertia = Angles.NormalizeAngle( newPitch - lastPitch );
		YawInertia = Angles.NormalizeAngle( lastYaw - newYaw );

		lastPitch = newPitch;
		lastYaw = newYaw;
	}

	private Vector3 lerpedWishMove;

	private Vector3 localPosition;
	private Rotation localRotation;

	private Vector3 lerpedLocalPosition;
	private Rotation lerpedlocalRotation;

	protected void ApplyVelocity()
	{
		var moveVel = Owner.CharacterController.Velocity;
		var moveLen = moveVel.Length;

		var wishMove = Owner.WishMove.Normal * 1f;
		if ( Equipment?.Tags.Has( "aiming" ) ?? false ) wishMove = 0;

		if ( Owner.IsSlowWalking || Owner.IsCrouching ) moveLen *= 0.5f;

		lerpedWishMove = lerpedWishMove.LerpTo( wishMove, Time.Delta * 7.0f );
		ModelRenderer?.Set( "move_bob", moveLen.Remap( 0, 300, 0, 1, true ) );

		if ( UseMovementInertia )
			YawInertia += lerpedWishMove.y * 10f;

		ModelRenderer?.Set( "aim_yaw_inertia", YawInertia * YawInertiaScale );
		ModelRenderer?.Set( "aim_pitch_inertia", PitchInertia * PitchInertiaScale );
	}

	private float FieldOfViewOffset = 0f;
	private float TargetFieldOfView = 90f;

	void ApplyAnimationParameters()
	{
		ModelRenderer.Set( "b_sprint", Owner.IsSprinting );
		ModelRenderer.Set( "b_grounded", Owner.IsGrounded );

		// Ironsights
		ModelRenderer.Set( "ironsights", Equipment.Tags.Has( "aiming" ) ? 1 : 0 );
		ModelRenderer.Set( "ironsights_fire_scale", Equipment.Tags.Has( "aiming" ) ? IronsightsFireScale : 0f );

		// Handedness
		ModelRenderer.Set( "b_twohanded", true );

		// Weapon state
		ModelRenderer.Set( "b_empty", !Equipment.GetComponentInChildren<AmmoComponent>()?.HasAmmo ?? false );
	}
	
	public enum ThrowableTypeEnum
	{
		HEGrenade,
		SmokeGrenade,
		StunGrenade,
		Molotov,
		Flashbang
	}

	[Property, ShowIf( nameof( IsThrowable ), true ), Group( "Configuration" )] public ThrowableTypeEnum ThrowableType { get; set; }

	/// <summary>
	/// Should we play deploy effects?
	/// </summary>
	public bool PlayDeployEffects
	{
		set
		{
			ModelRenderer?.Set( "b_deploy", value );
			ModelRenderer?.Set( "b_deploy_skip", !value );
		}
	}

	private void ApplyThrowableAnimations()
	{
		var throwFn = Equipment.GetComponentInChildren<ThrowWeaponComponent>();

		ModelRenderer.Set( "b_idle", throwFn.ThrowState == ThrowWeaponComponent.State.Idle );
		ModelRenderer.Set( "b_pull", throwFn.ThrowState == ThrowWeaponComponent.State.Cook );
		ModelRenderer.Set( "b_throw", throwFn.ThrowState == ThrowWeaponComponent.State.Throwing );
	}

	protected override void OnUpdate()
	{
		// Reset every frame
		localRotation = Rotation.Identity;
		localPosition = Vector3.Zero;

		if ( !Owner.IsValid() || !Owner.CharacterController.IsValid() )
			return;

		if ( IsThrowable )
		{
			ApplyThrowableAnimations();
		}
		else
		{
			ApplyAnimationParameters();
		}
		
		ApplyVelocity();
		ApplyAnimationTransform();
		ApplyInertia();
		ApplyOffsets();

		var baseFov = GameSettingsSystem.Current.FieldOfView;

		TargetFieldOfView = TargetFieldOfView.LerpTo( baseFov + FieldOfViewOffset, Time.Delta * 10f );
		FieldOfViewOffset = 0;

		lerpedlocalRotation = Rotation.Lerp( lerpedlocalRotation, localRotation, Time.Delta * 10f );
		lerpedLocalPosition = lerpedLocalPosition.LerpTo( localPosition, Time.Delta * 10f );

		Transform.LocalRotation = lerpedlocalRotation;
		Transform.LocalPosition = lerpedLocalPosition;
	}

	public void OnFireMode( FireMode currentFireMode )
	{
		var mode = currentFireMode switch
		{
			FireMode.Semi => 1,
			FireMode.Automatic => 3,
			FireMode.Burst => 2,
			_ => 0
		};

		ModelRenderer.Set( "firing_mode", mode );
	}
}
