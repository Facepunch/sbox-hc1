using Sandbox.Events;

namespace Facepunch;

public enum Team
{
	Unassigned = 0,

	Terrorist,
	CounterTerrorist
}

public record TeamChangedEvent( Team Before, Team After ) : IGameEvent;

/// <summary>
/// Designates the team for a player.
/// </summary>
public partial class TeamComponent : Component
{

}

public static class TeamExtensions
{
	/// <summary>
	/// Accessor to get the team of someone/something.
	/// </summary>
	/// <param name="gameObject"></param>
	/// <returns></returns>
	public static Team GetTeam( this GameObject gameObject )
	{
		if ( gameObject.Root.Components.Get<Pawn>() is { IsValid: true } pawn )
		{
			return pawn.Team;
		}

		if ( gameObject.Root.Components.Get<PlayerState>() is { IsValid: true } state )
		{
			return state.Team;
		}

		return Team.Unassigned;
	}

	//
	// For all of this, maybe the gamemode should be controlling it, and not some global methods
	//

	/// <summary>
	/// Are we friendly with this other team?
	/// </summary>
	/// <param name="teamOne"></param>
	/// <param name="teamTwo"></param>
	/// <returns></returns>
	private static bool IsFriendly( Team teamOne, Team teamTwo )
	{
		if ( teamOne == Team.Unassigned || teamTwo == Team.Unassigned ) return false;
		return teamOne == teamTwo;
	}

	/// <summary>
	/// Are these two GameObjects friends with eachother?
	/// </summary>
	/// <param name="self"></param>
	/// <param name="other"></param>
	/// <returns></returns>
	public static bool IsFriendly( this GameObject self, GameObject other )
	{
		return IsFriendly( self.GetTeam(), other.GetTeam() );
	}

	/// <summary>
	/// Are these two <see cref="PlayerPawn"/>s friends with each other?
	/// </summary>
	public static bool IsFriendly( this PlayerPawn self, PlayerPawn other )
	{
		return IsFriendly( self.Team, other.Team );
	}

	/// <summary>
	/// Are these two <see cref="PlayerState"/>s friends with each other?
	/// </summary>
	public static bool IsFriendly( this PlayerState self, PlayerState other )
	{
		return IsFriendly( self.Team, other.Team );
	}

	public static string GetName( this Team team )
	{
		return team switch
		{
			Team.Terrorist => "Anarchists",
			Team.CounterTerrorist => "Security",
			_ => "Unassigned",
		};
	}

	private static Dictionary<Team, Color> teamColors = new()
	{
		{ Team.CounterTerrorist, new Color32( 5, 146, 235 ) },
		{ Team.Terrorist, new Color32( 233, 190, 92 ) },
		{ Team.Unassigned, new Color32( 255, 255, 255 ) },
	};

	public static Color GetColor( this Team team )
	{
		return teamColors[team];
	}

	public static string GetIconPath( this Team team )
	{
		return team switch
		{
			Team.CounterTerrorist => "/ui/teams/operators_logo.png",
			Team.Terrorist => "/ui/teams/anarchists_logo.png",
			_ => ""
		};
	}

	public static string GetBannerPath( this Team team )
	{
		return team switch
		{
			Team.CounterTerrorist => "/ui/teams/operators_logo_banner.png",
			Team.Terrorist => "/ui/teams/anarchists_logo_banner.png",
			_ => ""
		};
	}

	public static Team GetOpponents( this Team team )
	{
		return team switch
		{
			Team.CounterTerrorist => Team.Terrorist,
			Team.Terrorist => Team.CounterTerrorist,
			_ => Team.Unassigned
		};
	}
}
