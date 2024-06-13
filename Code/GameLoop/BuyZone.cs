
namespace Facepunch;

internal class BuyZone : Component, ICustomMinimapIcon, IMinimapVolume
{
	[Property]
	public Team Team { get; set; }

	public Color Color => $"rgba( {Team.GetColor().Rgba}, 0.10)";
	public Color LineColor => $"rgba({Team.GetColor().Rgba}, 0.5)";

	public Vector3 Size => Components.Get<BoxCollider>().Scale;

	MinimapIconType IMinimapIcon.IconType => MinimapIconType.Buyzone;
	Vector3 IMinimapElement.WorldPosition => Transform.Position;
	string ICustomMinimapIcon.CustomStyle => $"background-image: url( '{Team.GetIconPath()}' );";

	bool IMinimapElement.IsVisible( IPawn viewer ) => true;
}
