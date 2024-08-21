using Sandbox;

namespace Facepunch;

public sealed class MainMenuCamera : Component
{
	public RealTimeUntil TimeUntilAnimComplete { get; set; } = 4f;

	[Property] 
	public CameraComponent Camera { get; set; }

	float lerpedVal;

	protected override void OnUpdate()
	{
		var x = (float)TimeUntilAnimComplete.Fraction.Clamp( 0f, 1f );

		lerpedVal = lerpedVal.LerpTo( x, Time.Delta * 2f );
		Transform.LocalPosition = Vector3.Right * -25f + ( Vector3.Right * 25f * lerpedVal );
		Transform.LocalRotation = Rotation.From( 0, 12.5f - ( 12.5f * lerpedVal ), 0 );

		Camera.FieldOfView = 60 - (5f * lerpedVal);
	}
}
