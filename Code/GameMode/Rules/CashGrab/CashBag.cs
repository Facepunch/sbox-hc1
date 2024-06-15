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

	bool IMinimapElement.IsVisible( IPawn viewer )
	{
		return true;
	}

	/// <summary>
	/// Temporary marker just to make it obvious there's a grenade
	/// </summary>
	MarkerFrame IMarkerObject.MarkerFrame => new MarkerFrame()
	{
		DisplayText = null,
		Position = Transform.Position + Vector3.Up * 32f,
		Rotation = Rotation.Identity,
		MaxDistance = 4096,
	};


	/// <summary>
	/// Custom marker panel
	/// </summary>
	Type IMarkerObject.MarkerPanelTypeOverride => typeof( UI.CashBagMarkerPanel );
}
