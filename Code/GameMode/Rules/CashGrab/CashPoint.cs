
namespace Facepunch;

[EditorHandle( "ui/Minimaps/cashgrab/cashpoint.png" )]
public partial class CashPoint : Component, ICustomMinimapIcon, IMarkerObject
{
	public enum CashPointState
	{
		Inactive,
		Spawning,
		Open
	}

	/// <summary>
	/// Is this cash point active?
	/// </summary>
	[Property] public CashPointState State { get; set; } = CashPointState.Inactive;

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
			var color = State == CashPointState.Open ? Color.Green : Color.White;
			return $"background-tint: {color.Hex};";
		}
	}

	bool IMinimapElement.IsVisible( IPawn viewer )
	{
		var vis = State == CashPointState.Spawning;
		return vis;
	}

	/// <summary>
	/// activates this cash point
	/// </summary>
	public void Activate()
	{
		State = CashPointState.Open;

		// TODO: track the cash!
		var eq = DroppedEquipment.Create( Resource, Transform.Position );
		var cashBag = eq.Components.GetOrCreate<CashBag>();
	}

	/// <summary>
	/// marks this cash point as to be spawning something soon
	/// </summary>
	public void SetSpawning()
	{
		State = CashPointState.Spawning;
	}

	/// <summary>
	/// deactivates this cash point
	/// </summary>
	public void Deactivate()
	{
		// TODO: kill the cash!
		State = CashPointState.Inactive;
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
	string IMarkerObject.DisplayText => "Cash Point";

	/// <summary>
	/// Should we show this marker?
	/// </summary>
	/// <returns></returns>
	bool IMarkerObject.ShouldShow() => State == CashPointState.Spawning;

	protected override void DrawGizmos()
	{
		Gizmo.Draw.Color = Color.Green;
		var box = BBox.FromPositionAndSize( Vector3.Zero, 64 );
		Gizmo.Hitbox.BBox( box );
		Gizmo.Draw.LineBBox( box );

		Gizmo.Transform = new Transform();

		foreach ( var extract in Extracts )
		{
			Gizmo.Draw.Line( Transform.Position, extract.Transform.Position );
		}
	}
}
