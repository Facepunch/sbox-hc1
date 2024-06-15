using Sandbox;
using Sandbox.Events;

namespace Facepunch;

public sealed class CashPointTracker : Component, IGameEventHandler<EquipmentDroppedEvent>, IGameEventHandler<EquipmentPickedUpEvent>
{
	[Property] public EquipmentResource Resource { get; set; }

	public HashSet<CashPoint> All { get; set; } = new();
	public CashPoint Last { get; set; }
	public CashPoint Current { get; set; }

	/// <summary>
	/// Who is holding the bag RIGHT now?
	/// </summary>
	public PlayerController Holder { get; set; }


	protected override void OnStart()
	{
		foreach ( var cashPoint in Scene.GetAllComponents<CashPoint>() )
		{
			All.Add( cashPoint );
		}
	}

	/// <summary>
	/// Called when someone drops a piece of equipment.
	/// </summary>
	/// <param name="eventArgs"></param>
	void IGameEventHandler<EquipmentDroppedEvent>.OnGameEvent( EquipmentDroppedEvent eventArgs )
	{
		if ( eventArgs.Dropped.Resource != Resource )
			return;

		if ( eventArgs.Player == Holder )
		{
			Holder = null;
		}
	}

	/// <summary>
	/// Called when someone picks up a piece of equipment
	/// </summary>
	/// <param name="eventArgs"></param>
	void IGameEventHandler<EquipmentPickedUpEvent>.OnGameEvent( EquipmentPickedUpEvent eventArgs )
	{
		if ( eventArgs.Dropped.Resource != Resource )
			return;

		Holder = eventArgs.Player;
	}
}
