namespace Facepunch;

/// <summary>
/// A simple component that destroys its GameObject.
/// </summary>
public sealed class TimedDestroyComponent : Component
{
	/// <summary>
	/// How long until we destroy the GameObject.
	/// </summary>
	[Property] public float Time { get; set; } = 1f;

	/// <summary>
	/// The real time until we destroy the GameObject.
	/// </summary>
	[Property, ReadOnly] TimeUntil TimeUntilDestroy { get; set; } = 0;

	[Property]
	public bool WaitForChildEffects = false;

	protected override void OnStart()
	{
		TimeUntilDestroy = Time;
	}

	bool HasActiveEffects()
	{
		foreach ( var pe in GetComponentsInChildren<ITemporaryEffect>() )
		{
			if ( pe.IsActive )
				return true;
		}

		return false;
	}

	protected override void OnUpdate()
	{
		if ( WaitForChildEffects && HasActiveEffects() )
		{
			return;
		}

		if ( TimeUntilDestroy )
		{
			GameObject.Destroy();
		}
	}
}

public static partial class GameObjectExtensions
{
	/// <summary>
	/// Creates a <see cref="TimedDestroyComponent"/> which will deferred delete the <see cref="GameObject"/>.
	/// </summary>
	/// <param name="self"></param>
	/// <param name="seconds"></param>
	public static void DestroyAsync( this GameObject self, float seconds = 1.0f )
	{
		var component = self.Components.Create<TimedDestroyComponent>();
		component.Time = seconds;
	}
}
