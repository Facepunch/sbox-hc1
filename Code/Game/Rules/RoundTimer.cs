
using System.Threading.Tasks;

public sealed class RoundTimer : Component, IRoundStartListener
{
	public TimeSince SinceRoundStart { get; private set; }

	public Task OnRoundStart()
	{
		SinceRoundStart = 0f;

		return Task.CompletedTask;
	}
}
