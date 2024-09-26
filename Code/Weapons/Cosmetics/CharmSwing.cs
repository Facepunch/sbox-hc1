using System;
using System.Numerics;

namespace Facepunch;

public sealed class CharmSwing : Component
{
	[Property]
	public GameObject AttachmentPoint { get; set; }

	// Adjustable parameters for swing behavior
	public float swingStrength => 50.0f;  // How strong the swing effect is
	public float damping => 0.01f;        // Damping factor to slow down the swing over time
	public float maxSwingAngle = 30.0f; // Maximum angle the charm can swing

	private Rotation initialRotation;  // Initial rotation relative to the attachment point
	private Vector3 previousPosition;    // Previous frame's position of the attachment point
	private Vector3 swingVelocity;       // Current swing velocity of the charm

	protected override void OnStart()
	{
		// Record the initial local rotation of the charm
		initialRotation = Transform.LocalRotation;
		
		if ( AttachmentPoint.IsValid() )
		{
			previousPosition = AttachmentPoint.Transform.Position;
		}
	}

	private Rotation targetRotation; // Target rotation of the charm based on the swing

	protected override void OnUpdate()
	{
		// Calculate the movement direction of the gun (or parent object)
		Vector3 currentPosition = Transform.Position;
		Vector3 movementDirection = currentPosition - previousPosition;
		previousPosition = currentPosition;

		// Transform the movement direction to local space relative to the charm
		Vector3 localMovement = AttachmentPoint.Transform.Rotation.Inverse * movementDirection;

		// Calculate the desired swing rotation based on movement and gravity
		ApplySwing( localMovement );

		// Smoothly interpolate towards the target rotation using Quaternion.Slerp
		Transform.LocalRotation = Quaternion.Slerp( Transform.LocalRotation, initialRotation * targetRotation, Time.Delta * swingStrength );
	}

	void ApplySwing( Vector3 localMovement )
	{
		// Calculate the angle we need to swing based on the movement direction
		float angleX = Math.Clamp( localMovement.y * swingStrength, -maxSwingAngle, maxSwingAngle );
		float angleZ = Math.Clamp( -localMovement.x * swingStrength, -maxSwingAngle, maxSwingAngle );

		// Create a quaternion for the target rotation (swing direction)
		Rotation swingRotation = Rotation.From( angleX, 0, angleZ );

		// Set the target rotation relative to the initial orientation
		targetRotation = swingRotation;

		// Apply damping to reduce the swing over time
		swingVelocity = Vector3.Lerp( swingVelocity, Vector3.Zero, damping * Time.Delta );
	}
}
