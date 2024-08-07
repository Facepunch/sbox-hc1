namespace Facepunch;

public sealed class Steering : Component
{
	[Property] public float MaxSteeringAngle { get; set; } = 20f;
	[Property] public float SteeringSmoothness { get; set; } = 10f;

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

		var targetRotation = Rotation.FromYaw( MaxSteeringAngle * inputState.direction.y );
		Transform.LocalRotation = Rotation.Lerp( Transform.LocalRotation, targetRotation, Time.Delta * SteeringSmoothness );
	}
}
