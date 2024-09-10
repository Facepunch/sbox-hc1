namespace Facepunch;

partial class DroppedEquipment : IMinimapIcon
{
	[RequireComponent] public Spottable Spottable { get; private set; }

	string IMinimapIcon.IconPath => "ui/minimaps/icon-map_bomb.png";
	Vector3 IMinimapElement.WorldPosition => Transform.Position;

	bool IMinimapElement.IsVisible( Pawn viewer )
	{
		if ( Resource.Slot != EquipmentSlot.Special )
			return false;

		// only showing C4 right now
		if ( Spottable is not null )
		{
			if ( Spottable.IsSpotted || Spottable.WasSpotted )
				return true;
		}

		// TODO: Restore
		return false;
		//return viewer.Team == Team.Terrorist;
	}
}
