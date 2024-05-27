public sealed class RoundTimer : Component, IRoundStartListener, IRoundEndListener
{
	[Sync]
	public float StartTime { get; private set; }

	[Sync]
	public float FinalTime { get; private set; }

	[Sync]
	public bool IsCounting { get; private set; }

	public float TimeSeconds => IsCounting ? Time.Now - StartTime : FinalTime;

	void IRoundStartListener.PostRoundStart()
	{
		StartTime = Time.Now;
		FinalTime = 0f;
		IsCounting = true;
	}

	void IRoundEndListener.PreRoundEnd()
	{
		FinalTime = TimeSeconds;
		IsCounting = false;
	}
}
