using Sandbox;

namespace Facepunch;

public sealed class MapInformation : Component, ISceneMetadata
{
	/// <summary>
	/// The map's image
	/// </summary>
	[Property, ImageAssetPath] 
	public string Image { get; set; }

	[Property]
	public bool IsVisibleInMenu { get; set; } = true;

	Dictionary<string, string> ISceneMetadata.GetMetadata()
	{
		return new()
		{
			{ "Image", Image },
			{ "IsVisibleInMenu", IsVisibleInMenu.ToString() }
		};
	}
}
