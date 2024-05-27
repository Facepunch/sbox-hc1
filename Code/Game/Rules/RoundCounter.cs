
using System.Threading.Tasks;

public sealed class RoundCounter : Component, IGameStartListener, IRoundStartListener
{
	/// <summary>
	/// Current round number, starting at 1.
	/// </summary>
	[Property, Sync]
	public int Round { get; set; }

	void IGameStartListener.PreGameStart()
	{
		Round = 0;
	}

	void IRoundStartListener.PreRoundStart()
	{
		Round += 1;
	}
}
