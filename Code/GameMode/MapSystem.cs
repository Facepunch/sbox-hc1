namespace Facepunch;


public static class MapSystem
{
	public record Map( SceneFile SceneFile );

	public static List<Map> All
	{
		get
		{
			// TODO: this should be configurable somewhere.
			var list = new List<Map>
			{
				new Map( ResourceLibrary.Get<SceneFile>( "scenes/maps/defusemap_01/de_garry_hammer.scene" ) ),
				new Map( ResourceLibrary.Get<SceneFile>( "scenes/maps/tdm_test/tdm_test.scene" ) ),
				new Map( ResourceLibrary.Get<SceneFile>( "scenes/maps/Shipment/shipment.scene" ) )
			};

			return list;
		}
	}
}
