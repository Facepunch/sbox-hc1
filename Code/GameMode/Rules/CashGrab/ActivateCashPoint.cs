using Sandbox.Events;

namespace Facepunch;

public partial class ActivateCashPoint : Component, IGameEventHandler<EnterStateEvent>, IGameEventHandler<EquipmentPickedUpEvent>
{
	[Property] public StateComponent PickedUpState { get; set; }

	public void OnGameEvent( EnterStateEvent eventArgs )
	{
		if ( !Networking.IsHost )
			return;

		var cashPoint = GameMode.Instance.Get<CashPointTracker>().Current;
		cashPoint.Activate();
	}

	void IGameEventHandler<EquipmentPickedUpEvent>.OnGameEvent( EquipmentPickedUpEvent eventArgs )
	{
		var tracker = GameMode.Instance.Get<CashPointTracker>();

		if ( eventArgs.Dropped.Resource == tracker.Resource )
		{
			GameMode.Instance.StateMachine.Transition( PickedUpState );
		}
	}
}
