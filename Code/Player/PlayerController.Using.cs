namespace Facepunch;

partial class PlayerController
{
	/// <summary>
	/// Is the player holding use?
	/// </summary>
	[Sync] public bool IsUsing { get; set; }

	/// <summary>
	/// Which object did the player last press use on?
	/// </summary>
	public GameObject LastUsedObject { get; private set; }

	private void UpdateUse()
	{
		if ( IsLocallyControlled )
		{
			IsUsing = Input.Down( "Use" );

			if ( Input.Pressed( "Use" ) )
			{
				TryUse( AimRay.Position, AimRay.Forward );
			}
		}
	}

	[Broadcast( NetPermission.OwnerOnly )]
	private void TryUse( Vector3 pos, Vector3 forward )
	{
		var hits = Scene.Trace.Ray( pos, forward * UseDistance )
			.Size( 5f )
			.IgnoreGameObjectHierarchy( GameObject )
			.HitTriggers()
			.RunAll() ?? Array.Empty<SceneTraceResult>();

		var usable = hits
			.Select( x => x.GameObject.Components.Get<IUse>( FindMode.EnabledInSelf | FindMode.InAncestors ) )
			.FirstOrDefault( x => x is not null );

		if ( usable.IsValid() && usable.CanUse( this ) )
		{
			UpdateLastUsedObject( (usable as Component)?.GameObject.Id ?? Guid.Empty );
			usable.OnUse( this );
		}
	}

	[Broadcast( NetPermission.HostOnly )]
	private void UpdateLastUsedObject( Guid id )
	{
		LastUsedObject = Scene.Directory.FindByGuid( id );
	}
}

