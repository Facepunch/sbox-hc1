using System.Collections.Generic;

namespace Sandbox.Events;

/// <summary>
/// Marks a <see cref="GameObject"/> as a state in a state machine. There must be a
/// <see cref="StateMachineComponent"/> on an ancestor object for this to function.
/// The object containing this state (and all ancestors) will be enabled when the state
/// machine transitions to this state, and will disable again when this state is exited.
/// States may be nested within each other.
/// </summary>
[Title( "State" ), Category( "State Machines" )]
public sealed class StateComponent : Component
{
	private StateMachineComponent? _stateMachine;

	/// <summary>
	/// Which state machine does this state belong to?
	/// </summary>
	public StateMachineComponent StateMachine =>
		_stateMachine ??= Components.GetInAncestorsOrSelf<StateMachineComponent>();

	/// <summary>
	/// Which state is this nested in, if any?
	/// </summary>
	public StateComponent? Parent => Components.GetInAncestors<StateComponent>( true );

	/// <summary>
	/// Transition to this state by default.
	/// </summary>
	[Property]
	public StateComponent? DefaultNextState { get; set; }

	/// <summary>
	/// If <see cref="DefaultNextState"/> is given, transition after this delay in seconds.
	/// </summary>
	[Property, HideIf( nameof( DefaultNextState ), null )]
	public float DefaultDuration { get; set; }

	/// <summary>
	/// Queue up a transition to the given state. This will occur at the end of
	/// a fixed update on the state machine.
	/// </summary>
	public void Transition( StateComponent next, float delaySeconds = 0f )
	{
		StateMachine.Transition( next, delaySeconds );
	}

	/// <summary>
	/// Queue up a transition to the default next state.
	/// </summary>
	public void Transition()
	{
		StateMachine.Transition( DefaultNextState! );
	}

	internal IReadOnlyList<StateComponent> GetAncestors()
	{
		var list = new List<StateComponent>();

		var parent = Parent;

		while ( parent != null )
		{
			list.Add( parent );
			parent = parent.Parent;
		}

		list.Reverse();

		return list;
	}
}

/// <summary>
/// Event dispatched on the host when a <see cref="StateMachineComponent"/> changes state.
/// Only invoked on components on the same object as the new state.
/// </summary>
public record EnterStateEventArgs( StateComponent State );

/// <summary>
/// Event dispatched on the host when a <see cref="StateMachineComponent"/> changes state.
/// Only invoked on components on the same object as the old state.
/// </summary>
public record LeaveStateEventArgs( StateComponent State );

/// <summary>
/// Event dispatched on the host every fixed update while a <see cref="StateComponent"/> is active.
/// Only invoked on components on the same object as the state.
/// </summary>
public record UpdateStateEventArgs( StateComponent State );
