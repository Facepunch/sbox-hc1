using Facepunch.UI;
using Facepunch;
using System.Threading.Tasks;

/// <summary>
/// Keep the players frozen for a few seconds at the start of each round.
/// </summary>
public sealed class FreezeTime : Component, IRoundStartListener
{
	[Property, Sync]
	public float DurationSeconds { get; set; } = 15f;

	[Sync]
	public float StartTime { get; set; }

	void IRoundStartListener.PreRoundStart()
	{
		foreach ( var player in GameUtils.ActivePlayers )
		{
			player.IsFrozen = true;
		}
	}

	void IRoundStartListener.PostRoundStart()
	{
		foreach ( var player in GameUtils.ActivePlayers )
		{
			player.IsFrozen = false;
		}
	}

	async Task IRoundStartListener.OnRoundStart()
	{
		StartTime = Time.Now;

		GameMode.Instance.ShowCountDownTimer( StartTime, DurationSeconds );
		GameMode.Instance.ShowStatusText( "PREPARE" );

		while ( Time.Now < StartTime + DurationSeconds )
		{
			await Task.FixedUpdate();
		}

		GameMode.Instance.HideStatusText();
	}
}
