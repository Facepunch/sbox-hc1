namespace Facepunch;

public sealed class SpectateController : Pawn
{

	[Property] public float FlySpeed = 10f;
	[Property] public CameraComponent CameraComponent { get; set; }

	public override CameraComponent Camera => CameraComponent;

	/// <summary>
	/// What are we called?
	/// </summary>
	public override string DisplayName => Network.OwnerConnection.DisplayName + " (spectator)";

	protected override void OnAwake()
	{
		Camera.Enabled = false;
	}

	protected override void OnUpdate()
	{
		EyeAngles += Input.AnalogLook;
		EyeAngles = EyeAngles.WithPitch( EyeAngles.pitch.Clamp( -90, 90 ) );
		Transform.Rotation = EyeAngles.ToRotation();

		Transform.Position += Input.AnalogMove * Transform.Rotation * FlySpeed * Time.Delta;
	}

	public override void OnDePossess()
	{
		Camera.Enabled = false;
	}

	public override void OnPossess()
	{
		Camera.Enabled = true;
	}
}
