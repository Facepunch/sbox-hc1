using Facepunch.UI;
using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Transition to a given state after a given number of rounds. Requires a <see cref="RoundCounter"/>.
/// </summary>
public sealed class RoundLimit : Component,
	IGameEventHandler<RoundCounterIncrementedEvent>
{
	[Property, Sync( SyncFlags.FromHost )] public int MaxRounds { get; set; } = 30;

	[Property] public StateComponent HalfTimeState { get; set; }
	[Property] public StateComponent NextState { get; set; }

	[Property, HideIf( nameof(HalfTimeState), null )]
	public string FinalRoundFirstHalfMessage { get; set; } = "Final round of the first half";
	[Property] public string FinalRoundMessage { get; set; } = "Final round";

	public int FirstHalfRounds => HalfTimeState is null ? 0 : MaxRounds / 2;

	void IGameEventHandler<RoundCounterIncrementedEvent>.OnGameEvent( RoundCounterIncrementedEvent eventArgs )
	{
		if ( NextState is null )
		{
			return;
		}

		var round = GameMode.Instance.Get<RoundCounter>( true ).Round;

		if ( HalfTimeState is not null )
		{
			if ( round == FirstHalfRounds && !string.IsNullOrEmpty( FinalRoundFirstHalfMessage ) )
			{
				Toast.Instance.Show( FinalRoundFirstHalfMessage );
			}
			else if ( round == FirstHalfRounds + 1 )
			{
				GameMode.Instance.StateMachine.Transition( HalfTimeState );
			}
		}

		if ( NextState is not null )
		{
			if ( round == MaxRounds && !string.IsNullOrEmpty( FinalRoundMessage ) )
			{
				Toast.Instance.Show( FinalRoundMessage );
			}
			else if ( round == MaxRounds + 1 )
			{
				GameMode.Instance.StateMachine.Transition( NextState );
			}
		}
	}
}
