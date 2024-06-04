namespace Facepunch.UI;

public partial class MarkerSystem : Panel
{
	public static MarkerSystem Instance { get; set; }

	public static HashSet<Marker> Markers { get; set; } = new();


	public MarkerSystem()
	{
		Instance = this;
		RegisterAll();
	}

	public override void Tick()
	{
		foreach ( var marker in Markers )
		{
			marker.Reposition();
		}
	}

	/// <summary>
	/// We might make this piece of UI too late, so fetch all the markers
	/// </summary>
	private static void RegisterAll()
	{
		foreach ( var obj in Game.ActiveScene.GetAllComponents<IMarkerObject>() )
		{
			Register( obj );
		}
	}

	public static void Register( IMarkerObject obj )
	{
		if ( Instance is null )
			return;

		if ( Markers.FirstOrDefault( x => x.Object == obj ) is { } found )
			return;

		var marker = Instance.AddChild<Marker>();
		if ( marker.IsValid() )
		{
			Log.Info( marker );
			marker.Object = obj;
			Markers.Add( marker );
			// Immediately repostion before tick
			marker.Reposition();
		}
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
