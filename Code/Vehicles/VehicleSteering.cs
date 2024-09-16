namespace Facepunch;

public sealed class Steering : Component
{
	[Property] public List<GameObject> Wheels { get; set; }
	[Property] public float MaxSteeringAngle { get; set; } = 20f;
	[Property] public float SteeringSmoothness { get; set; } = 10f;
	[Property] public Angles Offset { get; set; }

	private Vehicle ParentVehicle;

	protected override void OnEnabled()
	{
		ParentVehicle = Components.GetInAncestors<Vehicle>();
	}

	protected override void OnFixedUpdate()
	{
		if ( Scene.IsEditor )
			return;

		var inputState = ParentVehicle.InputState;

		foreach ( var wheel in Wheels )
		{
			var targetRotation = Rotation.FromYaw( MaxSteeringAngle * inputState.direction.y ) * Rotation.From( Offset );
			wheel.Transform.LocalRotation = Rotation.Lerp( wheel.Transform.LocalRotation, targetRotation, Time.Delta * SteeringSmoothness );
		}
	}
}
