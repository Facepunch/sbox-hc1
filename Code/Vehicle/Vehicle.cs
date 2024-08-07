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
	[Property] public float Torque { get; set; } = 15000f;

	private VehicleInputState _inputState;

	public void SetInputState( VehicleInputState inputState )
	{
		_inputState = inputState;
	}


	private float _currentTorque;

	protected override void OnFixedUpdate()
	{
		float verticalInput = _inputState.direction.x;
		float targetTorque = verticalInput * Torque;

		bool isBraking = (targetTorque < 0f);
		float lerpRate = isBraking ? 5.0f : 1.0f; // Brake applies quicker

		_currentTorque = _currentTorque.LerpTo( targetTorque, lerpRate * Time.Delta );
		_currentTorque = _currentTorque.Clamp( 0, float.MaxValue );

		foreach ( Wheel wheel in Wheels )
		{
			wheel.ApplyMotorTorque( _currentTorque );
		}

		var groundVel = Rigidbody.Velocity.WithZ( 0f );
		if ( verticalInput == 0f && groundVel.Length < 32f )
		{
			var z = Rigidbody.Velocity.z;
			Rigidbody.Velocity = (groundVel.Normal * (groundVel.Length * 0.1f)).WithZ( z );
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
