using Sandbox;
using Sandbox.Events;

namespace Facepunch;

public sealed class BuyZoneTime : Component,
	IGameEventHandler<DuringRoundStartEvent>,
	IGameEventHandler<PostRoundStartEvent>
{
	[Property] public float BuyTime { get; set; } = 30;
	[HostSync] public RealTimeUntil TimeUntilCannotBuy { get; private set; } = 60;

	void IGameEventHandler<DuringRoundStartEvent>.OnGameEvent( DuringRoundStartEvent eventArgs )
	{
		TimeUntilCannotBuy = BuyTime;
	}

	void IGameEventHandler<PostRoundStartEvent>.OnGameEvent( PostRoundStartEvent eventArgs )
	{
		TimeUntilCannotBuy = BuyTime;
	}

	public bool CanBuy()
	{
		return !TimeUntilCannotBuy;
	}
}
