using Facepunch;

namespace Sandbox.Movement;

/// <summary>
/// The character is noclipping
/// </summary>
[Icon( "scuba_diving" ), Group( "Movement" ), Title( "HC1 - Move Mode Noclip" ), Tint( EditorTint.Yellow )]
public partial class MoveModeNoclip : MoveMode
{
	public PlayerPawn Player => GetComponentInParent<PlayerPawn>();

	public override void UpdateRigidBody( Rigidbody body )
	{
		body.Gravity = false;
		body.LinearDamping = 10f;
		body.AngularDamping = 10f;
	}

	public override void OnModeBegin()
	{
		Controller.ColliderObject.Enabled = false;
	}

	public override void OnModeEnd( MoveMode next )
	{
		Controller.ColliderObject.Enabled = true;
	}

	public override int Score( PlayerController controller )
	{
		return GetComponent<PlayerPawn>().IsNoclipping ? 1000 : -1000;
	}

	public override Vector3 UpdateMove( Rotation eyes, Vector3 input )
	{
		// 💩💩💩
		var rot = eyes.Angles() with { pitch = Scene.Camera.WorldRotation.Pitch() };

		if ( Input.Down( "jump" ) )
		{
			input += Vector3.Up;
		}

		if ( Input.Down( "Duck" ) )
		{
			input += Vector3.Down;
		}

		return rot.ToRotation() * input * 4000f;
	}
}
