using System.Threading.Tasks;
using Facepunch;

/// <summary>
/// Called on the host when entering the <see cref="GameState.GameStart"/> state.
/// </summary>
public record PreGameStartEvent;

/// <summary>
/// Called on the host during the <see cref="GameState.GameStart"/> state.
/// </summary>
public record DuringGameStartEvent;

/// <summary>
/// Called on the host when leaving the <see cref="GameState.GameStart"/> state.
/// </summary>
public record PostGameStartEvent;

/// <summary>
/// Called on the host when entering the <see cref="GameState.RoundStart"/> state.
/// </summary>
public record PreRoundStartEvent;

/// <summary>
/// Called on the host during the <see cref="GameState.RoundStart"/> state.
/// </summary>
public record DuringRoundStartEvent;

/// <summary>
/// Called on the host when leaving the <see cref="GameState.RoundStart"/> state.
/// </summary>
public record PostRoundStartEvent;

/// <summary>
/// Called on the host during the <see cref="GameState.DuringRound"/> state.
/// </summary>
public record DuringRoundEvent;

/// <summary>
/// Called on the host when entering the <see cref="GameState.RoundEnd"/> state.
/// </summary>
public record PreRoundEndEvent;

/// <summary>
/// Called on the host during the <see cref="GameState.RoundEnd"/> state.
/// </summary>
public record DuringRoundEndEvent;

/// <summary>
/// Called on the host when leaving the <see cref="GameState.RoundEnd"/> state.
/// </summary>
public record PostRoundEndEvent;

/// <summary>
/// Called on the host when entering the <see cref="GameState.GameEnd"/> state.
/// </summary>
public record PreGameEndEvent;

/// <summary>
/// Called on the host during the <see cref="GameState.GameEnd"/> state.
/// </summary>
public record DuringGameEndEvent;

/// <summary>
/// Called on the host when leaving the <see cref="GameState.GameEnd"/> state.
/// </summary>
public record PostGameEndEvent;

public interface IPlayerSpawnListener
{
	/// <summary>
	/// Called on the host before <see cref="OnPlayerSpawn"/>.
	/// </summary>
	public void PrePlayerSpawn( PlayerController player ) { }

	/// <summary>
	/// Called on the host when <paramref name="player"/> respawns.
	/// </summary>
	public Task OnPlayerSpawn( PlayerController player ) => Task.CompletedTask;

	/// <summary>
	/// Called on the host after <see cref="OnPlayerSpawn"/>.
	/// </summary>
	public void PostPlayerSpawn( PlayerController player ) { }
}

public interface IPlayerJoinedListener
{
	/// <summary>
	/// Called on the host when a new player joins, before NetworkSpawn is called.
	/// </summary>
	public void OnConnect( PlayerController player ) { }

	/// <summary>
	/// Called on the host when a new player joins, after NetworkSpawn is called.
	/// </summary>
	public void OnJoined( PlayerController player ) { }
}

public interface ITeamAssignedListener
{
	/// <summary>
	/// Called on the host when a player is assigned to a team.
	/// </summary>
	public void OnTeamAssigned( PlayerController player, Team team ) { }
}

public interface ITeamSwapListener
{
	/// <summary>
	/// Called on the host when both teams swap.
	/// </summary>
	public void OnTeamSwap() { }
}
