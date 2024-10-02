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
	RangedFloat YawRange { get; set; } = new( -20, 20 );

	[Property]
	RangedFloat RollRange { get; set; } = new( -10, 10 );

	[Property]
	private RangedFloat ZoomRange { get; set; } = new( -0.6f, 0.3f );

	private Vector2 angleOffset;
	private Vector2 lastFrameMousePos = 0;
	private float InitialFOV = 60;
	private float ZoomFactor = 0;

	protected override void OnStart()
	{
		InitialFOV = CameraComponent.FieldOfView;
	}

	protected override void OnUpdate()
	{
		var mousePos = Mouse.Position;
		var delta = mousePos - lastFrameMousePos;
		lastFrameMousePos = Mouse.Position;

		if ( Input.Down( "Attack1" ) )
		{
			// Rotate the camera around the orbitObject
			angleOffset += new Vector2( delta.x * OrbitSpeed, delta.y * OrbitSpeed );
		}

		var scroll = -Input.MouseWheel.y;

		ZoomFactor += scroll * Time.Delta * 5f;
		ZoomFactor = ZoomFactor.Clamp( ZoomRange.Min, ZoomRange.Max );

		angleOffset = angleOffset.LerpTo( 0, Time.Delta * SlowSpeed );
		Weapon.Transform.Rotation *= Rotation.From( 0, angleOffset.x, -angleOffset.y );

		var ang = Weapon.Transform.Rotation.Angles();
		var clampedAng = new Angles( 0, ang.yaw.Clamp( YawRange.Min, YawRange.Max ), ang.roll.Clamp( RollRange.Min, RollRange.Max ) );

		Weapon.Transform.Rotation = Rotation.From( clampedAng );
		CameraComponent.FieldOfView = InitialFOV + ( ZoomFactor * 10f );
	}
}
