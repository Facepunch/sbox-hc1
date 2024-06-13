using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// A pawn might have armor, which reduces damage.
/// </summary>
public partial class ArmorComponent : Component, IGameEventHandler<ModifyDamageEvent>
{
	[Property, ReadOnly, HostSync]
	public float Armor { get; set; }

	public float MaxArmor => GetGlobal<PlayerGlobals>().MaxArmor;

	[Property, ReadOnly, HostSync, Change( nameof( OnHasHelmetChanged ) )]
	public bool HasHelmet { get; set; }

	protected void OnHasHelmetChanged( bool _, bool newValue )
	{
		GameObject.Root.Dispatch( new HelmetChangedEvent( newValue ) );
	}

	[DeveloperCommand( "Toggle Kevlar + Helmet", "Player" )]
	private static void Dev_ToggleKevlarAndHelmet()
	{
		var player = GameUtils.LocalPlayer;
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
	void IGameEventHandler<ModifyDamageEvent>.OnGameEvent( ModifyDamageEvent eventArgs )
	{
		if ( Armor > 0f ) eventArgs.AddFlag( DamageFlags.Armor );
		if ( HasHelmet ) eventArgs.AddFlag( DamageFlags.Helmet );
	}
}

public record HelmetChangedEvent( bool HasHelmet );
