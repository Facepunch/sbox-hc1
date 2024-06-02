using Sandbox;

namespace Facepunch;

public sealed class BuyZoneTime : Component, IRoundStartListener
{
	[Property] public float BuyTime { get; set; } = 30;

	RealTimeUntil TimeUntilCannotBuy = 0;

	void IRoundStartListener.PostRoundStart()
	{
		TimeUntilCannotBuy = BuyTime;
	}

	public bool CanBuy()
	{
		return !TimeUntilCannotBuy;
	}
}
