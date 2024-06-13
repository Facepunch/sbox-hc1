namespace Facepunch;

partial class DroppedWeapon : IMinimapIcon
{
	[RequireComponent] public Spottable Spottable { get; private set; }

	string IMinimapIcon.IconPath => "ui/minimaps/icon-map_bomb.png";
	Vector3 IMinimapElement.WorldPosition => Transform.Position;

	bool IMinimapElement.IsVisible( IPawn viewer )
	{
		// only showing C4 right now
		if ( Spottable is not null )
		{
			if ( Spottable.IsSpotted || Spottable.WasSpotted )
				return true;
		}

		return viewer.Team == Team.Terrorist;
	}
}
