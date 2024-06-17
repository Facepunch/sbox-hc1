namespace Facepunch;

/// <summary>
/// Behavior that makes a ping follow a player until they're no longer visible.
/// </summary>
public partial class PlayerPingReceiver : Component, IPingReceiver
{
	[RequireComponent] PlayerController Player { get; set; }

	private Vector3 _lastKnownPosition = Vector3.Zero;
	private bool _isCurrentlyLost = false;
	private bool _wasLost = false;

	string IPingReceiver.Text => "";
	string IPingReceiver.Icon => "ui/minimaps/enemy_icon.png";
	Color? IPingReceiver.Color => "#EE4B2B";

	bool IPingReceiver.ShouldShow() => !_isCurrentlyLost;

	Vector3 IPingReceiver.Position
	{
		get
		{
			var pos = Player.AimRay.Position;
			var isSpotted = Player.Spottable.IsSpotted;

			_isCurrentlyLost = !isSpotted;

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
