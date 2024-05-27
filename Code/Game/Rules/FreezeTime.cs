using System.Threading.Tasks;

public sealed class FreezeTime : Component, IRoundStartListener
{
	[Property] public float DurationSeconds { get; set; } = 15f;

	public Task OnRoundStart()
	{
		return Task.DelaySeconds( DurationSeconds );
	}
}
