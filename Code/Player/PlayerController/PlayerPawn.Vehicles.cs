using System.Text.Json.Serialization;

namespace Facepunch;

public partial class PlayerPawn
{
	[HostSync, Property, JsonIgnore, Change( nameof( OnSeatChanged ))] public VehicleSeat CurrentSeat { get; set; }

	void OnSeatChanged( VehicleSeat before, VehicleSeat after )
	{
		if ( after.IsValid() )
		{
			GameObject.SetParent( after.GameObject, false );
			// Zero out our transform
			Transform.Local = new();
			ClearCurrentWeapon();
		}
		else
		{
			// Shoot the player up a bit
			GameObject.SetParent( null, true );

			// Move player to best exit point
			Transform.Position = before.FindExitLocation();
			CharacterController.Velocity = before.Vehicle.Rigidbody.Velocity;

			SetCurrentEquipment( Inventory.Equipment.FirstOrDefault() );
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
