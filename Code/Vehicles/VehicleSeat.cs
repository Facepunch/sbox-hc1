namespace Facepunch;

public sealed class VehicleSeat : Component
{
	[Property] public Vehicle Vehicle { get; set; }
	[Property] public bool HasInput { get; set; } = true;
	[Property] public List<VehicleExitVolume> ExitVolumes { get; set; }

	[HostSync] public PlayerPawn Player { get; private set; }

	public bool CanEnter( PlayerPawn player )
	{
		return !Player.IsValid();
	}

	[Broadcast]
	private void BroadcastEnteredVehicle( PlayerPawn player )
	{
		player.GameObject.SetParent( GameObject, false );

		// Zero out our transform
		player.LocalPosition = Vector3.Zero;
		player.LocalRotation = Rotation.Identity;
	}

	[Broadcast]
	private void RpcEnter( PlayerPawn player )
	{
		if ( !Networking.IsHost )
			return;

		Player = player;
		player.CurrentSeat = this;

		BroadcastEnteredVehicle( player );

		if ( player.CurrentEquipment.IsValid() )
			player.CurrentEquipment?.Holster();

		if ( HasInput )
			Network.AssignOwnership( player.Network.Owner );
	}

	public bool Enter( PlayerPawn player )
	{
		if ( !CanEnter( player ) )
		{
			return false;
		}

		Log.Info( "Trying to enter a vehicle" );

		player.GameObject.SetParent( GameObject, false );
		player.TimeSinceSeatChanged = 0;

		// Zero out our transform
		player.LocalPosition = Vector3.Zero;
		player.LocalRotation = Rotation.Identity;

		using ( Rpc.FilterInclude( Connection.Host ) )
			RpcEnter( player );

		return true;
	}

	public bool CanLeave( PlayerPawn player )
	{
		if ( !Player.IsValid() ) return false;
		if ( Player.TimeSinceSeatChanged < 0.5f ) return false;

		if ( Player != player ) return false;

		return true;
	}

	[Broadcast]
	private void RpcLeave()
	{
		if ( !Networking.IsHost )
			return;

		Player.SetCurrentEquipment( Player.Inventory.Equipment.FirstOrDefault() );

		Player.CurrentSeat = null;
		Player = null;

		if ( HasInput )
			Network.DropOwnership();
	}

	public bool Leave( PlayerPawn player )
	{
		if ( !CanLeave( player ) )
		{
			return false;
		}

		player.GameObject.SetParent( null, true );

		// Move player to best exit point
		player.WorldPosition = FindExitLocation();
		player.CharacterController.Velocity = Vehicle.Rigidbody.Velocity;

		using ( Rpc.FilterInclude( Connection.Host ) )
			RpcLeave();

		return true;
	}

	public Vector3 FindExitLocation()
	{
		// TODO: Multiple volumes (e.g. fallback)
		return ExitVolumes[0].CheckClosestFreeSpace( WorldPosition );
	}

	internal void Eject()
	{
		Leave( Player );
	}
}
