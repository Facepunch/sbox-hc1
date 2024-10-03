using Sandbox.Events;

namespace Facepunch;

public record CashPointBagExtractedEvent( PlayerPawn Player, ExtractionPoint ExtractionPoint ) : IGameEvent;

/// <summary>
/// An extraction point, associated with a <see cref="CashPoint"/>
/// </summary>
[EditorHandle( "ui/Minimaps/cashgrab/extract.png" )]
public partial class ExtractionPoint : Component,
	IMarkerObject,
	ICustomMinimapIcon, 
	Component.ITriggerListener, 
	IGameEventHandler<CashPoint.StateChangedEvent>
{
	/// <summary>
	/// Our cash point
	/// </summary>
	[Property] public CashPoint CashPoint { get; set; }

	/// <summary>
	/// the world panel
	/// </summary>
	[Property] public Sandbox.WorldPanel WorldPanel { get; set; }

	/// <summary>
	/// The extraction point's trigger.
	/// </summary>
	[Property] public Collider Trigger { get; set; }

	string IMinimapIcon.IconPath => "ui/minimaps/cashgrab/extract.png";

	Vector3 IMinimapElement.WorldPosition => WorldPosition;

	string ICustomMinimapIcon.CustomStyle
	{
		get
		{
			var color = Color.Green;
			return $"background-tint: {color.Hex};";
		}
	}

	bool ShouldShow()
	{
		if ( !CashPoint.IsValid() )
			return false;

		return CashPoint.State == CashPoint.CashPointState.Open;
	}

	bool IMinimapElement.IsVisible( Pawn viewer ) => ShouldShow();

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		// Conna: only the host should do this stuff.
		if ( !Networking.IsHost ) return;
		
		var tracker = GameMode.Instance.Get<CashPointTracker>();

		if ( CashPoint.State == CashPoint.CashPointState.Open )
		{
			var player = other.GameObject.Root.GetComponentInChildren<PlayerPawn>();
			if ( player.IsValid() )
			{
				var inventory = player.Inventory;
				if ( inventory.Has( tracker.Resource ) )
				{
					inventory.Remove( tracker.Resource );
					Scene.Dispatch( new CashPointBagExtractedEvent( player, this ) );
				}
			}

			// TODO: tag the dropped weapon's player
			//var cash = other.GameObject.Root.Components.Get<CashBag>( FindMode.EnabledInSelfAndDescendants );
			//if ( cash.IsValid() )
			//{
			//	cash.GameObject.Destroy();
			//	Scene.Dispatch( new CashPointBagExtractedEvent( player, this ) );
			//}
		}
	}

	/// <summary>
	/// Where is the marker?
	/// </summary>
	Vector3 IMarkerObject.MarkerPosition => WorldPosition + Vector3.Up * 32f;

	/// <summary>
	/// What icon?
	/// </summary>
	string IMarkerObject.MarkerIcon => "/ui/minimaps/cashgrab/extract.png";

	/// <summary>
	/// What text?
	/// </summary>
	string IMarkerObject.DisplayText => "Extraction Point";

	/// <summary>
	/// Should we show this marker?
	/// </summary>
	/// <returns></returns>
	bool IMarkerObject.ShouldShow() => ShouldShow();

	protected override void DrawGizmos()
	{
		Gizmo.Draw.Color = Color.Green;
		var box = BBox.FromPositionAndSize( Vector3.Zero, 64 );
		Gizmo.Hitbox.BBox( box );
		Gizmo.Draw.LineBBox( box );
	}

	void IGameEventHandler<CashPoint.StateChangedEvent>.OnGameEvent( CashPoint.StateChangedEvent eventArgs )
	{
		if ( WorldPanel.IsValid() )
			WorldPanel.Enabled = eventArgs.State == CashPoint.CashPointState.Open;
	}
}
