namespace Gunfight;

/// <summary>
/// A base for a player controller mechanic.
/// </summary>
public abstract partial class BasePlayerControllerMechanic : Component
{
	[Property, Category( "Base" )] public PlayerController PlayerController { get; set; }

	/// <summary>
	/// A priority for the controller mechanic.
	/// </summary>
	[Property, Category( "Base" )] public virtual int Priority { get; set; } = 0;

	/// <summary>
	/// How long since <see cref="IsActive"/> changed.
	/// </summary>
	[Property, Category( "Base" ), ReadOnly] protected TimeSince TimeSinceActiveChanged { get; set; }

	private bool isActive; 

	/// <summary>
	/// Is this mechanic active?
	/// </summary>
	[Sync( Query = true )] public bool IsActive
	{
		get => isActive;
		set
		{
			var before = isActive;
			isActive = value;

			if ( isActive != before )
			{
				TimeSinceActiveChanged = 0;
				OnActiveChanged( before, isActive );
			}
		}
	}

	protected override void OnAwake()
	{
		// If we don't have the player controller defined, let's have a look for it
		if ( !PlayerController.IsValid() )
		{
			PlayerController = Components.Get<PlayerController>( FindMode.EverythingInSelfAndAncestors );
		}
	}

	/// <summary>
	/// Return a list of tags to be used by the player controller / other mechanics.
	/// </summary>
	/// <returns></returns>
	public virtual IEnumerable<string> GetTags()
	{
		return Enumerable.Empty<string>();
	}
	
	/// <summary>
	/// An accessor to see if the player controller has a tag.
	/// </summary>
	/// <param name="tag"></param>
	/// <returns></returns>
	public bool HasTag( string tag ) => PlayerController.HasTag( tag );

	/// <summary>
	/// An accessor to see if the player controller has all matched tags.
	/// </summary>
	/// <param name="tags"></param>
	/// <returns></returns>
	public bool HasAllTags( params string[] tags ) => PlayerController.HasAllTags( tags );

	/// <summary>
	/// An accessor to see if the player controller has any tag.
	/// </summary>
	/// <param name="tags"></param>
	/// <returns></returns>
	public bool HasAnyTag( params string[] tags ) => PlayerController.HasAnyTag( tags );

	/// <summary>
	/// Called when <see cref="IsActive"/> changes.
	/// </summary>
	/// <param name="before"></param>
	/// <param name="after"></param>
	protected virtual void OnActiveChanged( bool before, bool after )
	{
		//
	}

	/// <summary>
	/// Called by <see cref="PlayerController"/>, treat this like a Tick/Update while the mechanic is active.
	/// </summary>
	public virtual void OnActiveUpdate()
	{
		//
	}

	/// <summary>
	/// Should we be ticking this mechanic at all?
	/// </summary>
	/// <returns></returns>
	public virtual bool ShouldBecomeActive()
	{
		return false;
	}

	/// <summary>
	/// Should we be inactive?
	/// </summary>
	/// <returns></returns>
	public virtual bool ShouldBecomeInactive()
	{
		return !ShouldBecomeActive();
	}

	/// <summary>
	/// Mechanics can override the player's movement speed.
	/// </summary>
	/// <returns></returns>
	public virtual float? GetSpeed()
	{
		return null;
	}

	/// <summary>
	/// Mechanics can override the player's eye height.
	/// </summary>
	/// <returns></returns>
	public virtual float? GetEyeHeight()
	{
		return null;
	}

	/// <summary>
	/// Mechanics can override the player's ground friction.
	/// </summary>
	public virtual float? GetGroundFriction()
	{
		return null;
	}

	/// <summary>
	/// Mechanics can override the player's acceleration.
	/// </summary>
	/// <returns></returns>
	public virtual float? GetAcceleration()
	{
		return null;
	}

	/// <summary>
	/// Mechanics can override the player's wish input direction.
	/// </summary>
	public virtual void BuildWishInput( ref Vector3 wish )
	{
	}
}
