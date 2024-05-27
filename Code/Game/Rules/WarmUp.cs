
using System.Threading.Tasks;

internal class WarmUp : Component, IGameStartListener
{
	[Property, Sync]
	public float TotalTimeSeconds { get; set; }

	public Task OnGameStart()
	{
		return Task.DelaySeconds( TotalTimeSeconds );
	}
}
