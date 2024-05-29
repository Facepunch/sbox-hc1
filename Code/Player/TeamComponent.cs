using Sandbox.Diagnostics;

namespace Facepunch;

public enum Team
{
	Unassigned = 0,
	Terrorist,
	CounterTerrorist
}

public static class TeamExtensionMethods
{
	public static string GetName( this Team team )
	{
		return team switch
		{
			Team.Terrorist => "Terrorists",
			Team.CounterTerrorist => "Counter-Terrorists",
			_ => "Spectators",
		};
	}

	public static Color GetColor( this Team team )
	{
		return team switch
		{
			Team.CounterTerrorist => Color.Parse( "#00B5EB" ) ?? default,
			Team.Terrorist => Color.Parse( "#EB4C00" ) ?? default,
			_ => Color.Parse( "white" ) ?? default
		};
	}

	public static string GetIconPath( this Team team )
	{
		return team switch
		{
			Team.CounterTerrorist => "/ui/teams/counterterrorists.png",
			Team.Terrorist => "/ui/teams/terrorists.png",
			_ => "/ui/teams/spectator.png"
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

/// <summary>
/// Designates the team for a player.
/// </summary>
public partial class TeamComponent : Component
{
	/// <summary>
	/// An action (for ActionGraph, or other components) to listen for team changes
	/// </summary>
	[Property, Group( "Actions" )] public Action<Team, Team> OnTeamChanged { get; set; }

	private Team InternalTeam;

	/// <summary>
	/// The team this player is on.
	/// </summary>
	[Property, HostSync( Query = true ), Group( "Setup" )]
	public Team Team
	{
		get => InternalTeam;
		set
		{
			if ( InternalTeam == value )
				return;

			var before = InternalTeam;
			InternalTeam = value;
			TeamChanged( before, InternalTeam );
		}
	}

	/// <summary>
	/// Called when <see cref="Team"/> changes
	/// </summary>
	/// <param name="before"></param>
	/// <param name="after"></param>
	private void TeamChanged( Team before, Team after )
	{
		OnTeamChanged?.Invoke( before, after );
	}
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
		var comp = gameObject.Components.Get<TeamComponent>( FindMode.EverythingInSelfAndAncestors );
		if ( !comp.IsValid() ) return Team.Unassigned;

		return comp.Team;
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
	/// Are these two <see cref="PlayerController"/>s friends with each other?
	/// </summary>
	public static bool IsFriendly( this PlayerController self, PlayerController other )
	{
		return IsFriendly( self.TeamComponent.Team, other.TeamComponent.Team );
	}
}
