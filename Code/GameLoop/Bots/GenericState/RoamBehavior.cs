using Sandbox.Events;

namespace Facepunch;

public abstract class StateBehavior : Component,
	IGameEventHandler<UpdateStateEvent>,
	IGameEventHandler<LeaveStateEvent>,
	IGameEventHandler<EnterStateEvent>
{
	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		OnBehaviorStart();
	}

	void IGameEventHandler<LeaveStateEvent>.OnGameEvent( LeaveStateEvent eventArgs )
	{
		OnBehaviorEnd();
	}

	void IGameEventHandler<UpdateStateEvent>.OnGameEvent( UpdateStateEvent eventArgs )
	{
		OnBehaviorUpdate();
	}

	protected virtual void OnBehaviorStart()
	{
	}

	protected virtual void OnBehaviorEnd()
	{
	}

	protected abstract bool OnBehaviorUpdate();
}

public partial class RoamBehavior : StateBehavior
{
	protected override bool OnBehaviorUpdate()
	{
		Log.Info( $"roam forever" );
		return false;
	}

	protected override void OnBehaviorStart()
	{
		Log.Info( $"Start roaming" );
	}
	protected override void OnBehaviorEnd()
	{
		Log.Info( $"Stop roaming" );
	}
}
