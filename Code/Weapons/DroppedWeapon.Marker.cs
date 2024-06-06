namespace Facepunch;

partial class DroppedWeapon : IMinimapIcon
{
	[HostSync] public MinimapIconType IconType {  get; private set; }

	Vector3 IMinimapElement.WorldPosition => Transform.Position;

	bool IMinimapElement.IsVisible( PlayerController viewer )
	{
		// only showing C4 right now
		// todo: or has been seen by CTs?
		return viewer.TeamComponent.Team == Team.Terrorist;
	}
}
