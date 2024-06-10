namespace Facepunch;

/// <summary>
/// Grants a unique Id to each player, generated on host and synced.
/// When a player disconnects that Id is returned to the pool and can be issued to new players. Issues an Id globally and per-team.
/// </summary>
public class PlayerId : Component
{
	private struct PlayerIdGenerator
	{
		private Stack<int> freeIds;
		private int maxId;

		public PlayerIdGenerator()
		{
			freeIds = new Stack<int>();
			maxId = 0;
		}

		public int Get()
		{
			int id;
			if ( freeIds.TryPop( out id ) )
			{
				return id;
			}

			id = maxId;
			maxId++;
			return id;
		}

		public void Free(int id)
		{
			if (id != -1)
				freeIds.Push( id );
		}
	}
	private static PlayerIdGenerator uniqueGenerator;
	private static PlayerIdGenerator[] teamGenerator;

	[RequireComponent] TeamComponent TeamComponent { get; set; }

	/// <summary>
	/// Unique Id of this player in the game. New players will occupy vacant ids.
	/// </summary>
	[HostSync] public int UniqueId { get; private set; } = -1;

	/// <summary>
	/// Unique Id of this player within their team. New players will occupy vacant ids.
	/// </summary>
	[HostSync] public int TeamUniqueId { get; private set; } = -1;

	protected override void OnAwake()
	{
		base.OnAwake();

		if ( !Networking.IsHost )
			return;

		TeamComponent.OnTeamChanged += OnTeamChanged;
		UniqueId = uniqueGenerator.Get();
	}

	private void OnTeamChanged( Team before, Team after )
	{
		teamGenerator[(int)before].Free(TeamUniqueId);
		TeamUniqueId = teamGenerator[(int)after].Get();
	}

	protected override void OnDestroy()
	{
		if ( !Networking.IsHost )
			return;

		TeamComponent.OnTeamChanged -= OnTeamChanged;
		uniqueGenerator.Free( UniqueId );
		teamGenerator[(int)TeamComponent.Team].Free( TeamUniqueId );
	}

	public static void Init()
	{
		uniqueGenerator = new();

		var teams = Enum.GetValues<Team>();
		teamGenerator = new PlayerIdGenerator[teams.Count()];
		for ( int i = 0; i < teams.Count(); i++ )
		{
			teamGenerator[i] = new();
		}
	}
}
