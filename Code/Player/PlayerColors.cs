using Facepunch;
using Sandbox.Diagnostics;

internal class PlayerColors : SingletonComponent<PlayerColors>
{
	[HostSync] private int seed { get; set; }

	private static readonly Color[] colors = new Color[]
	{
		Color.Orange,
		Color.Green,
		Color.Cyan,
		Color.Yellow,
		Color.Magenta
	};
	private Color[] Colors;

	protected override void OnAwake()
	{
		base.OnAwake();

		if ( Networking.IsHost )
			seed = Random.Shared.Next( int.MaxValue );

		Colors = colors.Shuffle( new Random( seed ) ).ToArray();
	}
	
	public Color GetColor( PlayerState player )
	{
		return player.Team?.GetColor() ?? Color.White;
	}
}
