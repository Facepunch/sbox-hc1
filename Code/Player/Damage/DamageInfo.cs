using System.Numerics;

namespace Facepunch;

/// <summary>
/// A damage info struct. When inflicting damage on GameObjects, this is what we'll pass around.
/// </summary>
public partial class DamageInfo : Sandbox.DamageInfo
{
	/// <summary>
	/// The inflictor. This is normally the same as the attacker,
	/// though it lets us pass another responsible GameObject, such as a weapon, or a projectile.
	/// </summary>
	public GameObject Inflictor { get; set; }

	/// <summary>
	/// A list of tags that we can parse when taking damage.
	/// </summary>
	public string[] Tags { get; set; }

	/// <summary>
	/// Include some tags.
	/// </summary>
	/// <param name="tags"></param>
	public void WithTags( params string[] tags )
	{
		Tags = tags;
	}

	/// <summary>
	/// Do we have a tag?
	/// </summary>
	/// <param name="tag"></param>
	/// <returns></returns>
	public bool HasTag( string tag )
	{
		return Tags?.Contains( tag ) ?? false;
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

	/// <summary>
	/// Include an attacker.
	/// </summary>
	/// <param name="attacker"></param>
	public void WithAttacker( GameObject attacker )
	{
		Attacker = attacker;
	}

	/// <summary>
	/// Include an inflictor.
	/// </summary>
	/// <param name="inflictor"></param>
	public void WithInflictor( GameObject inflictor )
	{
		Inflictor = inflictor;
	}
}
