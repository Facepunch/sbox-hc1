using Facepunch.UI;

namespace Facepunch;

partial class PlayerPawn
{
	/// <summary>
	/// Is the player holding use?
	/// </summary>
	[Sync] public bool IsUsing { get; set; }

	/// <summary>
	/// How far can we use stuff?
	/// </summary>
	[Property, Group( "Interaction" )] public float UseDistance { get; set; } = 72f;

	/// <summary>
	/// Which object did the player last press use on?
	/// </summary>
	public GameObject LastUsedObject { get; private set; }

	private void UpdateUse()
	{
		IsUsing = Input.Down( "Use" );

		if ( Input.Pressed( "Use" ) )
		{
			using ( Rpc.FilterInclude( Connection.Host ) )
			{
				TryUse( AimRay );
			}
		}
	}

	[Rpc.Owner]
	private void TryUse( Ray ray )
	{
		var hits = Scene.Trace.Ray( ray, UseDistance )
			.Size( 5f )
			.IgnoreGameObjectHierarchy( GameObject )
			.HitTriggers()
			.RunAll() ?? Array.Empty<SceneTraceResult>();

		var usable = hits
			.Select( x => x.GameObject.GetComponentInParent<IUse>() )
			.FirstOrDefault( x => x is not null );

		if ( usable.IsValid() && usable.CanUse( this ) is { } useResult )
		{
			if ( useResult.CanUse )
			{
				UpdateLastUsedObject( usable as Component );
				usable.OnUse( this );
			}
			else
			{
				if ( !string.IsNullOrEmpty( useResult.Reason ) )
				{
					using ( Rpc.FilterInclude( Network.Owner ) )
					{
						GameMode.Instance.ShowToast( useResult.Reason, ToastType.Generic, 3 );
					}
				}
			}

		}
		else if ( Team == Team.Terrorist && GetZone<BombSite>() is not null )
		{
			Inventory.SwitchToSlot( EquipmentSlot.Special );
			return;
		}
	}

	[Rpc.Broadcast( NetFlags.HostOnly )]
	private void UpdateLastUsedObject( Component component )
	{
		if ( !component.IsValid() )
		{
			return;
		}

		LastUsedObject = component.GameObject;
	}
}

