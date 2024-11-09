using Facepunch;

namespace Sandbox.Movement;

/// <summary>
/// The character is noclipping
/// </summary>
[Icon( "scuba_diving" ), Group( "Movement" ), Title( "MoveMode - Noclip" )]
public partial class MoveModeNoclip : MoveMode
{
	public override void UpdateRigidBody( Rigidbody body )
	{
		body.Gravity = false;
		body.LinearDamping = 1f;
		body.AngularDamping = 1f;
	}

	public override int Score( PlayerController controller )
	{
		return GetComponent<PlayerPawn>().IsNoclipping ? 1000 : -1000;
	}

	public override void OnModeEnd( MoveMode next )
	{
	}

	protected override void OnFixedUpdate()
	{
	}

	public override Vector3 UpdateMove( Rotation eyes, Vector3 input )
	{
		return Input.AnalogMove;
	}
}
