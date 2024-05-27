using System.Collections.Immutable;

namespace Gunfight;

/// <summary>
/// We hold a separate list of tags for the player.
/// This might be absolutely useless, and we should just use regular tags.
/// </summary>
public partial class PlayerController
{
	private ImmutableArray<string> tags = ImmutableArray.Create<string>();

	/// <summary>
	/// Do we have a tag?
	/// </summary>
	/// <param name="tag"></param>
	/// <returns></returns>
	public bool HasTag( string tag )
	{
		return tags.Contains( tag );
	}

	/// <summary>
	/// Do we have any tag?
	/// </summary>
	/// <param name="tags"></param>
	/// <returns></returns>
	public bool HasAnyTag( params string[] tags )
	{
		foreach ( var tag in tags )
		{
			if ( HasTag( tag ) )
				return true;
		}

		return false;
	}

	/// <summary>
	/// Do we have all tags?
	/// </summary>
	/// <param name="tags"></param>
	/// <returns></returns>
	public bool HasAllTags( params string[] tags )
	{
		foreach ( var tag in tags )
		{
			if ( !HasTag( tag ) )
				return false;
		}

		return true;
	}
}
