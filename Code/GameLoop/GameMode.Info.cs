namespace Facepunch;

partial class GameMode
{
	/// <summary>
	/// Gets all game modes referenced by a scene.
	/// </summary>
	public static IEnumerable<GameMode> GetAll( SceneFile scene )
	{
		return GetGameModesForScene( scene );
	}

	private static IEnumerable<GameMode> GetGameModesForScene( SceneFile scene )
	{
		var list = new HashSet<GameMode>();
		var meta = scene.GetMetadata( "GameModes", "" );

		if ( !string.IsNullOrEmpty( meta ) )
		{
			var files = meta.Split( ", " );

			foreach ( var file in files )
			{
				var prefab = GameObject.GetPrefab( file );
				if ( prefab.IsValid() )
				{
					list.Add( prefab.GetComponent<GameMode>() );
				}
			}
		}
		return list;
	}

	public static IReadOnlyList<GameMode> AllUnique
	{
		get
		{
			var list = new HashSet<GameMode>();
			var scenes = GameUtils.GetAvailableMaps();

			foreach ( var scene in scenes )
			{
				var modes = GetAll( scene );
				foreach ( var mode in modes )
				{
					list.Add( mode );
				}
			}

			return list.ToList()
				.AsReadOnly();
		}
	}
}
