
namespace Facepunch;

internal class BuyZone : Component, IMinimapIcon, IMinimapVolume
{
	[Property]
	public Team Team { get; set; }

	public Color Color => $"rgba( {Team.GetColor().Rgba}, 0.10)";
	public Color LineColor => $"rgba({Team.GetColor().Rgba}, 0.5)";

	public Vector3 Size => Components.Get<BoxCollider>().Scale;

	string IMinimapIcon.IconPath => Team.GetIconPath();

	int IMinimapIcon.IconOrder => 15;

	Vector3 IMinimapElement.WorldPosition => Transform.Position;

	bool IMinimapElement.IsVisible( IPawn viewer ) => true;
}
