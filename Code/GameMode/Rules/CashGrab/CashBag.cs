namespace Facepunch;

/// <summary>
/// Lets us track the cash bag even when it gets dropped.
/// </summary>
public partial class CashBag : Component, IDroppedWeaponState<CashBag>, ICustomMinimapIcon
{
	string IMinimapIcon.IconPath => "ui/minimaps/cashgrab/cashpoint.png";
	Vector3 IMinimapElement.WorldPosition => Transform.Position;
	string ICustomMinimapIcon.CustomStyle
	{
		get
		{
			var color = Color.Green;
			return $"background-tint: {color.Hex};";
		}
	}

	bool IMinimapElement.IsVisible( IPawn viewer )
	{
		return true;
	}
}
