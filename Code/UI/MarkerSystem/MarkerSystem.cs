namespace Facepunch.UI;

public partial class MarkerSystem : Panel
{
	public static MarkerSystem Instance { get; set; }

	public static HashSet<Marker> Markers { get; set; } = new();

	public MarkerSystem()
	{
		Instance = this;
	}

	public override void Tick()
	{
		foreach ( var marker in Markers )
		{
			marker.Reposition();
		}
	}

	public static void Register( IMarkerObject obj )
	{
		var marker = new Marker();
		marker.Object = obj;
		Markers.Add( marker );
		Instance.AddChild( marker );

		// Immediately repostion before tick
		marker.Reposition();
	}

	public static void UnRegister( IMarkerObject obj )
	{
		var found = Markers.FirstOrDefault( x => x.Object.GameObject == obj.GameObject );
		if ( found.IsValid() )
		{
			Markers.Remove( found );
			found.Delete( true );
		}
	}
}
