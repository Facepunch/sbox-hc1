namespace Facepunch;


public static class MapSystem
{
	public static IEnumerable<MapDefinition> All
	{
		get
		{
			return ResourceLibrary.GetAll<MapDefinition>().Where( x => x.IsEnabled );
		}
	}
}
