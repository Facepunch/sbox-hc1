
using Facepunch;

/// <summary>
/// End the round if one team is eliminated.
/// </summary>
public sealed class TeamEliminated : Component, IRoundStartListener, IRoundEndCondition
{
	private bool _bothTeamsHadPlayers;

	void IRoundStartListener.PostRoundStart()
	{
		_bothTeamsHadPlayers = GameUtils.GetPlayers( Team.CounterTerrorist ).Any()
			&& GameUtils.GetPlayers( Team.Terrorist ).Any();
	}

	public bool ShouldRoundEnd()
	{
		if ( !_bothTeamsHadPlayers && !GameUtils.InactivePlayers.Any() )
		{
			// Let you test stuff in single player
			return false;
		}

		return GameUtils.GetPlayers( Team.CounterTerrorist ).All( x => x.HealthComponent.State == LifeState.Dead )
			|| GameUtils.GetPlayers( Team.Terrorist ).All( x => x.HealthComponent.State == LifeState.Dead );
	}
}
