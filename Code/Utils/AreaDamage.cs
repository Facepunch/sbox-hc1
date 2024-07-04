namespace Facepunch;

public interface IAreaDamageReceiver : IValid
{
	Guid Id { get; }
	GameObject GameObject { get; }
	void ApplyAreaDamage( AreaDamage component );
}

[Title( "Area Damage" )]
public class AreaDamage : Component, Component.ITriggerListener
{
	private class AreaDamageTarget
	{
		public IAreaDamageReceiver Receiver { get; init; }
		public TimeUntil NextDamageTime { get; set; }
		public TimeSince LastDamageTime { get; set; }
	}

	private Dictionary<Guid, AreaDamageTarget> Targets { get; set; } = new();
	private HashSet<Guid> TargetsToRemove { get; set; } = new();

	/// <summary>
	/// How much damage to deal each interval.
	/// </summary>
	[Property]
	public float Damage { get; set; } = 10f;

	/// <summary>
	/// How often to deal damage while colliding (in seconds.)
	/// </summary>
	[Property]
	public float Interval { get; set; } = 0.5f;
	
	/// <summary>
	/// Ignore any <see cref="IAreaDamageReceiver"/> targets with any of these tags.
	/// </summary>
	[Property] public TagSet IgnoreTags { get; set; }

	[Property] public DamageFlags DamageFlags { get; set; } = DamageFlags.None;
	
	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		if ( !Networking.IsHost ) return;
		
		var receiver = other.GameObject?.Root.Components.GetInDescendantsOrSelf<IAreaDamageReceiver>();
		if ( !receiver.IsValid() ) return;

		if ( IgnoreTags is not null && receiver.GameObject.Tags.HasAny( IgnoreTags ) )
			return;

		if ( !Targets.TryGetValue( receiver.Id, out var target ) )
		{
			target = new() { Receiver = receiver, NextDamageTime = 0f };
			Targets[receiver.Id] = target;
		}
		
		if ( !target.NextDamageTime )
			return;
			
		target.LastDamageTime = 0f;
		target.NextDamageTime = Interval;
		target.Receiver.ApplyAreaDamage( this );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( !Networking.IsHost )
			return;

		foreach ( var ( id, target ) in Targets )
		{
			if ( target.Receiver.IsValid() && target.LastDamageTime <= Interval * 2f )
				continue;

			TargetsToRemove.Add( id );
		}

		foreach ( var id in TargetsToRemove )
		{
			Targets.Remove( id );
		}
		
		TargetsToRemove.Clear();
	}
}
