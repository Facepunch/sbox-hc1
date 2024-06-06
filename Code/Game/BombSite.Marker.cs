
using Facepunch;

partial class BombSite : IMinimapLabel, IMinimapVolume
{
	Vector3 IMinimapElement.WorldPosition => Transform.Position;
	bool IMinimapElement.IsVisible( PlayerController viewer ) => true;

	Color IMinimapVolume.Color => "rgb(255, 106, 0,0.25)";
	Vector3 IMinimapVolume.Size => Components.Get<BoxCollider>().Scale;

	string IMinimapLabel.Label => $"{(char)('A' + Index)}";
}
