using System.Threading.Tasks;
using Facepunch;

/// <summary>
/// Gives a special weapon to one player on the specified team.
/// </summary>
public sealed class SpecialWeaponAllocator : Component, IRoundStartListener, IPlayerSpawnListener
{
	/// <summary>
	/// We'll give this weapon to one player on the specified team.
	/// </summary>
	[Property]
	public WeaponData Weapon { get; set; }

	/// <summary>
	/// Which team to give the special weapon to.
	/// </summary>
	[Property]
	public Team Team { get; set; }

	Task IRoundStartListener.OnRoundStart()
	{
		if ( Weapon is null )
		{
			return Task.CompletedTask;
		}

		var players = GameUtils.GetPlayers( Team ).Shuffle();

		if ( players.Count == 0 )
		{
			return Task.CompletedTask;
		}

		Log.Info( $"Trying to spawn {Weapon} on {players[0]}" );

		var weapon = players[0].Inventory.GiveWeapon( Weapon, false );

		if ( weapon is not null )
		{
			weapon.Components.GetOrCreate<DestroyBetweenRounds>();
		}

		return Task.CompletedTask;
	}
}
