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
		{
			seed = Random.Shared.Next( int.MaxValue );
		}

		Colors = colors.Shuffle( new Random( seed ) ).ToArray();
	}

	public Color GetColor( PlayerPawn player )
	{
		var team = player.Team;
		if ( team == Team.Unassigned )
			return Color.White;

		// todo: make this a gamemode setting?
		bool bigTeamMode = GameUtils.GetPlayers( team ).Count() > Colors.Length;

		int idx = player.PlayerState.PlayerId.TeamUniqueId;
		if ( bigTeamMode || idx == -1 || idx >= Colors.Length )
			return team.GetColor();

		return Colors[idx];
	}
}
