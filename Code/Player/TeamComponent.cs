using Sandbox.Events;
using Sandbox.Utility;
using System.Runtime.CompilerServices;

namespace Facepunch;

/// <summary>
/// A component that has a team on it.
/// </summary>
public interface ITeam : IValid
{
	public TeamDefinition Team { get; set; }
}

public record TeamChangedEvent( TeamDefinition Before, TeamDefinition After ) : IGameEvent;

public static class TeamExtensions
{
	/// <summary>
	/// Accessor to get the team of someone/something.
	/// </summary>
	/// <param name="gameObject"></param>
	/// <returns></returns>
	public static TeamDefinition GetTeam( this GameObject gameObject )
	{
		if ( gameObject.Root.Components.Get<Pawn>() is { IsValid: true } pawn )
		{
			return pawn.Team;
		}

		if ( gameObject.Root.Components.Get<PlayerState>() is { IsValid: true } state )
		{
			return state.Team;
		}

		return null;
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
	private static bool IsFriendly( TeamDefinition teamOne, TeamDefinition teamTwo )
	{
		// TODO: Friendly team list in teams
		if ( teamOne is null || teamTwo is null ) return false;
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
		if ( !self.IsValid() || !other.IsValid() ) return false;
		return IsFriendly( self.GetTeam(), other.GetTeam() );
	}

	/// <summary>
	/// Are these two <see cref="PlayerPawn"/>s friends with each other?
	/// </summary>
	public static bool IsFriendly( this PlayerPawn self, PlayerPawn other )
	{
		if ( !self.IsValid() || !other.IsValid() ) return false;
		return IsFriendly( self.Team, other.Team );
	}

	/// <summary>
	/// Are these two <see cref="PlayerState"/>s friends with each other?
	/// </summary>
	public static bool IsFriendly( this PlayerState self, PlayerState other )
	{
		if ( !self.IsValid() || !other.IsValid() ) return false;
		return IsFriendly( self.Team, other.Team );
	}

	public static string GetName( this TeamDefinition team )
	{
		return team.Name;
	}

	public static Color GetColor( this TeamDefinition team )
	{
		if ( team is null ) return Color.White;

		return team.Color;
	}

	public static string GetIconPath( this TeamDefinition team )
	{
		return team.Icon;
	}

	public static string GetBannerPath( this TeamDefinition team )
	{
		return team.Banner;
	}

	public static TeamDefinition GetOpponents( this TeamDefinition team )
	{
		if ( team is null ) return null;

		// TODO: FIX THIS
		var security = ResourceLibrary.GetAll<TeamDefinition>().FirstOrDefault( x => x.Name == "Security" );
		var anarchists = ResourceLibrary.GetAll<TeamDefinition>().FirstOrDefault( x => x.Name == "Anarchists" );

		if ( team.Name == "Security" ) return anarchists;
		if ( team.Name == "Anarchists" ) return security;

		return null;
	}
}
