public sealed class RoundTimer : Component, IRoundStartListener, IRoundEndListener
{
	private TimeSince _sinceStart;
	private float _endTime;
	private bool _counting;

	public float TimeSeconds => _counting ? _sinceStart : _endTime;

	void IRoundStartListener.PostRoundStart()
	{
		_sinceStart = 0f;
		_endTime = 0f;
		_counting = true;
	}

	void IRoundEndListener.PreRoundEnd()
	{
		_endTime = _sinceStart;
		_counting = false;
	}
}
