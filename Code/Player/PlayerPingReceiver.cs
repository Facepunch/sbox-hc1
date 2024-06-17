namespace Facepunch;

/// <summary>
/// Behavior that makes a ping follow a player until they're no longer visible.
/// </summary>
public partial class PlayerPingReceiver : Component, IPingReceiver
{
	[RequireComponent] PlayerController Player { get; set; }

	private Vector3 _lastKnownPosition = Vector3.Zero;
	private bool _wasLost = false;

	Vector3 IPingReceiver.Position
	{
		get
		{
			var pos = Player.AimRay.Position;
			var isSpotted = Player.Spottable.IsSpotted;

			if ( !isSpotted && !_wasLost )
				_wasLost = true;

			if ( !isSpotted || _wasLost ) 
				return _lastKnownPosition;

			pos += Vector3.Up * 32f;
			_lastKnownPosition = pos;

			return pos;
		}
	}

	void IPingReceiver.OnPing()
	{
		_wasLost = false;
		_lastKnownPosition = Vector3.Zero;
	}
}
