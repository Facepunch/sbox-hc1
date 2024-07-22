using Facepunch.UI;
using Sandbox;
using Sandbox.Events;

namespace Facepunch;

public enum CameraMode
{
	FirstPerson,
	ThirdPerson
}

public sealed class CameraController : Component, IGameEventHandler<DamageTakenEvent>
{
	/// <summary>
	/// A reference to the camera component we're going to be doing stuff with.
	/// </summary>
	public CameraComponent Camera { get; set; }
	public AudioListener AudioListener { get; set; }
	public ColorAdjustments ColorAdjustments { get; set; }
	public ScreenShaker ScreenShaker { get; set; }
	public ChromaticAberration ChromaticAberration { get; set; }
	public Pixelate Pixelate { get; set; }

	[Property] public PlayerPawn Player { get; set; }

	/// <summary>
	/// The default player camera prefab.
	/// </summary>
	[Property] public GameObject DefaultPlayerCameraPrefab { get; set; }

	public GameObject PlayerCameraGameObject { get; set; }

	[Property, Group( "Config" )] public bool ShouldViewBob { get; set; } = true;
	[Property, Group( "Config" )] public float RespawnProtectionSaturation { get; set; } = 0.25f;
	
	[Property] public float ThirdPersonDistance { get; set; } = 128f;
	[Property] public float AimFovOffset { get; set; } = -5f;
	[Property] public GameObject Boom { get; set; }

	private CameraMode _mode;
	public CameraMode Mode
	{
		get => _mode;
		set
		{
			_mode = value;
			OnModeChanged();
		}
	}
	public float MaxBoomLength { get; set; }

	/// <summary>
	/// Constructs a ray using the camera's GameObject
	/// </summary>
	public Ray AimRay
	{
		get
		{
			if ( Camera.IsValid() )
			{
				return new( Camera.Transform.Position + Camera.Transform.Rotation.Forward, Camera.Transform.Rotation.Forward );
			}

			return new( Transform.Position + Vector3.Up * 64f, Player.EyeAngles.ToRotation().Forward );
		}
	}

	private float FieldOfViewOffset = 0f;
	private float TargetFieldOfView = 90f;

	public void AddFieldOfViewOffset( float degrees )
	{
		FieldOfViewOffset -= degrees;
	}

	private GameObject GetOrCreateCameraObject()
	{
		var component = Scene.GetAllComponents<PlayerCameraOverride>().FirstOrDefault();

		var config = new CloneConfig()
		{
			StartEnabled = true,
			Parent = Boom,
			Transform = new Transform()
		};

		if ( component.IsValid() )
			return component.Prefab.Clone( config );

		return DefaultPlayerCameraPrefab?.Clone( config );
	}

	public void SetActive( bool isActive )
	{
		if ( PlayerCameraGameObject.IsValid() )
			PlayerCameraGameObject.Destroy();

		if ( isActive )
		{
			PlayerCameraGameObject = GetOrCreateCameraObject();

			Camera = PlayerCameraGameObject.Components.GetOrCreate<CameraComponent>();
			Pixelate = PlayerCameraGameObject.Components.GetOrCreate<Pixelate>();
			ChromaticAberration = PlayerCameraGameObject.Components.GetOrCreate<ChromaticAberration>();
			AudioListener = PlayerCameraGameObject.Components.GetOrCreate<AudioListener>();
			ScreenShaker = PlayerCameraGameObject.Components.GetOrCreate<ScreenShaker>();

			// Optional
			ColorAdjustments = PlayerCameraGameObject.Components.Get<ColorAdjustments>();
		}
		else
		{
		}

		OnModeChanged();

		Boom.Transform.Rotation = Player.EyeAngles.ToRotation();
	}

	/// <summary>
	/// Updates the camera's position, from player code
	/// </summary>
	/// <param name="eyeHeight"></param>
	internal void UpdateFromEyes( float eyeHeight )
	{
		// All transform effects are additive to camera local position, so we need to reset it before anything is applied
		Camera.Transform.LocalPosition = Vector3.Zero;
		Camera.Transform.LocalRotation = Rotation.Identity;

		if ( Mode == CameraMode.ThirdPerson && !Player.IsLocallyControlled )
		{
			// orbit cam: spectating only
			var angles = Boom.Transform.Rotation.Angles();
			angles += Input.AnalogLook;
			Boom.Transform.Rotation = angles.WithPitch( angles.pitch.Clamp( -90, 90 ) ).ToRotation();
		}
		else
		{
			Boom.Transform.Rotation = Player.EyeAngles.ToRotation();
		}

		if ( MaxBoomLength > 0 )
		{
			var tr = Scene.Trace.Ray( new Ray( Boom.Transform.Position, Boom.Transform.Rotation.Backward ), MaxBoomLength )
				.IgnoreGameObjectHierarchy( GameObject.Root )
				.WithoutTags( "trigger", "player", "ragdoll" )
				.Run();

			Camera.Transform.LocalPosition = Vector3.Backward * (tr.Hit ? tr.Distance - 5.0f : MaxBoomLength);
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
		var pl = PlayerState.Local.PlayerPawn;
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

		Boom.Transform.LocalRotation *= Rotation.FromYaw( -yaw * LerpBobSpeed );
		Boom.Transform.LocalRotation *= Rotation.FromPitch( -pitch * LerpBobSpeed * 0.5f );
	}

	private void ApplyScope()
	{
		if ( !Player.CurrentEquipment.IsValid() )
			return;

		if ( Player?.CurrentEquipment?.Components.Get<ScopeWeaponComponent>( FindMode.EnabledInSelfAndDescendants ) is { } scope )
		{
			var fov = scope.GetFOV();
			FieldOfViewOffset -= fov;
		}
	}

	private void Update( float eyeHeight )
	{
		var baseFov = GameSettingsSystem.Current.FieldOfView;
		FieldOfViewOffset = 0;

		if ( !Player.IsValid() )
			return;

		if ( Player.CurrentEquipment.IsValid() )
		{ 
			if ( Player.CurrentEquipment?.Tags.Has( "aiming" ) ?? false )
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
			ColorAdjustments.Saturation = Player.HealthComponent.IsGodMode
				? RespawnProtectionSaturation
				: ColorAdjustments.Saturation.MoveToLinear( 1f, 1f );
		}

		ApplyRecoil();
		ApplyScope();

		Boom.Transform.LocalPosition = Vector3.Zero.WithZ( eyeHeight );

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
		if ( !Player.CurrentEquipment.IsValid() )
			return;

		if ( Player.CurrentEquipment?.Components.Get<RecoilWeaponComponent>( FindMode.EnabledInSelfAndDescendants ) is { } fn )
			Player.EyeAngles += fn.Current;
	}

	void OnModeChanged()
	{
		SetBoomLength( Mode == CameraMode.FirstPerson ? 0.0f : ThirdPersonDistance );

		var firstPersonPOV = Mode == CameraMode.FirstPerson && Player.IsViewer;
		Player.Body?.SetFirstPersonView( firstPersonPOV );

		if ( firstPersonPOV )
			Player.CreateViewModel( false );
		else
			Player.ClearViewModel();
	}

	private void SetBoomLength(float length)
	{
		MaxBoomLength = length;
	}
}
