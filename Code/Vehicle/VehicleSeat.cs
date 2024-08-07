namespace Facepunch;

public sealed class VehicleSeat : Component
{
	[Property] public Vehicle Vehicle { get; set; }
	[Property] public bool HasInput { get; set; } = true;

	public PlayerPawn Player { get; private set; }

	public bool CanEnter( PlayerPawn player )
	{
		return !Player.IsValid();
	}

	public bool Enter( PlayerPawn player )
	{
		if ( !CanEnter( player ) )
		{
			return false;
		}

		Player = player;
		player.CurrentSeat = this;

		return true;
	}

	public bool CanLeave( PlayerPawn player )
	{
		if ( Player != player ) return false;

		return true;
	}

	public bool Leave( PlayerPawn player )
	{
		if ( !CanLeave( player ) )
		{
			return false;
		}

		Player.CurrentSeat = null;
		Player = null;

		return true;
	}
}
