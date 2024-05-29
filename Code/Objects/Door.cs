using Sandbox;

namespace Facepunch;

public sealed class Door : Component, IUse
{
	/// <summary>
	/// Animation curve to use, X is the time between 0-1 and Y is how much the door is open to its target angle from 0-1.
	/// </summary>
	[Property] public Curve AnimationCurve { get; set; } = new Curve( new Curve.Frame( 0f, 0f ), new Curve.Frame( 1f, 1.0f ) );

	/// <summary>
	/// Sound to play when a door is opened.
	/// </summary>
	[Property, Group( "Sound" )] public SoundEvent OpenSound { get; set; }

	/// <summary>
	/// Sound to play when a door is fully opened.
	/// </summary>
	[Property, Group( "Sound" )] public SoundEvent OpenFinishedSound { get; set; }

	/// <summary>
	/// Sound to play when a door is closed.
	/// </summary>
	[Property, Group( "Sound" )] public SoundEvent CloseSound { get; set; }

	/// <summary>
	/// Sound to play when a door has finished closing.
	/// </summary>
	[Property, Group( "Sound" )] public SoundEvent CloseFinishedSound { get; set; }

	/// <summary>
	/// Optional pivot point, origin will be used if not specified.
	/// </summary>
	[Property] public GameObject Pivot { get; set; }

	/// <summary>
	/// How far should the door rotate.
	/// </summary>
	[Property, Range( 0.0f, 90.0f )] public float TargetAngle { get; set; } = 90.0f;

	/// <summary>
	/// How long in seconds should it take to open this door.
	/// </summary>
	[Property] public float OpenTime { get; set; } = 0.5f;

	/// <summary>
	/// Open away from the person who uses this door.
	/// </summary>
	[Property] public bool OpenAwayFromPlayer { get; set; } = true;

	public enum DoorState
	{
		Open,
		Opening,
		Closing,
		Closed
	}

	Transform StartTransform { get; set; }
	Vector3 PivotPosition { get; set; }
	bool ReverseDirection { get; set; }
	[Sync] public TimeSince LastUse { get; set; }
	[Sync] public DoorState State { get; set; } = DoorState.Closed;

	protected override void OnStart()
	{
		StartTransform = Transform.Local;
		PivotPosition = Pivot is not null ? Pivot.Transform.Position : StartTransform.Position;
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();
	}

	public bool CanUse( PlayerController player )
	{
		// Don't use doors already opening/closing
		return State == DoorState.Open || State == DoorState.Closed;
	}

	public void OnUse( PlayerController player )
	{
		LastUse = 0.0f;

		if ( State == DoorState.Closed )
		{
			State = DoorState.Opening;
			if ( OpenSound is not null ) Sound.Play( OpenSound, Transform.Position ).Occlusion = false;

			if ( OpenAwayFromPlayer )
			{
				var playerPosition = player.GameObject.Transform.Position;
				var doorToPlayer = (playerPosition - PivotPosition).Normal;
				var doorForward = Transform.Local.Rotation.Forward;

				ReverseDirection = Vector3.Dot( doorToPlayer, doorForward ) > 0;
			}
		}
		else if ( State == DoorState.Open )
		{
			State = DoorState.Closing;
			if ( CloseSound is not null ) Sound.Play( CloseSound, Transform.Position ).Occlusion = false;
		}
	}

	protected override void OnFixedUpdate()
	{
		// Don't do anything if we're not opening or closing
		if ( State != DoorState.Opening && State != DoorState.Closing )
			return;

		// Normalize the last use time to the amount of time to open
		var time = LastUse.Relative.Remap( 0.0f, OpenTime, 0.0f, 1.0f );

		// Evaluate our animation curve
		var curve = AnimationCurve.Evaluate( time );

		// Rotate backwards if we're closing
		if ( State == DoorState.Closing ) curve = 1.0f - curve;

		var targetAngle = TargetAngle;
		if ( ReverseDirection ) targetAngle *= -1.0f;

		// Do the rotation
		Transform.Local = StartTransform.RotateAround( PivotPosition, Rotation.FromYaw( curve * targetAngle ) );

		// If we're done finalize the state and play the sound
		if ( time >= 1.0f )
		{
			State = State == DoorState.Opening ? DoorState.Open : DoorState.Closed;

			if ( State == DoorState.Open && OpenFinishedSound is not null ) Sound.Play( OpenFinishedSound, Transform.Position ).Occlusion = false;
			if ( State == DoorState.Closed && CloseFinishedSound is not null ) Sound.Play( CloseFinishedSound, Transform.Position ).Occlusion = false;

			return;
		}
	}
}
