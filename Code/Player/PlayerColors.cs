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
		var team = player.Team;
		if ( team == Team.Unassigned ) return Color.White;
		
		var bigTeamMode = GameUtils.GetPlayerPawns( team ).Count() > Colors.Length;
		var idx = player.PlayerId.TeamUniqueId;
		if ( bigTeamMode || idx == -1 || idx >= Colors.Length )
			return team.GetColor();

		return Colors[idx];
	}
}
