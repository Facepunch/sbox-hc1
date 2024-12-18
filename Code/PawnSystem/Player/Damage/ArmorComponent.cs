using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// A pawn might have armor, which reduces damage.
/// </summary>
public partial class ArmorComponent : Component, IGameEventHandler<ModifyDamageTakenEvent>
{
	[Property, ReadOnly, Sync( SyncFlags.FromHost )]
	public float Armor { get; set; }

	public float MaxArmor => GetGlobal<PlayerGlobals>().MaxArmor;

	[Property, ReadOnly, Sync( SyncFlags.FromHost ), Change( nameof( OnHasHelmetChanged ) )]
	public bool HasHelmet { get; set; }

	protected void OnHasHelmetChanged( bool _, bool newValue )
	{
		GameObject.Root.Dispatch( new HelmetChangedEvent( newValue ) );
	}

	[DeveloperCommand( "Toggle Kevlar + Helmet", "Player" )]
	private static void Dev_ToggleKevlarAndHelmet()
	{
		var player = PlayerState.Local.PlayerPawn;
		if ( player is null ) return;

		if ( player.ArmorComponent.HasHelmet )
		{
			player.ArmorComponent.Armor = 0;
			player.ArmorComponent.HasHelmet = false;
		}
		else
		{
			player.ArmorComponent.Armor = player.ArmorComponent.MaxArmor;
			player.ArmorComponent.HasHelmet = true;
		}
	}

	[Early]
	void IGameEventHandler<ModifyDamageTakenEvent>.OnGameEvent( ModifyDamageTakenEvent eventArgs )
	{
		if ( Armor > 0f ) eventArgs.AddFlag( DamageFlags.Armor );
		if ( HasHelmet ) eventArgs.AddFlag( DamageFlags.Helmet );
	}
}

public record HelmetChangedEvent( bool HasHelmet ) : IGameEvent;
