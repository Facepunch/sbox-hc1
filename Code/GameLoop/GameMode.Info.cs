﻿using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Facepunch;

public record GameModeInfo(
	[property: JsonIgnore] string Path,
	string Title,
	string Description );

partial class GameMode
{
	/// <summary>
	/// Gets all game modes referenced by a scene.
	/// </summary>
	public static IReadOnlyList<GameModeInfo> GetAll( SceneFile scene )
	{
		var list = new List<GameModeInfo>();

		foreach ( var go in scene.GameObjects )
		{
			GetAll( go, "", list );
		}

		return list;
	}

	public static IReadOnlyList<GameModeInfo> AllUnique
	{
		get
		{
			var list = new HashSet<GameModeInfo>();
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

	private static void GetAll( JsonObject go, string path, List<GameModeInfo> infos )
	{
		if ( !go.TryGetPropertyValue( "Enabled", out var enabledNode ) || enabledNode?.GetValue<bool>() is not true )
		{
			return;
		}

		if ( !go.TryGetPropertyValue( "Name", out var nameNode ) || nameNode?.GetValue<string>() is not {} name )
		{
			return;
		}

		path = $"{path}/{name}";

		// TODO: actually support prefab variables properly, instead of hard coding "Title" and "Description"

		string titleOverride = null;
		string descriptionOverride = null;

		if ( go.TryGetPropertyValue( "__Prefab", out var prefabNode )
			&& prefabNode?.GetValue<string>() is { } prefabPath
			&& ResourceLibrary.TryGet<PrefabFile>( prefabPath, out var prefabFile ) )
		{
			if ( go.TryGetPropertyValue( "__PrefabVariables", out var variables ) )
			{
				titleOverride = variables?["Title"]?.GetValue<string>();
				descriptionOverride = variables?["Description"]?.GetValue<string>();
			}

			go = prefabFile.RootObject;
		}

		if ( go.TryGetPropertyValue( "Components", out var componentsNode ) && componentsNode is JsonArray components )
		{
			foreach ( var component in components.Select( x => x.AsObject() ) )
			{
				if ( !component.TryGetPropertyValue( "__type", out var typeNode )
					|| !string.Equals( typeNode?.GetValue<string>(), typeof(GameMode).FullName, StringComparison.OrdinalIgnoreCase ) )
				{
					continue;
				}

				var info = (GameModeInfo)Json.FromNode( component, typeof(GameModeInfo) ) with { Path = path };

				info = info with
				{
					Title = titleOverride ?? info.Title,
					Description = descriptionOverride ?? info.Description
				};

				infos.Add( info );
			}
		}

		if ( !go.TryGetPropertyValue( "Children", out var childrenNode ) || childrenNode is not JsonArray children )
		{
			return;
		}

		foreach ( var child in children )
		{
			GetAll( child.AsObject(), path, infos );
		}
	}
}
