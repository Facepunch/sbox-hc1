namespace Facepunch;

public sealed class MapInformation : Component, ISceneMetadata
{
	/// <summary>
	/// The map's image
	/// </summary>
	[Property, ImageAssetPath]
	public string Image { get; set; }

	/// <summary>
	/// Is this map visible from in the menu?
	/// </summary>
	[Property]
	public bool IsVisibleInMenu { get; set; } = true;

	/// <summary>
	/// Gets all gameobjects, gets the gamemodes, saves their paths as meta
	/// </summary>
	/// <returns></returns>
	private List<string> GetGameModeList()
	{
		var list = new List<string>();
		foreach ( var go in Scene.GetAllObjects( false ) )
		{
			var gm = go.GetComponent<GameMode>( true );
			if ( !gm.IsValid() ) continue;
			list.Add( go.PrefabInstanceSource );
		}

		return list;
	}

	Dictionary<string, string> ISceneMetadata.GetMetadata()
	{
		return new()
		{
			{ "Image", Image },
			{ "IsVisibleInMenu", IsVisibleInMenu.ToString() },
			{ "GameModes", string.Join( ", ", GetGameModeList() ) }
		};
	}
}
