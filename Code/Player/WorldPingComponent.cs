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

	// IMarkerObject
	string IMarkerObject.MarkerIcon => "ui/marker.png";
	string IMarkerObject.DisplayText => Owner?.Network.OwnerConnection?.DisplayName ?? "";
	Vector3 IMarkerObject.MarkerPosition => Transform.Position + Vector3.Up * 32f;

	// IMinimapIcon
	string IMinimapIcon.IconPath => "ui/marker.png";
	Vector3 IMinimapElement.WorldPosition => Transform.Position + Vector3.Up * 32f;

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
	}
}
