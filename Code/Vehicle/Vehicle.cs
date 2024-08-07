namespace Facepunch;

public partial class Vehicle : Component, IRespawnable, ICustomMinimapIcon, ITeam, IUse
{
	[Property, Group( "Components" )] public Rigidbody Rigidbody { get; set; }
	[Property, Group( "Components" )] public ModelRenderer Model { get; set; }

	/// <summary>
	/// What team does this pawn belong to?
	/// </summary>
	public virtual Team Team { get; set; } = Team.Unassigned;

	[RequireComponent] public PawnCameraController CameraController { get; set; }

	/// <summary>
	/// An accessor for health component if we have one.
	/// </summary>
	[Property] public virtual HealthComponent HealthComponent { get; set; }

	/// <summary>
	/// What to spawn when we explode?
	/// </summary>
	[Property] public GameObject Explosion { get; set; }

	[Property, Group( "Vehicle" )] public List<Wheel> Wheels { get; set; }
	[Property, Group( "Vehicle" )] public List<VehicleSeat> Seats { get; set; }
	[Property, Group( "Vehicle" )] public float Torque { get; set; } = 15000f;
	[Property, Group( "Vehicle" )] public float AccelerationRate { get; set; } = 1.0f;
	[Property, Group( "Vehicle" )] public float DecelerationRate { get; set; } = 0.5f;
	[Property, Group( "Vehicle" )] public float BrakingRate { get; set; } = 2.0f;

	public VehicleInputState InputState { get; set; }

	private float _currentTorque;

	protected override void OnFixedUpdate()
	{
		float verticalInput = InputState.direction.x;
		float targetTorque = verticalInput * Torque;

		bool isBraking = Math.Sign( verticalInput * _currentTorque ) == -1;
		bool isDecelerating = verticalInput == 0;

		float lerpRate = AccelerationRate;
		if ( isBraking )
			lerpRate = BrakingRate;
		else if ( isDecelerating )
			lerpRate = DecelerationRate;

		_currentTorque = _currentTorque.LerpTo( targetTorque, lerpRate * Time.Delta );

		foreach ( Wheel wheel in Wheels )
		{
			wheel.ApplyMotorTorque( _currentTorque );
		}

		var groundVel = Rigidbody.Velocity.WithZ( 0f );
		if ( verticalInput == 0f && groundVel.Length < 32f )
		{
			var z = Rigidbody.Velocity.z;
			Rigidbody.Velocity = Vector3.Zero.WithZ( z );
		}
	}

	public void OnKill( DamageInfo damageInfo )
	{
		foreach ( var seat in Seats )
		{
			seat.Eject();
		}

		Explosion?.Clone( Transform.Position );
		GameObject.Destroy();
	}

	bool IMinimapElement.IsVisible( Pawn viewer )
	{
		return viewer.Team == Team;
	}

	public bool CanUse( PlayerPawn player )
	{
		return true;
	}

	public void OnUse( PlayerPawn player )
	{
		if ( player.CurrentSeat.IsValid() && player.CurrentSeat.Vehicle == this )
		{
			// Leave the seat
			if ( player.CurrentSeat.Leave( player ) )
			{
				Log.Info( "Left vehicle" );
			}

			return;
		}

		if ( Seats.FirstOrDefault( x => x.CanEnter( player ) ) is { } availableSeat )
		{
			if ( availableSeat.Enter( player ) )
			{
				Log.Info( "Entered vehicle" );
			}

			return;
		}
	}

	string IMinimapIcon.IconPath => "ui/icons/vehicle.png";
	string ICustomMinimapIcon.CustomStyle => $"";
	Vector3 IMinimapElement.WorldPosition => Transform.Position;
}
