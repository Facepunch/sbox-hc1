namespace Facepunch;

partial class DroppedWeapon : IMinimapIcon
{
	[RequireComponent] public Spottable Spottable { get; private set; }

	[HostSync] public MinimapIconType IconType {  get; private set; }

	Vector3 IMinimapElement.WorldPosition => Transform.Position;

	bool IMinimapElement.IsVisible( PlayerController viewer )
	{
		// only showing C4 right now
		if ( Spottable is not null )
		{
			if ( Spottable.IsSpotted || Spottable.WasSpotted )
				return true;
		}

		return viewer.TeamComponent.Team == Team.Terrorist;
	}
}
