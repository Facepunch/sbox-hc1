namespace Facepunch;

public static partial class GameObjectExtensions
{
	/// <summary>
	/// Take damage.
	/// </summary>
	/// <param name="go"></param>
	/// <param name="info"></param>
	public static void TakeDamage( this GameObject go, ref DamageInfo info )
	{
		foreach ( var damageable in go.Root.Components.GetAll<HealthComponent>( FindMode.EnabledInSelfAndDescendants ) )
		{
			damageable.TakeDamage( info.Damage, info.Position, default, info.Attacker?.Id ?? default );
		}

		go.Scene.GetSystem<GameEventSystem>().OnDamageGivenEvent?.Invoke( go, info );
	}
}
