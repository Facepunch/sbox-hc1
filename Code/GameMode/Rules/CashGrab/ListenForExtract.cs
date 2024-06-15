using Sandbox.Events;

namespace Facepunch;

public sealed class ListenForExtract : Component, IGameEventHandler<CashPointBagExtractedEvent>
{
	[Property] public StateComponent ToState { get; set; }

	/// <summary>
	/// How much is the cash reward?
	/// </summary>
	[Property] public int CashReward { get; set; } = 10000;

	void IGameEventHandler<CashPointBagExtractedEvent>.OnGameEvent( CashPointBagExtractedEvent eventArgs )
	{
		var team = eventArgs.Player.TeamComponent.Team;
		var teamScoring = GameMode.Instance.Get<TeamScoring>();
		teamScoring.IncrementScore( team, CashReward );

		GameMode.Instance.StateMachine.Transition( ToState );
	}
}
