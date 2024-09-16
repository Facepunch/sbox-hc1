using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Grants a unique Id to each player, generated on host and synced.
/// When a player disconnects that Id is returned to the pool and can be issued to new players. Issues an Id globally and per-team.
/// </summary>
public class PlayerId : Component, IGameEventHandler<TeamChangedEvent>
{
	[RequireComponent] PlayerState PlayerState { get; set; }

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
		if ( !Networking.IsHost )
			return;
		
		UniqueId = uniqueGenerator.Get();
	}

	public void TeamUpdate( Team before, Team after )
	{
		if ( !Networking.IsHost )
			return;

		teamGenerator[(int)before].Free( TeamUniqueId );
		TeamUniqueId = teamGenerator[(int)after].Get();
	}

	public void Free()
	{
		if ( !Networking.IsHost )
			return;

		uniqueGenerator.Free( UniqueId );
		teamGenerator[(int)PlayerState.Team].Free( TeamUniqueId );
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

	void IGameEventHandler<TeamChangedEvent>.OnGameEvent( TeamChangedEvent eventArgs )
	{
		TeamUpdate( eventArgs.Before, eventArgs.After );
	}
}
