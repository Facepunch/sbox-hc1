namespace Facepunch;

/// <summary>
/// A weapon's viewmodel. It's responsibility is to listen to events from a weapon.
/// It should only exist on the client for the currently possessed pawn.
/// </summary>
public partial class ViewModel : Component, IWeapon
{
	/// <summary>
	/// A reference to the <see cref="Weapon"/> we want to listen to.
	/// </summary>
	public Weapon Weapon { get; set; }

	/// <summary>
	/// A reference to the viewmodel's arms.
	/// </summary>
	[Property] public SkinnedModelRenderer Arms { get; set; }

	/// <summary>
	/// Is this a throwable?
	/// </summary>
	[Property] public bool IsThrowable { get; set; }

	/// <summary>
	/// Looks up the tree to find the player controller.
	/// </summary>
	PlayerController PlayerController => Weapon.PlayerController;

	[Property, Group( "GameObjects" )] public GameObject Muzzle { get; set; }
	[Property, Group( "GameObjects" )] public GameObject EjectionPort { get; set; }

	[Property] public SkinnedModelRenderer ModelRenderer { get; set; }

	private float YawInertiaScale => 2f;
	private float PitchInertiaScale => 2f;
	private bool activateInertia = false;
	private float lastPitch;
	private float lastYaw;
	private float YawInertia;
	private float PitchInertia;

	/// <summary>
	/// The View Model camera 
	/// </summary>
	public CameraComponent ViewModelCamera { get; set; }

	IEnumerable<IViewModelOffset> Offsets => Weapon.Components.GetAll<IViewModelOffset>( FindMode.EverythingInSelfAndDescendants );

	/// <summary>
	/// Does this viewmodel have any offests for aiming?
	/// </summary>
	bool HasAimOffset => Offsets.Count() > 0;

	/// <summary>
	/// The ironsights parameter, which could be different based on if we have any aim offsets.
	/// </summary>
	int Ironsights
	{
		get => HasAimOffset ? 1 : 2;
	}

	protected override void OnStart()
	{
		if ( IsThrowable )
			ModelRenderer?.Set( "throwable_type", (int)ThrowableType );
		else
			ModelRenderer?.Set( "b_deploy", true );

		// Somehow?
		if ( PlayerController.IsValid() )
			PlayerController.OnJump += OnPlayerJumped;
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
		var camera = Weapon.PlayerController.CameraGameObject;

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
		var camera = Weapon.PlayerController.CameraGameObject;
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

		ModelRenderer?.Set( "aim_yaw_inertia", YawInertia * YawInertiaScale );
		ModelRenderer?.Set( "aim_pitch_inertia", PitchInertia * PitchInertiaScale );
	}

	private Vector3 lerpedWishLook;

	private Vector3 localPosition;
	private Rotation localRotation;

	private Vector3 lerpedLocalPosition;
	private Rotation lerpedlocalRotation;

	protected void ApplyVelocity()
	{
		var moveVel = PlayerController.CharacterController.Velocity;
		var moveLen = moveVel.Length;

		var wishLook = PlayerController.WishMove.Normal * 1f;
		if ( Weapon?.Tags.Has( "aiming" ) ?? false ) wishLook = 0;

		if ( PlayerController.IsSlowWalking || PlayerController.IsCrouching ) moveLen *= 0.2f;

		lerpedWishLook = lerpedWishLook.LerpTo( wishLook, Time.Delta * 5.0f );

		localRotation *= Rotation.From( 0, -lerpedWishLook.y * 3f, 0 );
		localPosition += -lerpedWishLook;

		ModelRenderer?.Set( "move_groundspeed", moveLen );
	}

	private float FieldOfViewOffset = 0f;
	private float TargetFieldOfView = 90f;

	void ApplyAnimationParameters()
	{
		ModelRenderer.Set( "b_sprint", PlayerController.IsSprinting );
		ModelRenderer.Set( "b_grounded", PlayerController.IsGrounded );

		// Ironsights
		ModelRenderer.Set( "ironsights", Weapon.Tags.Has( "aiming" ) ? Ironsights : 0 );
		ModelRenderer.Set( "ironsights_fire_scale", Weapon.Tags.Has( "aiming" ) ? 0.2f : 0f );

		// Handedness
		ModelRenderer.Set( "b_twohanded", true );

		// Weapon state
		ModelRenderer.Set( "b_empty", !Weapon.Components.Get<AmmoComponent>( FindMode.EnabledInSelfAndDescendants )?.HasAmmo ?? false );
	}
	
	public enum ThrowableTypeEnum
	{
		HEGrenade,
		SmokeGrenade,
		StunGrenade,
		Molotov
	}

	[Property] public ThrowableTypeEnum ThrowableType { get; set; }

	private void ApplyThrowableAnimations()
	{
		var throwFn = Weapon.Components.Get<ThrowWeaponComponent>( FindMode.EnabledInSelfAndDescendants );

		ModelRenderer.Set( "b_idle", throwFn.ThrowState == ThrowWeaponComponent.State.Idle );
		ModelRenderer.Set( "b_pull", throwFn.ThrowState == ThrowWeaponComponent.State.Cook );
		ModelRenderer.Set( "b_throw", throwFn.ThrowState == ThrowWeaponComponent.State.Throwing );
	}

	protected override void OnUpdate()
	{
		// Reset every frame
		localRotation = Rotation.Identity;
		localPosition = Vector3.Zero;

		if ( !PlayerController.IsValid() || !PlayerController.CharacterController.IsValid() )
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
		ViewModelCamera.FieldOfView = TargetFieldOfView;

		lerpedlocalRotation = Rotation.Lerp( lerpedlocalRotation, localRotation, Time.Delta * 5f );
		lerpedLocalPosition = lerpedLocalPosition.LerpTo( localPosition, Time.Delta * 7f );

		Transform.LocalRotation = lerpedlocalRotation;
		Transform.LocalPosition = lerpedLocalPosition;
	}
}
