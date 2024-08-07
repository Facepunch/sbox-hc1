using System.Text.Json.Serialization;

namespace Facepunch;

public partial class PlayerPawn
{
	[HostSync, Property, JsonIgnore] public VehicleSeat CurrentSeat { get; set; }

	public TimeSince TimeSinceSeatChanged { get; set; } = 0;


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
		
		if ( Input.Pressed( "Use" ) )
		{
			// Try leaving the seat
			CurrentSeat.Leave( this );
		}
	}
}
