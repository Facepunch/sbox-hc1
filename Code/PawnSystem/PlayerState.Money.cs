using Facepunch.UI;
using Sandbox.Diagnostics;

namespace Facepunch;

public enum BuyMenuMode
{
	Disabled,
	EnabledInBuyZone,
	EnabledEverywhere
}

public partial class PlayerState : IScore
{
	private int _balance = 16_000;

	/// <summary>
	/// Players current cash balance
	/// </summary>
	[HostSync, Order( -100 ), Score( "Balance", Format = "${0:N0}", ShowIf = nameof( ShouldShowBalance ) )]
	public int Balance
	{
		get => GameMode.Instance?.UnlimitedMoney is true ? GameMode.Instance.MaxBalance : _balance;
		set => _balance = GameMode.Instance?.UnlimitedMoney is true ? GameMode.Instance.MaxBalance : value;
	}

	[HostSync]
	public BuyMenuMode BuyMenuMode { get; set; }

	private bool ShouldShowBalance()
	{
		return GameMode.Instance?.UnlimitedMoney is not true;
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
