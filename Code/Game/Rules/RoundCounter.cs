
using System.Threading.Tasks;

public sealed class RoundCounter : Component, IGameStartListener, IRoundStartListener
{
	/// <summary>
	/// Current round number, starting at 1.
	/// </summary>
	public int Round { get; set; }

	public Task OnGameStart()
	{
		Round = 1;

		return Task.CompletedTask;
	}

	public Task OnRoundStart()
	{
		Round += 1;

		return Task.CompletedTask;
	}
}
