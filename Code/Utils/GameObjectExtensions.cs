using Scene = Sandbox.Scene;

namespace Facepunch;

public static partial class GameObjectExtensions
{
	/// <summary>
	/// Take damage.
	/// </summary>
	/// <param name="go"></param>
	/// <param name="damageEvent"></param>
	public static void TakeDamage( this GameObject go, DamageEvent damageEvent )
	{
		foreach ( var damageable in go.Root.Components.GetAll<HealthComponent>() )
		{
			damageable.TakeDamage( damageEvent.Damage,
						 damageEvent.Position,
						 damageEvent.Force,
						 damageEvent.Attacker?.Id ?? default,
						 damageEvent.Inflictor?.Id ?? default,
						 damageEvent.Hitboxes,
						 damageEvent.Tags );
		}
	}

	public static void CopyPropertiesTo( this Component src, Component dst )
	{
		var json = src.Serialize().AsObject();

		json.Remove( "__guid" );

		Log.Info( json );

		dst.DeserializeImmediately( json );
	}

	public static string GetScenePath( this GameObject go )
	{
		return go is Scene ? "" : $"{go.Parent.GetScenePath()}/{go.Name}";
	}
}
