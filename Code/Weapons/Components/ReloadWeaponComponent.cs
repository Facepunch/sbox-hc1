using Sandbox.Events;

namespace Facepunch;

[Title( "Reload" ), Group( "Weapon Components" )]
public partial class ReloadWeaponComponent : InputWeaponComponent,
	IGameEventHandler<EquipmentHolsteredEvent>
{
	/// <summary>
	/// How long does it take to reload?
	/// </summary>
	[Property] public float ReloadTime { get; set; } = 1.0f;

	/// <summary>
	/// How long does it take to reload while empty?
	/// </summary>
	[Property] public float EmptyReloadTime { get; set; } = 2.0f;
	
	[Property] public bool SingleReload { get; set; } = false;

	/// <summary>
	/// This is really just the magazine for the weapon. 
	/// </summary>
	[Property] public AmmoComponent AmmoComponent { get; set; }

	private TimeUntil TimeUntilReload { get; set; }
	[Sync] private bool IsReloading { get; set; }

	protected override void OnEnabled()
	{
		BindTag( "reloading", () => IsReloading );
	}

	protected override void OnInput()
	{
		if ( CanReload() )
		{
			StartReload();
		}
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		if ( SingleReload && IsReloading && Input.Pressed( "Attack1" ) )
		{
			_queueCancel = true;
		}

		if ( IsReloading && TimeUntilReload )
		{
			EndReload();
		}
	}

	void IGameEventHandler<EquipmentHolsteredEvent>.OnGameEvent( EquipmentHolsteredEvent eventArgs )
	{
		if ( !IsProxy && IsReloading )
		{
			// Conna: if we're no longer deployed cancel reloading.
			CancelReload();
		}
	}

	bool CanReload()
	{
		return !IsReloading && AmmoComponent.IsValid() && !AmmoComponent.IsFull;
	}

	float GetReloadTime()
	{
		if ( !AmmoComponent.HasAmmo ) return EmptyReloadTime;
		return ReloadTime;
	}

	Dictionary<float, SoundEvent> GetReloadSounds()
	{
		if ( !AmmoComponent.HasAmmo ) return EmptyReloadSounds;
		return TimedReloadSounds;
	}

	bool _queueCancel = false;

	[Broadcast( NetPermission.OwnerOnly )]
	void StartReload()
	{
		_queueCancel = false;

		if ( !IsProxy )
		{
			IsReloading = true;
			TimeUntilReload = GetReloadTime();
		}

		// Tags will be better so we can just react to stimuli.
		Equipment.ViewModel?.ModelRenderer.Set( "b_reload", true );
		Equipment.Owner?.BodyRenderer?.Set( "b_reload", true );

		foreach ( var kv in GetReloadSounds() )
		{
			// Play this sound after a certain time but only if we're reloading.
			PlayAsyncSound( kv.Key, kv.Value, () => IsReloading );
		}
	}
	
	[Broadcast( NetPermission.OwnerOnly )]
	void CancelReload()
	{
		if ( !IsProxy )
			IsReloading = false;

		// Tags will be better so we can just react to stimuli.
		Equipment.ViewModel?.ModelRenderer.Set( "b_reload", false );
		Equipment.Owner?.BodyRenderer.Set( "b_reload", false );
	}

	[Broadcast( NetPermission.OwnerOnly )]
	void EndReload()
	{
		if ( !IsProxy )
		{
			if ( SingleReload )
			{
				AmmoComponent.Ammo++;
				AmmoComponent.Ammo = AmmoComponent.Ammo.Clamp( 0, AmmoComponent.MaxAmmo );

				// Reload more!
				if ( !_queueCancel && AmmoComponent.Ammo < AmmoComponent.MaxAmmo )
					StartReload();
				else
					IsReloading = false;
			}
			else
			{
				IsReloading = false;
				// Refill the ammo container.
				AmmoComponent.Ammo = AmmoComponent.MaxAmmo;
			}
		}

		// Tags will be better so we can just react to stimuli.
		Equipment.ViewModel?.ModelRenderer.Set( "b_reload", false );
	}

	[Property] public Dictionary<float, SoundEvent> TimedReloadSounds { get; set; } = new();
	[Property] public Dictionary<float, SoundEvent> EmptyReloadSounds { get; set; } = new();

	async void PlayAsyncSound( float delay, SoundEvent snd, Func<bool> playCondition = null )
	{
		await GameTask.DelaySeconds( delay );
		
		// Can we play this sound?
		if ( playCondition != null && !playCondition.Invoke() )
			return;
		
		GameObject.PlaySound( snd );
	}
}
