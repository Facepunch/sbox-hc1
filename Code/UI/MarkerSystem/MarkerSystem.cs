namespace Facepunch.UI;

public partial class MarkerSystem : Panel
{
	public Dictionary<IMarkerObject, Marker> ActiveMarkers { get; set; } = new();

	void Refresh()
	{
		var deleteList = new List<IMarkerObject>();
		deleteList.AddRange( ActiveMarkers.Keys );

		foreach ( var markerObject in Scene.GetAllComponents<IMarkerObject>() )
		{
			if ( UpdateMarker( markerObject ) )
			{
				deleteList.Remove( markerObject );
			}
		}

		foreach ( var marker in deleteList )
		{
			ActiveMarkers[marker].Delete();
			ActiveMarkers.Remove( marker );
		}
	}

	public Marker CreateMarker( IMarkerObject marker )
	{
		var inst = new Marker()
		{
			Object = marker,
		};
		AddChild( inst );
		return inst;
	}

	public bool UpdateMarker( IMarkerObject marker )
	{
		if ( !marker.GameObject.IsValid() )
			return false;

		if ( !marker.ShouldShow() )
			return false;

		var camera = Scene.Camera;
		if ( marker.MarkerMaxDistance != 0f && camera.Transform.Position.Distance( marker.MarkerPosition ) > marker.MarkerMaxDistance )
			return false;

		if ( !ActiveMarkers.TryGetValue( marker, out var instance ) )
		{
			instance = CreateMarker( marker );
			if ( instance.IsValid() )
			{
				ActiveMarkers[marker] = instance;
			}
		}

		instance.Reposition();

		return true;
	}

	public override void Tick()
	{
		Refresh();
	}
}
