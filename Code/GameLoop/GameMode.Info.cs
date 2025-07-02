using System.Text.Json.Nodes;

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

	private static bool ShouldIgnoreGameObject( JsonObject json )
	{
		if ( json.TryGetPropertyValue( "__Prefab", out var node ) )
		{
			return false;
		}

		return true;
	}

	private static IEnumerable<GameMode> GetGameModesForScene( SceneFile scene )
	{
		var list = new HashSet<GameMode>();

		foreach ( var json in scene.GameObjects.Where( x => !ShouldIgnoreGameObject( x ) ) )
		{
			var go = new GameObject( false );
			go.Flags |= GameObjectFlags.NotSaved;
			go.SetParent( null, true );
			go.Deserialize( json );

			var x = json.TryGetPropertyValue( "__Prefab", out var pathNode );
			var path = pathNode.ToString();

			if ( go.GetComponent<GameMode>( true ) is not null )
			{
				var prefab = GameObject.GetPrefab( path );
				var gm = prefab.GetComponent<GameMode>();
				list.Add( gm );
			}

			go.DestroyImmediate();
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
