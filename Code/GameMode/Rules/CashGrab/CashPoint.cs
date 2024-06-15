namespace Facepunch;

public partial class CashPoint : Component, ICustomMinimapIcon
{
	/// <summary>
	/// A reference to all cash points on the map.
	/// </summary>
	public static List<CashPoint> All { get; private set; } = new();

	/// <summary>
	/// Is this cash point active?
	/// </summary>
	[HostSync] public bool IsActive { get; private set; }

	/// <summary>
	/// The resource to spawn.
	/// </summary>
	[Property] public EquipmentResource Resource { get; set; }

	/// <summary>
	/// Extractions associated with this cash point.
	/// </summary>
	[Property] public List<ExtractionPoint> Extracts { get; set; }

	string IMinimapIcon.IconPath => "ui/minimaps/cashgrab/cashpoint.png";

	Vector3 IMinimapElement.WorldPosition => Transform.Position;

	string ICustomMinimapIcon.CustomStyle
	{
		get
		{
			var color = IsActive ? Color.Green : Color.White;
			return $"background-tint: {color.Hex};";
		}
	}

	bool IMinimapElement.IsVisible( IPawn viewer )
	{
		return true;
	}

	public CashPoint()
	{
		All.Add( this );
	}

	/// <summary>
	/// activates this cash point
	/// </summary>
	public void Activate()
	{
		// TODO: track the cash!
		var eq = DroppedEquipment.Create( Resource, Transform.Position );
	}

	public void Cleanup()
	{
		// TODO: kill the cash!
		IsActive = false;
	}
}
