using Sandbox.Events;

namespace Facepunch;

public enum CameraMode
{
	FirstPerson,
	ThirdPerson
}

public interface ICameraSetup : ISceneEvent<ICameraSetup>
{
	// Effects before viewmodel
	public void PreSetup( CameraComponent cc ) { }

	// Place viewmodel
	public void Setup( CameraComponent cc ) { }

	// Effects including viewmodel
	public void PostSetup( CameraComponent cc ) { }
}

public sealed class CameraController : PawnCameraController, IGameEventHandler<DamageTakenEvent>
{
	[Property] public PlayerPawn Player { get; set; }

	[Property, Group( "Config" )] public bool ShouldViewBob { get; set; } = true;
	[Property, Group( "Config" )] public float RespawnProtectionSaturation { get; set; } = 0.25f;
	
	[Property] public float ThirdPersonDistance { get; set; } = 64f;
	[Property] public float AimFovOffset { get; set; } = -5f;

	private CameraMode _mode;
	public CameraMode Mode
	{
		get => _mode;
		set
		{
			if ( _mode == value )
				return;

			_mode = value;
			OnModeChanged();
		}
	}

	public float MaxBoomLength { get; set; }

	private float FieldOfViewOffset = 0f;
	private float TargetFieldOfView = 90f;

	/// <summary>
	/// Constructs a ray using the camera's GameObject
	/// </summary>
	public Ray AimRay
	{
		get
		{
			if ( Camera.IsValid() )
			{
				return new( Camera.WorldPosition + Camera.WorldRotation.Forward, Camera.WorldRotation.Forward );
			}

			return new( WorldPosition + Vector3.Up * 64f, Player.EyeAngles.ToRotation().Forward );
		}
	}

	public void AddFieldOfViewOffset( float degrees )
	{
		FieldOfViewOffset -= degrees;
	}

	private void UpdateRotation()
	{
		Boom.WorldRotation = Player.EyeAngles.ToRotation();
	}

	public override void SetActive( bool isActive )
	{
		base.SetActive( isActive );
		OnModeChanged();
		Boom.WorldRotation = Player.EyeAngles.ToRotation();
	}

	protected override void OnPreRender()
	{
		if ( !Camera.IsValid() )
			return;

		ICameraSetup.Post( x => x.PreSetup( Camera ) );
		ICameraSetup.Post( x => x.Setup( Camera ) );
		ICameraSetup.Post( x => x.PostSetup( Camera ) );
	}

	/// <summary>
	/// Updates the camera's position, from player code
	/// </summary>
	/// <param name="eyeHeight"></param>
	internal void UpdateFromEyes( float eyeHeight )
	{
		if ( !Camera.IsValid() )
			return;

		// All transform effects are additive to camera local position, so we need to reset it before anything is applied
		Camera.LocalPosition = Vector3.Zero;
		Camera.LocalRotation = Rotation.Identity;

		if ( Mode == CameraMode.ThirdPerson && !Player.IsLocallyControlled )
		{
			// orbit cam: spectating only
			var angles = Boom.WorldRotation.Angles();
			angles += Input.AnalogLook;
			Boom.WorldRotation = angles.WithPitch( angles.pitch.Clamp( -90, 90 ) ).ToRotation();
		}
		else
		{
			UpdateRotation();
		}

		if ( MaxBoomLength > 0 )
		{
			var traceStart = Boom.WorldPosition;
			var traceEnd = Boom.WorldRotation.Backward * MaxBoomLength;

			// Right amount for third person
			traceEnd += Boom.WorldRotation.Right * 25f;

			var tr = Scene.Trace.Ray( traceStart, traceStart + traceEnd )
				.IgnoreGameObjectHierarchy( GameObject.Root )
				.WithoutTags( "trigger", "player", "ragdoll" )
				.Run();

			Camera.WorldPosition = tr.EndPosition;
		}

		if ( ShouldViewBob )
		{
			ViewBob();
		}

		Update( eyeHeight );
	}

	float walkBob = 0;
	private float LerpBobSpeed = 0;

	[DeveloperCommand( "Toggle Third Person", "Player" )]
	public static void ToggleThirdPerson()
	{
		var pl = Client.Local.PlayerPawn;
		pl.CameraController.Mode = pl.CameraController.Mode == CameraMode.FirstPerson ? CameraMode.ThirdPerson : CameraMode.FirstPerson;
	}

	/// <summary>
	/// Bob the view!
	/// This could be better, but it doesn't matter really.
	/// </summary>
	void ViewBob()
	{
		if ( Mode != CameraMode.FirstPerson )
			return;

		var bobSpeed = Player.CharacterController.Velocity.Length.LerpInverse( 0, 300 );
		if ( !Player.IsGrounded ) bobSpeed *= 0.1f;
		if ( !Player.IsSprinting ) bobSpeed *= 0.3f;

		LerpBobSpeed = LerpBobSpeed.LerpTo( bobSpeed, Time.Delta * 10f );

		walkBob += Time.Delta * 10.0f * LerpBobSpeed;
		var yaw = MathF.Sin( walkBob ) * 0.5f;
		var pitch = MathF.Cos( -walkBob * 2f ) * 0.5f;

		Boom.LocalRotation *= Rotation.FromYaw( -yaw * LerpBobSpeed );
		Boom.LocalRotation *= Rotation.FromPitch( -pitch * LerpBobSpeed * 0.5f );
	}

	private void ApplyScope()
	{
		if ( !Player.IsValid() )
			return;
		
		if ( !Player.CurrentEquipment.IsValid() )
			return;

		if ( Player.CurrentEquipment.GetComponentInChildren<ScopeWeaponComponent>() is { } scope )
		{
			var fov = scope.GetFOV();
			FieldOfViewOffset -= fov;
		}
	}

	bool fetchedInitial = false;
	float defaultSaturation = 1f;

	private void Update( float eyeHeight )
	{
		var baseFov = GameSettingsSystem.Current.FieldOfView;
		FieldOfViewOffset = 0;

		if ( !Player.IsValid() )
			return;

		if ( Player.CurrentEquipment.IsValid() )
		{ 
			if ( Player.CurrentEquipment.EquipmentFlags.HasFlag( EquipmentFlags.Aiming ) )
			{
				FieldOfViewOffset += AimFovOffset;
			}
		}

		// deathcam, "zoom" at target.
		if ( Player.HealthComponent.State == LifeState.Dead )
		{
			FieldOfViewOffset += AimFovOffset; 
		}

		if ( ColorAdjustments.IsValid() )
		{
			if ( !fetchedInitial )
			{
				defaultSaturation = ColorAdjustments.Saturation;
				fetchedInitial = true;
			}

			ColorAdjustments.Saturation = Player.HealthComponent.IsGodMode
				? RespawnProtectionSaturation
				: ColorAdjustments.Saturation.MoveToLinear( defaultSaturation, 1f );
		}

		ApplyRecoil();
		ApplyScope();

		Boom.LocalPosition = Vector3.Zero.WithZ( eyeHeight );

		ApplyCameraEffects();
		ScreenShaker?.Apply( Camera );

		TargetFieldOfView = TargetFieldOfView.LerpTo( baseFov + FieldOfViewOffset, Time.Delta * 5f );
		Camera.FieldOfView = TargetFieldOfView;
	}
	RealTimeSince TimeSinceDamageTaken = 1;

	void IGameEventHandler<DamageTakenEvent>.OnGameEvent( DamageTakenEvent eventArgs )
	{
		TimeSinceDamageTaken = 0;
	}

	void ApplyCameraEffects()
	{
		var timeSinceDamage = TimeSinceDamageTaken.Relative;
		var shortDamageUi = timeSinceDamage.LerpInverse( 0.1f, 0.0f, true );
		ChromaticAberration.Scale = shortDamageUi * 1f;
		Pixelate.Scale = shortDamageUi * 0.2f;
	}

	void ApplyRecoil()
	{
		if ( !Player.IsValid() )
			return;

		if ( !Player.CurrentEquipment.IsValid() )
			return;

		if ( Player.CurrentEquipment.GetComponentInChildren<RecoilWeaponComponent>() is { } fn )
			Player.EyeAngles += fn.Current;
	}

	void OnModeChanged()
	{
		SetBoomLength( Mode == CameraMode.FirstPerson ? 0.0f : ThirdPersonDistance );

		if ( Camera.IsValid() )
		{
			// Update render exclude tags
			Camera.RenderExcludeTags.Set( "viewer", Mode == CameraMode.FirstPerson );
		}

		var firstPersonPOV = Mode == CameraMode.FirstPerson && IsActive;

		if ( Player.IsValid() && Player.Body.IsValid() )
		{
			Player.Body.SetFirstPersonView( firstPersonPOV );
		}

		if ( firstPersonPOV )
			Player.CreateViewModel( false );
		else
			Player.ClearViewModel();
	}

	private void SetBoomLength( float length )
	{
		MaxBoomLength = length;
	}
}
