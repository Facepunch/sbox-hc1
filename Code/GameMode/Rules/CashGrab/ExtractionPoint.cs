using Sandbox.Events;

namespace Facepunch;

public record CashPointBagExtractedEvent( PlayerController Player, ExtractionPoint ExtractionPoint ) : IGameEvent;

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
		var tracker = GameMode.Instance.Get<CashPointTracker>();

		if ( CashPoint.State == CashPoint.CashPointState.Open )
		{
			var player = other.GameObject.Root.Components.Get<PlayerController>();
			if ( player.IsValid() )
			{
				var inventory = player.Inventory;

				if ( inventory.Has( tracker.Resource ) )
				{
					inventory.Remove( tracker.Resource );
					Scene.Dispatch( new CashPointBagExtractedEvent( player, this ) );
				}
			}
		}
	}
}
