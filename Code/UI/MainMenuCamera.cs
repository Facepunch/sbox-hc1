using Sandbox.Utility;

namespace Facepunch;

public sealed class MainMenuCamera : Component
{
	public RealTimeUntil TimeUntilAnimComplete { get; set; } = 4f;

	[Property] 
	public CameraComponent Camera { get; set; }

	float lerpedVal;

	[Property] 
	public float positionAmplitude { get; set; } = 0.5f;

	[Property]
	public float positionFrequency { get; set; } = 10f;

	[Property] 
	public float rotationAmplitude { get; set; } = 1f;

	[Property]
	public float rotationFrequency { get; set; } = 10f;

	protected override void OnUpdate()
	{
		var x = (float)TimeUntilAnimComplete.Fraction.Clamp( 0f, 1f );

		lerpedVal = lerpedVal.LerpTo( x, Time.Delta * 2f );
		LocalPosition = Vector3.Right * -25f + ( Vector3.Right * 25f * lerpedVal );
		LocalRotation = Rotation.From( 0, 12.5f - ( 12.5f * lerpedVal ), 0 );

		float positionOffsetX = Noise.Perlin( Time.Now * positionFrequency, 0.0f ) * positionAmplitude - (positionAmplitude / 2.0f);
		float positionOffsetY = Noise.Perlin( 0.0f, Time.Now * positionFrequency ) * positionAmplitude - (positionAmplitude / 2.0f);
		float positionOffsetZ = Noise.Perlin( Time.Now * positionFrequency, Time.Now * positionFrequency ) * positionAmplitude - (positionAmplitude / 2.0f);
		float rotationOffsetX = Noise.Perlin( Time.Now * rotationFrequency, 0.0f ) * rotationAmplitude - (rotationAmplitude / 2.0f);
		float rotationOffsetY = Noise.Perlin( 0.0f, Time.Now * rotationFrequency ) * rotationAmplitude - (rotationAmplitude / 2.0f);
		float rotationOffsetZ = Noise.Perlin( Time.Now * rotationFrequency, Time.Now * rotationFrequency ) * rotationAmplitude - (rotationAmplitude / 2.0f);

		LocalPosition += new Vector3( positionOffsetX, positionOffsetY, positionOffsetZ );
		LocalRotation *= Rotation.From( rotationOffsetX, rotationOffsetY, rotationOffsetZ );

		Camera.FieldOfView = 60 - (5f * lerpedVal);
	}
}
