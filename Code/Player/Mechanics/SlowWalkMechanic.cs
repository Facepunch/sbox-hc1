namespace Facepunch;

public partial class SlowWalkMechanic : BasePlayerControllerMechanic
{
    public override bool ShouldBecomeActive()
    {
        return Input.Down( "Run" ) && !HasAnyTag( "crouch" );
    }

    public override IEnumerable<string> GetTags()
    {
        yield return "slow_walk";
    }

    public override float? GetSpeed()
    {
        return 70.0f;
    }
}
