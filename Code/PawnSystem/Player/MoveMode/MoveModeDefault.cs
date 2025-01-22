using Sandbox.Movement;

namespace Facepunch;

[Title( "HC1 - Move Mode Default" ), Tint( EditorTint.Yellow )]
public partial class MoveModeDefault : MoveMode
{
	public PlayerPawn Player => GetComponentInParent<PlayerPawn>();

	[Property] public int Priority { get; set; } = 0;

	[Property] public float GroundAngle { get; set; } = 45.0f;
	[Property] public float StepUpHeight { get; set; } = 18.0f;
	[Property] public float StepDownHeight { get; set; } = 18.0f;

	public override bool AllowGrounding => true;
	public override bool AllowFalling => true;

	public override int Score( PlayerController controller ) => Priority;

	public override void AddVelocity()
	{
		var body = Controller.Body;
		var wish = Controller.WishVelocity;
		if ( wish.IsNearZeroLength ) return;

		var groundFriction = (Controller.GroundFriction / 4f) * Player.GetFriction();
		var groundVelocity = Controller.GroundVelocity;

		var z = body.Velocity.z;

		var velocity = (body.Velocity - Controller.GroundVelocity);
		var speed = velocity.Length;

		var maxSpeed = MathF.Max( wish.Length, speed );

		if ( Controller.IsOnGround )
		{
			velocity = velocity.AddClamped( wish * (0.05f + (groundFriction / 2f)), wish.Length );
		}
		else
		{
			var amount = 0.05f;
			velocity = velocity.AddClamped( wish * amount, wish.Length );
		}

		if ( velocity.Length > maxSpeed )
			velocity = velocity.Normal * maxSpeed;

		velocity += groundVelocity;

		if ( Controller.IsOnGround )
		{
			velocity.z = z;
		}

		body.Velocity = velocity;
	}

	public override void PrePhysicsStep()
	{
		if ( StepUpHeight > 0 )
		{
			TrySteppingUp( StepUpHeight );
		}
	}

	public override void PostPhysicsStep()
	{
		if ( StepDownHeight > 0 )
		{
			StickToGround( StepDownHeight );
		}
	}

	public override bool IsStandableSurface( in SceneTraceResult result )
	{
		if ( Vector3.GetAngle( Vector3.Up, result.Normal ) > GroundAngle )
			return false;

		return true;
	}

	public override Vector3 UpdateMove( Rotation eyes, Vector3 input )
	{
		// ignore pitch when walking
		eyes = eyes.Angles() with { pitch = 0 };

		var wish = eyes * input;
		var velocity = Player.GetWishSpeed();

		return wish * velocity;
	}
}
