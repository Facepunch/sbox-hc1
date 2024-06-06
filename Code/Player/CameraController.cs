using Facepunch.UI;
using Sandbox;

namespace Facepunch;

public enum CameraMode
{
	FirstPerson,
	ThirdPerson
}

public sealed class CameraController : Component
{
	/// <summary>
	/// A reference to the camera component we're going to be doing stuff with.
	/// </summary>
	[Property] public CameraComponent Camera { get; set; }
	[Property] public GameObject Boom { get; set; }
	[Property] public AudioListener AudioListener { get; set; }
	[Property] public ColorAdjustments ColorAdjustments { get; set; }
	[Property] public PlayerController Player { get; set; }

	[Property, Group( "Config" )] public bool ShouldViewBob { get; set; } = false;
	[Property, Group( "Config" )] public float RespawnProtectionSaturation { get; set; } = 0.25f;

	[ConVar( "hc1_thirdperson" )] public static bool ThirdPersonEnabled { get; set; } = false;
	[Property] public float ThirdPersonDistance { get; set; } = 128f;

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
	public Ray AimRay => new( Camera.Transform.Position + Camera.Transform.Rotation.Forward, Camera.Transform.Rotation.Forward );

	private float FieldOfViewOffset = 0f;
	private float TargetFieldOfView = 90f;

	public void AddFieldOfViewOffset( float degrees )
	{
		FieldOfViewOffset -= degrees;
	}

	public void SetActive( bool isActive )
	{
		Camera.Enabled = isActive;
		AudioListener.Enabled = isActive;

		OnModeChanged();

		Boom.Transform.Rotation = Player.EyeAngles.ToRotation();
	}

	/// <summary>
	/// Updates the camera's position, from player code
	/// </summary>
	/// <param name="eyeHeight"></param>
	internal void UpdateFromEyes( float eyeHeight )
	{
		if ( Player.IsLocallyControlled )
		{
			Boom.Transform.Rotation = Player.EyeAngles.ToRotation();
		}
		else
		{
			Boom.Transform.Rotation = Rotation.Lerp( Boom.Transform.Rotation,
				Player.EyeAngles.ToRotation(), Time.Delta / Scene.NetworkRate );
		}

		Boom.Transform.LocalPosition = Vector3.Zero.WithZ( eyeHeight );

		if ( ShouldViewBob )
		{
			ViewBob();
		}
	}

	float walkBob = 0;

	Rotation lerpedRotation = Rotation.Identity;

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

		walkBob += Time.Delta * 10.0f * bobSpeed;
		var yaw = MathF.Sin( walkBob ) * 0.5f;
		var pitch = MathF.Cos( -walkBob * 2f ) * 0.5f;

		Boom.Transform.LocalRotation *= Rotation.FromYaw( -yaw * bobSpeed );
		Boom.Transform.LocalRotation *= Rotation.FromPitch( -pitch * bobSpeed * 0.5f );
	}

	protected override void OnStart()
	{
		// Create a highlight component if it doesn't exist on the camera.
		Camera.Components.GetOrCreate<Highlight>();
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		var baseFov = GameSettingsSystem.Current.FieldOfView;

		TargetFieldOfView = TargetFieldOfView.LerpTo( baseFov + FieldOfViewOffset, Time.Delta * 5f );
		FieldOfViewOffset = 0;
		Camera.FieldOfView = TargetFieldOfView;

		if ( ColorAdjustments is not null )
		{
			ColorAdjustments.Saturation = Player.HealthComponent.IsGodMode
				? RespawnProtectionSaturation
				: ColorAdjustments.Saturation.MoveToLinear( 1f, 1f );
		}

		// Developer override
		if ( ThirdPersonEnabled & DeveloperMenu.IsDeveloper )
		{
			if ( Mode == CameraMode.ThirdPerson )
				Mode = CameraMode.FirstPerson;
			else
				Mode = CameraMode.ThirdPerson;
		}

		ApplyRecoil();

		if (MaxBoomLength > 0)
		{
			var tr = Scene.Trace.Ray( new Ray( Boom.Transform.Position, Boom.Transform.Rotation.Backward ), MaxBoomLength )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "trigger", "player" )
			.Run();

			Camera.Transform.LocalPosition = Vector3.Backward * (tr.Hit ? tr.Distance - 5.0f : MaxBoomLength);
			Camera.Transform.LocalPosition += Vector3.Right * 20f;
		}
		else
		{
			Camera.Transform.LocalPosition = Vector3.Backward * 0.0f;
		}
		Camera.Transform.LocalRotation = Rotation.Identity;	
	}

	void ApplyRecoil()
	{
		if ( !Player.IsViewer )
			return;

		if ( Player.CurrentWeapon.IsValid() && Player.CurrentWeapon?.Components.Get<RecoilWeaponComponent>( FindMode.EnabledInSelfAndDescendants ) is { } fn )
			Player.EyeAngles += fn.Current;
	}

	void OnModeChanged()
	{
		SetBoomLength( Mode == CameraMode.FirstPerson ? 0.0f : ThirdPersonDistance );

		var firstPersonPOV = Mode == CameraMode.FirstPerson && Player.IsViewer;
		Player.Body.ShowBodyParts( !firstPersonPOV );

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
