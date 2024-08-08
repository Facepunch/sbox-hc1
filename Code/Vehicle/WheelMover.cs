namespace Facepunch;

public sealed class WheelMover : Component
{
	[Property] public Wheel Target { get; set; }
	[Property] public bool ReverseRotation { get; set; }
	[Property] public float Speed { get; set; } = MathF.PI;

	private Rigidbody _rigidbody;

	protected override void OnEnabled()
	{
		_rigidbody = Components.GetInAncestors<Rigidbody>();
	}

	protected override void OnFixedUpdate()
	{
		var groundVel = _rigidbody.Velocity;

		Transform.Position = Target.GetCenter();
		Transform.LocalRotation *= Rotation.From( 0, 0, groundVel.Length * Time.Delta * (ReverseRotation ? -1f : 1f) * Speed );
	}
}
