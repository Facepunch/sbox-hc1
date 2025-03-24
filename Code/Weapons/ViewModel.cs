
namespace Facepunch;

/// <summary>
/// A weapon's viewmodel. It's responsibility is to listen to events from a weapon.
/// It should only exist on the client for the currently possessed pawn.
/// </summary>
public partial class ViewModel : WeaponModel, ICameraSetup
{
	/// <summary>
	/// A reference to the <see cref="Equipment"/> we want to listen to.
	/// </summary>
	public Equipment Equipment { get; set; }

	/// <summary>
	/// The resource
	/// </summary>
	public EquipmentResource Resource { get; set; }

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

	[Property, Range( 0, 1 ), Group( "Configuration" )] public float IronsightsFireScale { get; set; } = 0.2f;
	[Property, Group( "Configuration" )] public bool UseMovementInertia { get; set; } = true;

	[Property]
	public float ReloadSpeed { get; set; } = 1f;

	private float YawInertiaScale => 2f;
	private float PitchInertiaScale => 2f;
	private bool activateInertia = false;
	private float lastPitch;
	private float lastYaw;
	private float YawInertia;
	private float PitchInertia;

	IEnumerable<IViewModelOffset> Offsets => Equipment.GetComponentsInChildren<IViewModelOffset>();

	void ICameraSetup.Setup( CameraComponent cc )
	{
		if ( !Owner.IsValid() || !Owner.CharacterController.IsValid() )
			return;

		WorldPosition = cc.WorldPosition;
		WorldRotation = cc.WorldRotation;

		ApplyInertia();
		ApplyOffsets();

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

		var baseFov = GameSettingsSystem.Current.FieldOfView;

		TargetFieldOfView = TargetFieldOfView.LerpTo( baseFov + FieldOfViewOffset, Time.Delta * 10f );
		FieldOfViewOffset = 0;
	}

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

	void OnPlayerJumped()
	{
		ModelRenderer?.Set( "b_jump", true );
	}

	void ApplyAnimationTransform()
	{
		if ( !ModelRenderer.IsValid() ) return;
		if ( !ModelRenderer.Enabled ) return;
		if ( !Equipment.IsValid() ) return;
		if ( !Equipment.Owner.IsValid() ) return;
		if ( !ModelRenderer.SceneModel.IsValid() ) return;

		var bone = ModelRenderer.SceneModel.GetBoneLocalTransform( "camera" );
		var camera = Equipment.Owner.CameraGameObject;
		if ( !camera.IsValid() ) return;

		var scale = GameSettingsSystem.Current.ViewBob / 100f;

		camera.LocalPosition += bone.Position * scale;
		camera.LocalRotation *= bone.Rotation * scale;
	}

	void ApplyOffsets()
	{
		foreach ( var offset in Offsets )
		{
			// Log.Info( $"Offsetting by {offset.PositionOffset}" );
			WorldPosition += offset.PositionOffset;
			WorldRotation *= offset.AngleOffset.ToRotation();
		}
	}

	void ApplyInertia()
	{
		var camera = Equipment.Owner.CameraGameObject;
		var inRot = camera.WorldRotation;

		// Need to fetch data from the camera for the first frame
		if ( !activateInertia )
		{
			lastPitch = inRot.Pitch();
			lastYaw = inRot.Yaw();
			YawInertia = 0;
			PitchInertia = 0;
			activateInertia = true;
		}

		var newPitch = camera.WorldRotation.Pitch();
		var newYaw = camera.WorldRotation.Yaw();

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
		if ( !Equipment.IsValid() )
			return;

		var moveVel = Owner.CharacterController.Velocity;
		var moveLen = moveVel.Length;

		var wishMove = Owner.WishMove.Normal * 1f;
		if ( Equipment.EquipmentFlags.HasFlag( EquipmentFlags.Aiming ) ) wishMove = 0;

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

		var aiming = Equipment.EquipmentFlags.HasFlag( EquipmentFlags.Aiming );
		// Ironsights
		ModelRenderer.Set( "ironsights", aiming ? 1 : 0 );
		ModelRenderer.Set( "ironsights_fire_scale", aiming ? IronsightsFireScale : 0f );

		ModelRenderer.Set( "speed_ironsights", 2 );

		ModelRenderer.Set( "reload_speed", ReloadSpeed );

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
		if ( !Equipment.IsValid() )
			return;
		
		var throwFn = Equipment.GetComponentInChildren<ThrowWeaponComponent>();
		
		if ( !throwFn.IsValid() )
			return;

		ModelRenderer.Set( "b_idle", throwFn.ThrowState == ThrowWeaponComponent.State.Idle );
		ModelRenderer.Set( "b_pull", throwFn.ThrowState == ThrowWeaponComponent.State.Cook );
		ModelRenderer.Set( "b_throw", throwFn.ThrowState == ThrowWeaponComponent.State.Throwing );
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
