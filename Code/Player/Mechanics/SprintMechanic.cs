namespace Gunfight;

/// <summary>
/// A sprinting mechanic.
/// </summary>
public partial class SprintMechanic : BasePlayerControllerMechanic
{
	public override bool ShouldBecomeActive()
	{
		if ( !PlayerController.IsGrounded ) return false;

		var wish = PlayerController.WishMove;

		// Can't sprint backward
		if ( wish.x < 0.0f ) return false;

		// Don't sprint if we're not moving
		if ( wish.Length.AlmostEqual( 0 ) ) return false;

		// Don't sprint if we're only moving horizontally
		if ( wish.y != 0.0f && wish.x.AlmostEqual( 0 ) ) return false;

		return Input.Down( "Run" );
	}

	public override IEnumerable<string> GetTags()
	{
		yield return "sprint";
		yield return "no_aiming";
	}

	public override void BuildWishInput( ref Vector3 wish )
	{
		// Clamp the horizontal wish direction by half if we're sprinting
		wish.y *= 0.5f;
	}

	public override float? GetSpeed()
	{
		return 300.0f;
	}
}
