using Sandbox;

namespace Facepunch;

public sealed class CameraController : Component
{
	/// <summary>
	/// A reference to the camera component we're going to be doing stuff with.
	/// </summary>
	[Property] public CameraComponent Camera { get; set; }
	[Property] public AudioListener AudioListener { get; set; }

	[Property] public PlayerController Player { get; set; }

	[Property, Group( "Config" )] public bool ShouldViewBob { get; set; } = false;

	/// <summary>
	/// Constructs a ray using the camera's GameObject
	/// </summary>
	public Ray AimRay => new Ray( Camera.Transform.Position + Camera.Transform.Rotation.Forward * 25f, Camera.Transform.Rotation.Forward );

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

		Player.Body.ShowBodyParts( !isActive );
	}

	/// <summary>
	/// Updates the camera's position, from player code
	/// </summary>
	/// <param name="eyeHeight"></param>
	internal void UpdateFromEyes( float eyeHeight )
	{
		// Don't move eyes if we're dead
		if ( Player.HealthComponent.State != LifeState.Alive )
			return;

		Camera.Transform.Rotation = Player.EyeAngles.ToRotation();
		Camera.Transform.LocalPosition = Vector3.Zero.WithZ( eyeHeight );

		if ( ShouldViewBob )
		{
			ViewBob();
		}
	}

	float walkBob = 0;

	Rotation lerpedRotation = Rotation.Identity;
	Vector3 lerpedPosition = Vector3.Zero;

	/// <summary>
	/// Bob the view!
	/// This could be better, but it doesn't matter really.
	/// </summary>
	void ViewBob()
	{
		var targetRotation = Rotation.Identity;
		var targetPosition = Vector3.Zero;

		var bobSpeed = Player.CharacterController.Velocity.Length.LerpInverse( 0, 300 );
		if ( !Player.IsGrounded ) bobSpeed *= 0.1f;

		walkBob += Time.Delta * 10.0f * bobSpeed;
		var yaw = MathF.Sin( walkBob ) * 0.5f;
		var pitch = MathF.Cos( -walkBob * 2f ) * 0.5f;

		Camera.Transform.LocalRotation *= Rotation.FromYaw( -yaw * bobSpeed );
		Camera.Transform.LocalRotation *= Rotation.FromPitch( -pitch * bobSpeed * 0.5f );

		lerpedRotation = Rotation.Lerp( lerpedRotation, targetRotation, Time.Delta * 5f );
		lerpedPosition = lerpedPosition.LerpTo( targetPosition, Time.Delta * 5f );

		Camera.Transform.LocalRotation *= lerpedRotation;
		Camera.Transform.LocalPosition += lerpedPosition;
	}

	protected override void OnUpdate()
	{
		var baseFov = Preferences.FieldOfView;

		TargetFieldOfView = TargetFieldOfView.LerpTo( baseFov + FieldOfViewOffset, Time.Delta * 5f );
		FieldOfViewOffset = 0;
		Camera.FieldOfView = TargetFieldOfView;

		ApplyRecoil();
	}

	void ApplyRecoil()
	{
		if ( !Player.IsLocallyControlled )
		{
			return;
		}

		if ( Player.CurrentWeapon.IsValid() && Player.CurrentWeapon?.GetFunction<RecoilFunction>() is { } fn )
		{
			Player.EyeAngles += fn.Current;
		}

		if ( Player.CurrentWeapon.IsValid() && Player.CurrentWeapon?.GetFunction<SwayFunction>() is { } sFn )
		{
			Player.EyeAngles += sFn.Current;
		}
	}
}
