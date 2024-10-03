namespace Facepunch;

public sealed class SpectateController : Pawn
{
	[RequireComponent] PawnCameraController PawnCameraController { get; set; }
	[Property] public float FlySpeed = 10f;
	public override CameraComponent Camera => PawnCameraController.Camera;

	/// <summary>
	/// What are we called?
	/// </summary>
	public override string DisplayName => "Spectator";

	protected override void OnUpdate()
	{
		EyeAngles += Input.AnalogLook;
		EyeAngles = EyeAngles.WithPitch( EyeAngles.pitch.Clamp( -90, 90 ) );
		WorldRotation = EyeAngles.ToRotation();

		WorldPosition += Input.AnalogMove * WorldRotation * FlySpeed * Time.Delta;
	}

	public override void OnDePossess()
	{
		PawnCameraController.SetActive( false );
	}

	public override void OnPossess()
	{
		PawnCameraController.SetActive( true );
	}
}
