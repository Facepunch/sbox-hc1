namespace Facepunch;

public sealed class Door : Component, IUse, IRoundStartListener
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
	[HostSync] public TimeSince LastUse { get; set; }
	[HostSync] public DoorState State { get; set; } = DoorState.Closed;

	private DoorState DefaultState { get; set; } = DoorState.Closed;

	protected override void OnStart()
	{
		StartTransform = Transform.Local;
		PivotPosition = Pivot is not null ? Pivot.Transform.Position : StartTransform.Position;
		DefaultState = State;
	}

	public bool CanUse( PlayerController player )
	{
		// Don't use doors already opening/closing
		return State is DoorState.Open or DoorState.Closed;
	}
	
	void IRoundStartListener.PreRoundStart()
	{
		Transform.Local = StartTransform;
		State = DefaultState;
	}

	private void PlaySound( SoundEvent resource )
	{
		PlaySoundRpc( resource.ResourceId );
	}
	
	[Broadcast]
	private void PlaySoundRpc( int resourceId )
	{
		var resource = ResourceLibrary.Get<SoundEvent>( resourceId );
		if ( resource == null ) return;
		
		var handle = Sound.Play( resource, Transform.Position );
		if ( !handle.IsValid() ) return;
		
		// handle.Occlusion = false;
	}

	public void OnUse( PlayerController player )
	{
		LastUse = 0.0f;

		if ( State == DoorState.Closed )
		{
			State = DoorState.Opening;
			if ( OpenSound is not null )
				PlaySound( OpenSound );

			if ( OpenAwayFromPlayer )
			{
				var doorToPlayer = (player.Transform.Position - PivotPosition).Normal;
				var doorForward = Transform.Local.Rotation.Forward;

				ReverseDirection = Vector3.Dot( doorToPlayer, doorForward ) > 0;
			}
		}
		else if ( State == DoorState.Open )
		{
			State = DoorState.Closing;
			if ( CloseSound is not null )
				PlaySound( CloseSound );
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
		if ( time < 1f ) return;
		
		State = State == DoorState.Opening ? DoorState.Open : DoorState.Closed;

		if ( Networking.IsHost )
		{
			if ( State == DoorState.Open && OpenFinishedSound is not null )
				PlaySound( OpenFinishedSound );
		
			if ( State == DoorState.Closed && CloseFinishedSound is not null )
				PlaySound( CloseFinishedSound );
		}
	}
}
