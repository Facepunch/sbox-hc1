namespace Facepunch;

/// <summary>
/// Lets us track the cash bag even when it gets dropped.
/// </summary>
public partial class CashBag : Component, IDroppedWeaponState<CashBag>, ICustomMinimapIcon, IMarkerObject
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

	bool IMinimapElement.IsVisible( Pawn viewer )
	{
		return true;
	}

	/// <summary>
	/// Where is the marker?
	/// </summary>
	Vector3 IMarkerObject.MarkerPosition => Transform.Position + Vector3.Up * 32f;

	/// <summary>
	/// What icon?
	/// </summary>
	string IMarkerObject.MarkerIcon => "/ui/minimaps/cashgrab/cashpoint.png";

	/// <summary>
	/// What text?
	/// </summary>
	string IMarkerObject.DisplayText => "Cash";
}
