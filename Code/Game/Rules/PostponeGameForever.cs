using Facepunch;
using System.Threading.Tasks;
using Facepunch.UI;

/// <summary>
/// Never start the game, useful for testing weapons.
/// </summary>
public sealed class PostponeGameForever : Component, IGameStartListener
{
	async Task IGameStartListener.OnGameStart()
	{
		while ( true )
		{
			await Task.DelaySeconds( 1f );
		}
	}

	protected override void OnUpdate()
	{
		if ( GameMode.Instance.State != GameState.PreGame )
		{
			return;
		}

		if ( GameUtils.GetHudPanel<RoundStateDisplay>() is { } display )
		{
			display.Status = "Testing Mode";
			display.Time = null;
		}
	}
}
