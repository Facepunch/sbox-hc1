namespace Facepunch;

/// <summary>
/// An extraction point, associated with a <see cref="CashPoint"/>
/// </summary>
public partial class ExtractionPoint : Component, ICustomMinimapIcon, Component.ITriggerListener
{
	/// <summary>
	/// Our cash point
	/// </summary>
	[Property] public CashPoint CashPoint { get; set; } 

	/// <summary>
	/// The extraction point's trigger.
	/// </summary>
	[Property] public Collider Trigger { get; set; }

	string IMinimapIcon.IconPath => "ui/minimaps/cashgrab/extract.png";

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
		return CashPoint.State == CashPoint.CashPointState.Open;
	}

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		// TODO: do we have the bag?
		// if so, delete it, do game state stuff..
	}
}
