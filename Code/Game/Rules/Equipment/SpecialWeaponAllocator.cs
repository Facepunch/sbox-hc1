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

	void IRoundStartListener.PreRoundStart()
	{
		if ( Weapon is null )
		{
			return;
		}

		var players = GameUtils.GetPlayers( Team ).Shuffle();

		if ( players.Count == 0 )
		{
			return;
		}

		players[0].Inventory.GiveWeapon( Weapon, false );
	}
}
