namespace Facepunch;

/// <summary>
/// This is spawned clientside when a ping happens.
/// </summary>
public partial class WorldPingComponent : Component, IMarkerObject, IMinimapIcon
{
	/// <summary>
	/// Who's the owner?
	/// </summary>
	public PlayerState Owner { get; set; }

	private Vector3 MarkerPosition
	{
		get
		{
			if ( Receiver.IsValid() )
				return Receiver.Position;

			return Transform.Position + Vector3.Up * 32f;
		}
	}

	// IMarkerObject
	string IMarkerObject.MarkerIcon => "ui/marker.png";
	string IMarkerObject.DisplayText => Owner?.Network.OwnerConnection?.DisplayName ?? "";
	Vector3 IMarkerObject.MarkerPosition => MarkerPosition;

	// IMinimapIcon
	string IMinimapIcon.IconPath => "ui/marker.png";
	Vector3 IMinimapElement.WorldPosition => MarkerPosition;

	/// <summary>
	/// 
	/// </summary>
	public IPingReceiver Receiver { get; private set; }

	/// <summary>
	/// Are we pinging a world object that might move?
	/// </summary>
	public Component Target
	{
		set
		{
			var receiver = value.GameObject.Root.Components.Get<IPingReceiver>();
			if ( receiver.IsValid() )
			{
				Receiver = receiver;
			}
		}
	}

	// IMinimapElement
	bool IMinimapElement.IsVisible( IPawn viewer ) => true;

	/// <summary>
	/// Triggers the ping to tear off in X seconds.
	/// </summary>
	/// <param name="lifetime"></param>
	public void Trigger( float lifetime = 15f )
	{
		var destroy = Components.Create<TimedDestroyComponent>();
		destroy.Time = lifetime;

		Receiver?.OnPing();
	}
}
