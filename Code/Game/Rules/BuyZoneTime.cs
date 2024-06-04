using Sandbox;

namespace Facepunch;

public sealed class BuyZoneTime : Component, IRoundStartListener
{
	[Property] public float BuyTime { get; set; } = 30;
	[HostSync] public RealTimeUntil TimeUntilCannotBuy { get; private set; } = 60;

	void IRoundStartListener.PostRoundStart()
	{
		TimeUntilCannotBuy = BuyTime;
	}

	void IRoundStartListener.PreRoundStart()
	{
		TimeUntilCannotBuy = BuyTime;
	}

	public bool CanBuy()
	{
		return !TimeUntilCannotBuy;
	}
}
