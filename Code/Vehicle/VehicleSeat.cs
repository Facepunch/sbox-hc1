using System.Diagnostics.Metrics;

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

	private void RpcEnter( PlayerPawn player )
	{
		if ( !Networking.IsHost )
			return;

		Player = player;
		player.CurrentSeat = this;

		player.GameObject.SetParent( GameObject, false );
		// Zero out our transform
		player.Transform.Local = new();
		player.CurrentEquipment?.Holster();

		if ( HasInput )
			Network.AssignOwnership( Player.Network.OwnerConnection );
	}

	public bool Enter( PlayerPawn player )
	{
		if ( !CanEnter( player ) )
		{
			return false;
		}

		using ( Rpc.FilterInclude( Connection.Host ) )
			RpcEnter( player );

		return true;
	}

	public bool CanLeave( PlayerPawn player )
	{
		if ( !Player.IsValid() ) return false;
		if ( Player != player ) return false;

		return true;
	}

	private void RpcLeave()
	{
		if ( !Networking.IsHost )
			return;

		Player.GameObject.SetParent( null, true );

		// Move player to best exit point
		Transform.Position = FindExitLocation();
		Player.CharacterController.Velocity = Vehicle.Rigidbody.Velocity;

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

		using ( Rpc.FilterInclude( Connection.Host ) )
			RpcLeave();

		return true;
	}

	public Vector3 FindExitLocation()
	{
		// TODO: Multiple volumes (e.g. fallback)
		return ExitVolumes[0].CheckClosestFreeSpace( Transform.Position );
	}

	internal void Eject()
	{
		Leave( Player );
	}
}
