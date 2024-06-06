
using Facepunch;

partial class BombSite : IMinimapLabel, IMinimapVolume
{
	Vector3 IMinimapElement.WorldPosition => Transform.Position;
	bool IMinimapElement.IsVisible( PlayerController viewer ) => true;

	Color IMinimapVolume.Color => "rgba( #992d32, 0.25 )";
	Vector3 IMinimapVolume.Size => Components.Get<BoxCollider>().Scale;
	Color IMinimapVolume.LineColor => new Color32( 183, 85, 70 );

	string IMinimapLabel.Label => $"{(char)('A' + Index)}";
	Color IMinimapLabel.LabelColor => new Color32( 183, 85, 70 );

}
