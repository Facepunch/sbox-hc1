using Sandbox;
using System;

[Category( "Vehicles" )]
[Title( "Wheel" )]
[Icon( "sync" )]
public sealed class Wheel : Component
{
	[Property] public float MinSuspensionLength { get; set; } = 0f;
	[Property] public float MaxSuspensionLength { get; set; } = 8f;
	[Property] public float SuspensionStiffness { get; set; } = 3000.0f;
	[Property] public float SuspensionDamping { get; set; } = 140.0f;
	[Property] public float WheelRadius { get; set; } = 14.0f;

	[Property] public WheelFrictionInfo ForwardFriction { get; set; }
	[Property] public WheelFrictionInfo SideFriction { get; set; }

	public bool IsGrounded => _groundTrace.Hit;

	private const float LowSpeedThreshold = 20.0f;

	private SceneTraceResult _groundTrace;
	private Rigidbody _rigidbody;
	private float _motorTorque;
	private float _suspensionTotalLength;

	protected override void OnEnabled()
	{
		_rigidbody = Components.GetInAncestorsOrSelf<Rigidbody>();
		_suspensionTotalLength = (MaxSuspensionLength + WheelRadius) - MinSuspensionLength;
	}

	protected override void OnFixedUpdate()
	{
		if ( !_rigidbody.IsValid() )
			return;

		DoTrace();

		UpdateSuspension();
		UpdateWheelForces();
	}

	public void ApplyMotorTorque( float value )
	{
		_motorTorque = value;
	}

	private void UpdateWheelForces()
	{
		Log.Info( IsGrounded + ", " + _motorTorque + ", " + _rigidbody.Velocity );

		if ( !IsGrounded )
			return;

		var forwardDir = Transform.Rotation.Forward;
		var sideDir = Transform.Rotation.Right;
		var wheelVelocity = _rigidbody.GetVelocityAtPoint( Transform.Position );
		var wheelSpeed = wheelVelocity.Length;

		var sideForce = Vector3.Zero;
		var forwardForce = Vector3.Zero;

		if ( wheelSpeed > LowSpeedThreshold )
		{
			float sideSlip = CalculateSlip( wheelVelocity, sideDir, wheelSpeed );
			float forwardSlip = CalculateSlip( wheelVelocity, forwardDir, wheelSpeed );

			sideForce = CalculateFrictionForce( SideFriction, sideSlip, sideDir );
			forwardForce = CalculateFrictionForce( ForwardFriction, forwardSlip, forwardDir );
		}

		var targetAcceleration = (sideForce + forwardForce);
		targetAcceleration += _motorTorque * Transform.Rotation.Forward;

		var force = targetAcceleration / Time.Delta;
		_rigidbody.ApplyForceAt( GameObject.Transform.Position, force );
	}

	private float CalculateSlip( Vector3 velocity, Vector3 direction, float speed )
	{
		var epsilon = 0.01f; // to avoid division by zero
		return Vector3.Dot( velocity, direction ) / (speed + epsilon);
	}

	private Vector3 CalculateFrictionForce( WheelFrictionInfo friction, float slip, Vector3 direction )
	{
		return -friction.Evaluate( MathF.Abs( slip ) ) * MathF.Sign( slip ) * direction;
	}

	public Vector3 GetCenter()
	{
		var up = _rigidbody.Transform.Rotation.Up;

		return _groundTrace.EndPosition + up * WheelRadius;
	}

	private void UpdateSuspension()
	{
		if ( !IsGrounded )
			return;

		var worldVelocity = _rigidbody.GetVelocityAtPoint( Transform.Position );
		var suspensionCompression = _groundTrace.Distance - _suspensionTotalLength;

		var dampingForce = -SuspensionDamping * worldVelocity.z;
		var springForce = -SuspensionStiffness * suspensionCompression;
		var totalForce = (dampingForce + springForce) / Time.Delta;

		var suspensionForce = new Vector3( 0, 0, totalForce );
		_rigidbody.ApplyForceAt( Transform.Position, suspensionForce );
	}

	private void DoTrace()
	{
		var down = _rigidbody.Transform.Rotation.Down;

		var startPos = Transform.Position + down * MinSuspensionLength;
		var endPos = startPos + down * (MaxSuspensionLength + WheelRadius);

		_groundTrace = Scene.Trace
				.Radius( 1f )
				.IgnoreGameObjectHierarchy( GameObject )
				.WithoutTags( "car", "vehicle", "wheel" )
				.FromTo( startPos, endPos )
				.Run();
	}
}
