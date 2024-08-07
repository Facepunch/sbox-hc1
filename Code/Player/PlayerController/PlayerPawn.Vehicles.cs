using System.Text.Json.Serialization;

namespace Facepunch;

public partial class PlayerPawn
{
	[HostSync, Property, JsonIgnore] private VehicleSeat currentSeat { get; set; }

	// TODO: should some of this logic be in the seat/vehicle??
	public VehicleSeat CurrentSeat
	{
		get => currentSeat;

		set
		{
			if ( value.IsValid() )
			{
				GameObject.SetParent( value.GameObject, false );
				// Zero out our transform
				Transform.Local = new();
				ClearCurrentWeapon();
			}
			else
			{
				// Shoot the player up a bit, TODO: Exit points
				Transform.Local = new Transform().WithPosition( Vector3.Up * 100f );
				GameObject.SetParent( null, true );

				CharacterController.Velocity = currentSeat.Vehicle.Rigidbody.Velocity;

				SetCurrentEquipment( Inventory.Equipment.FirstOrDefault() );
			}

			currentSeat = value;
		}
	}

	public bool IsInVehicle => CurrentSeat.IsValid();

	private void ApplyVehicle()
	{
		// Shouldn't happen, but fuck it anyway
		if ( !CurrentSeat.IsValid() )
			return;

		// Improve this later
		if ( CurrentSeat.HasInput )
		{
			CurrentSeat.Vehicle.InputState.UpdateFromLocal();
		}
	}
}
