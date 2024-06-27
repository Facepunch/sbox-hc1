using Sandbox.Diagnostics;

namespace Facepunch;

public partial class PlayerState
{
	private int _balance = 16_000;

	/// <summary>
	/// Players current cash balance
	/// </summary>
	[HostSync]
	public int Balance
	{
		get => GameMode.Instance?.UnlimitedMoney is true ? GameMode.Instance.MaxBalance : _balance;
		set => _balance = GameMode.Instance?.UnlimitedMoney is true ? GameMode.Instance.MaxBalance : value;
	}

	public void SetCash( int amount )
	{
		using var _ = Rpc.FilterInclude( Connection.Host );
		SetCashHost( amount );
	}

	[Broadcast]
	private void SetCashHost( int amount )
	{
		Assert.True( Networking.IsHost );
		Balance = Math.Clamp( amount, 0, GameMode.Instance.MaxBalance );
	}

	public void GiveCash( int amount )
	{
		using var _ = Rpc.FilterInclude( Connection.Host );
		GiveCashHost( amount );
	}

	[Broadcast]
	private void GiveCashHost( int amount )
	{
		Assert.True( Networking.IsHost );
		Balance = Math.Clamp( Balance + amount, 0, GameMode.Instance.MaxBalance );
	}
}
