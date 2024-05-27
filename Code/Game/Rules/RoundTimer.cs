public sealed class RoundTimer : Component, IRoundStartListener
{
	[Property]
	public TimeSince SinceRoundStart { get; private set; }

	void IRoundStartListener.PostRoundStart()
	{
		SinceRoundStart = 0f;
	}
}
