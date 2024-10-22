namespace Facepunch;

[GameResource( "HC1/Map Definition", "map", "A map definition for HC1." )]
public partial class MapDefinition : GameResource
{
	[Property] public bool IsEnabled { get; set; } = true;
	[Property] public string Title { get; set; }
	[Property] public string Description { get; set; }
	[Property] public SceneFile SceneFile { get; set; }
	[Property, ImageAssetPath] public string Background { get; set; }

	public static IEnumerable<MapDefinition> All
	{
		get
		{
			return ResourceLibrary.GetAll<MapDefinition>().Where( x => x.IsEnabled );
		}
	}
}
