using Sandbox;

namespace Facepunch;

public sealed class GunsmithController : Component
{
	[Property] 
	public GunsmithWeapon Weapon { get; set; }

	[Property] 
	public CameraComponent CameraComponent { get; set; }

	[Property]
	float OrbitSpeed { get; set; } = 0.01f;

	[Property]
	float SlowSpeed { get; set; } = 5f;

	[Property]
	RangedFloat PitchRange { get; set; } = new( -20, 20 );

	[Property]
	RangedFloat YawRange { get; set; } = new( -35, 35 );

	private Vector2 angleOffset;
	private Vector2 lastFrameMousePos = 0;

	protected override void OnUpdate()
	{
		var mousePos = Mouse.Position;
		var delta = mousePos - lastFrameMousePos;
		lastFrameMousePos = Mouse.Position;

		if ( Input.Down( "Attack1" ) )
		{
			// Rotate the camera around the orbitObject
			angleOffset += new Vector2( delta.y * OrbitSpeed, delta.x * OrbitSpeed );

			// Limit pitch angle to prevent camera flipping
			angleOffset.x = Math.Clamp( angleOffset.x, PitchRange.Min, PitchRange.Max );
		}

		angleOffset = angleOffset.LerpTo( 0, Time.Delta * SlowSpeed );
		Weapon.Transform.Rotation *= Rotation.From( 0, -angleOffset.y, 0 );
		Weapon.Transform.Rotation *= Rotation.From( angleOffset.x, 0, 0 );

		var ang = Weapon.Transform.Rotation.Angles();
		var clampedAng = new Angles( ang.pitch.Clamp( PitchRange.Min, PitchRange.Max ), ang.yaw.Clamp( YawRange.Min, YawRange.Max ), 0 );

		Weapon.Transform.Rotation = Rotation.From( clampedAng );
	}
}
